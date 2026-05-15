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
[Route("api/v{version:apiVersion}/amenities")]
[Authorize]
public class AmenitiesController : ControllerBase
{
    private readonly HotelProDbContext _dbContext;

    public AmenitiesController(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Reception,Housekeeping")]
    public async Task<ActionResult<IEnumerable<AmenityDto>>> GetAmenities([FromQuery] bool? isActive)
    {
        var query = _dbContext.Amenities.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        var amenities = await query
            .Select(a => new AmenityDto(a.Id, a.Name, a.Icon, a.IsActive))
            .ToListAsync();

        return Ok(amenities);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Reception,Housekeeping")]
    public async Task<ActionResult<AmenityDto>> GetAmenity(Guid id)
    {
        var a = await _dbContext.Amenities.FindAsync(id);
        if (a == null)
            return NotFound();

        return Ok(new AmenityDto(a.Id, a.Name, a.Icon, a.IsActive));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AmenityDto>> CreateAmenity(CreateAmenityDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { error = "Name is required." });

        var amenity = new Amenity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Icon = dto.Icon,
            IsActive = true
        };

        _dbContext.Amenities.Add(amenity);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAmenity), new { id = amenity.Id, version = "2.0" },
            new AmenityDto(amenity.Id, amenity.Name, amenity.Icon, amenity.IsActive));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AmenityDto>> UpdateAmenity(Guid id, UpdateAmenityDto dto)
    {
        var a = await _dbContext.Amenities.FindAsync(id);
        if (a == null)
            return NotFound();

        if (dto.Name != null) a.Name = dto.Name;
        if (dto.Icon != null) a.Icon = dto.Icon;
        if (dto.IsActive.HasValue) a.IsActive = dto.IsActive.Value;

        await _dbContext.SaveChangesAsync();

        return Ok(new AmenityDto(a.Id, a.Name, a.Icon, a.IsActive));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteAmenity(Guid id)
    {
        var a = await _dbContext.Amenities.FindAsync(id);
        if (a == null)
            return NotFound();

        a.IsActive = false;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
