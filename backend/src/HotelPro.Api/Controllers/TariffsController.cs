using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/tariffs")]
[Authorize]
public class TariffsController : ControllerBase
{
    private readonly HotelProDbContext _dbContext;

    public TariffsController(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Reception")]
    public async Task<ActionResult<IEnumerable<TariffDto>>> GetTariffs(
        [FromQuery] Guid? roomTypeId,
        [FromQuery] bool? isActive)
    {
        var query = _dbContext.Tariffs
            .Include(t => t.RoomType)
            .AsQueryable();

        if (roomTypeId.HasValue)
            query = query.Where(t => t.RoomTypeId == roomTypeId.Value);

        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        var tariffs = await query
            .Select(t => new TariffDto(
                t.Id,
                t.Name,
                t.RoomTypeId,
                t.RoomType != null ? t.RoomType.Name : null,
                t.ValidFrom,
                t.ValidTo,
                t.BasePrice,
                t.Currency,
                t.IsActive
            ))
            .ToListAsync();

        return Ok(tariffs);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Reception")]
    public async Task<ActionResult<TariffDto>> GetTariff(Guid id)
    {
        var t = await _dbContext.Tariffs
            .Include(t => t.RoomType)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (t == null)
            return NotFound();

        return Ok(new TariffDto(t.Id, t.Name, t.RoomTypeId, t.RoomType?.Name, t.ValidFrom, t.ValidTo, t.BasePrice, t.Currency, t.IsActive));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TariffDto>> CreateTariff(CreateTariffDto dto)
    {
        if (dto.BasePrice <= 0)
            return BadRequest(new { error = "BasePrice must be greater than 0." });

        if (dto.ValidFrom.HasValue && dto.ValidTo.HasValue && dto.ValidFrom >= dto.ValidTo)
            return BadRequest(new { error = "ValidFrom must be before ValidTo." });

        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 100)
            return BadRequest(new { error = "Name is required and must be under 100 characters." });

        var tariff = new Tariff
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            RoomTypeId = dto.RoomTypeId,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            BasePrice = dto.BasePrice,
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "EUR" : dto.Currency,
            IsActive = true
        };

        _dbContext.Tariffs.Add(tariff);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTariff), new { id = tariff.Id, version = "2.0" },
            new TariffDto(tariff.Id, tariff.Name, tariff.RoomTypeId, null, tariff.ValidFrom, tariff.ValidTo, tariff.BasePrice, tariff.Currency, tariff.IsActive));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TariffDto>> UpdateTariff(Guid id, UpdateTariffDto dto)
    {
        var t = await _dbContext.Tariffs.FindAsync(id);
        if (t == null)
            return NotFound();

        if (dto.Name != null)
        {
            if (dto.Name.Length > 100)
                return BadRequest(new { error = "Name must be under 100 characters." });
            t.Name = dto.Name;
        }

        if (dto.RoomTypeId.HasValue) t.RoomTypeId = dto.RoomTypeId.Value;
        if (dto.ValidFrom.HasValue) t.ValidFrom = dto.ValidFrom.Value;
        if (dto.ValidTo.HasValue) t.ValidTo = dto.ValidTo.Value;
        if (dto.BasePrice.HasValue)
        {
            if (dto.BasePrice.Value <= 0)
                return BadRequest(new { error = "BasePrice must be greater than 0." });
            t.BasePrice = dto.BasePrice.Value;
        }
        if (dto.Currency != null) t.Currency = dto.Currency;
        if (dto.IsActive.HasValue) t.IsActive = dto.IsActive.Value;

        await _dbContext.SaveChangesAsync();

        return Ok(new TariffDto(t.Id, t.Name, t.RoomTypeId, null, t.ValidFrom, t.ValidTo, t.BasePrice, t.Currency, t.IsActive));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteTariff(Guid id)
    {
        var t = await _dbContext.Tariffs.FindAsync(id);
        if (t == null)
            return NotFound();

        t.IsActive = false;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
