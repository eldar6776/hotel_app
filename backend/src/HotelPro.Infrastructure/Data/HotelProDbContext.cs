using HotelPro.Core.Entities;
using HotelPro.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.Data;

public class HotelProDbContext : DbContext
{
    public HotelProDbContext(DbContextOptions<HotelProDbContext> options) : base(options)
    {
    }

    // Rooms
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Tariff> Tariffs => Set<Tariff>();
    public DbSet<Amenity> Amenities => Set<Amenity>();

    // Guests & Partners
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<GuestDocument> GuestDocuments => Set<GuestDocument>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<SalesAgent> SalesAgents => Set<SalesAgent>();

    // Bookings
    public DbSet<BookingSource> BookingSources => Set<BookingSource>();
    public DbSet<BookingType> BookingTypes => Set<BookingType>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingRoom> BookingRooms => Set<BookingRoom>();
    public DbSet<GroupBooking> GroupBookings => Set<GroupBooking>();
    public DbSet<BookingHistory> BookingHistories => Set<BookingHistory>();
    public DbSet<RoomAssignment> RoomAssignments => Set<RoomAssignment>();

    // Finance
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<ServiceCatalog> ServiceCatalogs => Set<ServiceCatalog>();
    public DbSet<Folio> Folios => Set<Folio>();
    public DbSet<StayNight> StayNights => Set<StayNight>();
    public DbSet<Charge> Charges => Set<Charge>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<OutstandingBalance> OutstandingBalances => Set<OutstandingBalance>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    // Employees & Housekeeping
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<HousekeepingLog> HousekeepingLogs => Set<HousekeepingLog>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    // System
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<LegacyIdMapping> LegacyIdMappings => Set<LegacyIdMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new BuildingConfiguration());
        modelBuilder.ApplyConfiguration(new RoomTypeConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new TariffConfiguration());
        modelBuilder.ApplyConfiguration(new AmenityConfiguration());

        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new GuestConfiguration());
        modelBuilder.ApplyConfiguration(new GuestDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new PartnerConfiguration());
        modelBuilder.ApplyConfiguration(new SalesAgentConfiguration());

        modelBuilder.ApplyConfiguration(new BookingSourceConfiguration());
        modelBuilder.ApplyConfiguration(new BookingTypeConfiguration());
        modelBuilder.ApplyConfiguration(new BookingConfiguration());
        modelBuilder.ApplyConfiguration(new BookingRoomConfiguration());
        modelBuilder.ApplyConfiguration(new GroupBookingConfiguration());
        modelBuilder.ApplyConfiguration(new BookingHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new RoomAssignmentConfiguration());

        modelBuilder.ApplyConfiguration(new PaymentMethodConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceCatalogConfiguration());
        modelBuilder.ApplyConfiguration(new FolioConfiguration());
        modelBuilder.ApplyConfiguration(new StayNightConfiguration());
        modelBuilder.ApplyConfiguration(new ChargeConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
        modelBuilder.ApplyConfiguration(new OutstandingBalanceConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceItemConfiguration());

        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new ShiftConfiguration());
        modelBuilder.ApplyConfiguration(new HousekeepingLogConfiguration());
        modelBuilder.ApplyConfiguration(new WorkOrderConfiguration());
        modelBuilder.ApplyConfiguration(new AccessLogConfiguration());

        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("audit_logs");
            b.HasKey(x => x.Id);
            b.Property(x => x.OldValues).HasColumnType("jsonb");
            b.Property(x => x.NewValues).HasColumnType("jsonb");
            b.Property(x => x.ChangedProperties).HasColumnType("jsonb");
            b.HasIndex(x => new { x.EntityName, x.EntityId });
            b.HasIndex(x => x.ChangedAt).IsDescending();
        });

        modelBuilder.Entity<FeatureFlag>(b =>
        {
            b.ToTable("feature_flags");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.FeatureName, x.HotelId }).IsUnique();
        });

        modelBuilder.Entity<LegacyIdMapping>(b =>
        {
            b.ToTable("legacy_id_mapping");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.EntityType, x.LegacyId }).IsUnique();
        });
    }
}
