using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HotelPro.Infrastructure.Services;

public class InvoiceGenerator : IInvoiceGenerator
{
    private readonly HotelProDbContext _dbContext;

    public InvoiceGenerator(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<InvoiceDetailDto> GenerateInvoiceAsync(Guid bookingId, string? currency = null)
    {
        var booking = await _dbContext.Bookings
            .IgnoreQueryFilters()
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms).ThenInclude(br => br.RoomType)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new InvalidOperationException($"Booking {bookingId} not found");

        var invoiceNumber = await GetNextInvoiceNumberAsync();
        var nights = Math.Max(1, (booking.DepartureDate - booking.ArrivalDate).Days);
        var curr = currency ?? booking.Currency;

        var items = booking.BookingRooms.Select(br => new InvoiceItemDto(
            $"{br.RoomType?.Name ?? "Room"} - {nights}n",
            1,
            br.PricePerNight * nights,
            br.PricePerNight * nights,
            25m,
            (br.PricePerNight * nights) * 0.25m
        )).ToList();

        var subtotal = items.Sum(i => i.TotalPrice);
        var vatAmount = items.Sum(i => i.VatAmount);
        var total = subtotal + vatAmount;

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            FolioId = Guid.Empty,
            GuestId = booking.GuestId,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            TotalNet = subtotal,
            TotalVat = vatAmount,
            TotalGross = total,
            Status = InvoiceStatus.Sent,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        return new InvoiceDetailDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.IssueDate,
            $"{booking.Guest?.FirstName} {booking.Guest?.LastName}",
            booking.Guest?.Address,
            subtotal,
            vatAmount,
            total,
            curr,
            invoice.Status.ToString(),
            false,
            null,
            items
        );
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Guest)
            .FirstOrDefaultAsync(i => i.Id == invoiceId)
            ?? throw new InvalidOperationException($"Invoice {invoiceId} not found");

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontFamily("Arial"));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("HotelPRO").FontSize(20).Bold();
                        c.Item().Text("Adresa hotela").FontSize(9);
                        c.Item().Text("VAT: HR123456789").FontSize(8);
                    });
                    row.ConstantItem(150).Column(c =>
                    {
                        c.Item().AlignRight().Text($"RACUN #{invoice.InvoiceNumber}").FontSize(14).Bold();
                        c.Item().AlignRight().Text(invoice.IssueDate.ToString("dd.MM.yyyy.")).FontSize(10);
                    });
                });

                page.Content().PaddingVertical(20).Column(c =>
                {
                    c.Item().Text($"Gost: {invoice.Guest?.FirstName} {invoice.Guest?.LastName}").FontSize(10);
                    c.Item().Text($"Datum: {invoice.IssueDate:dd.MM.yyyy.}").FontSize(10);
                    c.Item().PaddingTop(10);

                    c.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Opis").Bold();
                            header.Cell().AlignRight().Text("Kol.").Bold();
                            header.Cell().AlignRight().Text("Cijena").Bold();
                            header.Cell().AlignRight().Text("Iznos").Bold();
                        });
                    });
                });

                page.Footer().AlignRight().Column(c =>
                {
                    c.Item().Text($"Neto: {invoice.TotalNet:F2} €");
                    c.Item().Text($"PDV: {invoice.TotalVat:F2} €");
                    c.Item().Text($"Ukupno: {invoice.TotalGross:F2} €").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    public async Task<string> GetNextInvoiceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var seq = await _dbContext.Set<InvoiceSequence>()
            .FirstOrDefaultAsync(s => s.Year == year);

        if (seq == null)
        {
            seq = new InvoiceSequence { Id = Guid.NewGuid(), Prefix = "INV", LastNumber = 0, Year = year };
            _dbContext.Set<InvoiceSequence>().Add(seq);
        }

        seq.LastNumber++;
        await _dbContext.SaveChangesAsync();

        return $"{seq.Prefix}-{year}-{seq.LastNumber:D6}";
    }

    public async Task<InvoiceDetailDto> StornoInvoiceAsync(Guid invoiceId, string reason, string? description)
    {
        var original = await _dbContext.Invoices.FindAsync(invoiceId)
            ?? throw new InvalidOperationException($"Invoice {invoiceId} not found");

        if (original.Status != InvoiceStatus.Sent)
            throw new InvalidOperationException("Only issued invoices can be stornoed.");

        var storno = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = $"ST-{original.InvoiceNumber}",
            FolioId = original.FolioId,
            GuestId = original.GuestId,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow,
            TotalNet = -original.TotalNet,
            TotalVat = -original.TotalVat,
            TotalGross = -original.TotalGross,
            Status = InvoiceStatus.Cancelled,
            CreatedAt = DateTime.UtcNow,
        };

        original.Status = InvoiceStatus.Cancelled;
        _dbContext.Invoices.Add(storno);
        await _dbContext.SaveChangesAsync();

        return new InvoiceDetailDto(
            storno.Id, storno.InvoiceNumber, storno.IssueDate,
            $"{original.Guest?.FirstName} {original.Guest?.LastName}",
            original.Guest?.Address,
            storno.TotalNet, storno.TotalVat, storno.TotalGross,
            "EUR", "Cancelled", false, null, new()
        );
    }

    public async Task<InvoiceDetailDto?> GetInvoiceAsync(Guid invoiceId)
    {
        var inv = await _dbContext.Invoices
            .Include(i => i.Guest)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (inv == null) return null;

        return new InvoiceDetailDto(
            inv.Id, inv.InvoiceNumber, inv.IssueDate,
            $"{inv.Guest?.FirstName} {inv.Guest?.LastName}",
            inv.Guest?.Address,
            inv.TotalNet, inv.TotalVat, inv.TotalGross,
            "EUR", inv.Status.ToString(), false, null, new()
        );
    }
}
