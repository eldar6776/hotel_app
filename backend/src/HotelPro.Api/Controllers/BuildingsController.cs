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
[Route("api/v{version:apiVersion}/buildings")]
[Authorize]
public class BuildingsController : ControllerBase
{
    private readonly HotelProDbContext _dbContext;

    public BuildingsController(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Reception")]
    public async Task<ActionResult<IEnumerable<BuildingDto>>> GetBuildings()
    {
        var buildings = await _dbContext.Buildings
            .Where(b => b.IsActive)
            .Select(b => new BuildingDto(
                b.Id,
                b.Name,
                b.Code,
                b.Address,
                b.City,
                b.IsActive
            ))
            .ToListAsync();

        return Ok(buildings);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Reception")]
    public async Task<ActionResult<BuildingDto>> GetBuilding(Guid id)
    {
        var b = await _dbContext.Buildings.FindAsync(id);
        if (b == null)
            return NotFound();

        return Ok(new BuildingDto(b.Id, b.Name, b.Code, b.Address, b.City, b.IsActive));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BuildingDto>> CreateBuilding(CreateBuildingDto dto)
    {
        var exists = await _dbContext.Buildings.AnyAsync(b => b.Code == dto.Code && b.IsActive);
        if (exists)
            return BadRequest(new { error = $"Building code '{dto.Code}' already exists." });

        var building = new Building
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            Address = dto.Address,
            City = dto.City,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Buildings.Add(building);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBuilding), new { id = building.Id, version = "2.0" },
            new BuildingDto(building.Id, building.Name, building.Code, building.Address, building.City, building.IsActive));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BuildingDto>> UpdateBuilding(Guid id, UpdateBuildingDto dto)
    {
        var b = await _dbContext.Buildings.FindAsync(id);
        if (b == null)
            return NotFound();

        if (dto.Code != null && dto.Code != b.Code)
        {
            var exists = await _dbContext.Buildings.AnyAsync(x => x.Code == dto.Code && x.Id != id && x.IsActive);
            if (exists)
                return BadRequest(new { error = $"Building code '{dto.Code}' already exists." });
            b.Code = dto.Code;
        }

        if (dto.Name != null) b.Name = dto.Name;
        if (dto.Address != null) b.Address = dto.Address;
        if (dto.City != null) b.City = dto.City;
        if (dto.IsActive.HasValue) b.IsActive = dto.IsActive.Value;

        b.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(new BuildingDto(b.Id, b.Name, b.Code, b.Address, b.City, b.IsActive));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteBuilding(Guid id)
    {
        var b = await _dbContext.Buildings.FindAsync(id);
        if (b == null)
            return NotFound();

        var hasRooms = await _dbContext.Rooms.AnyAsync(r => r.BuildingId == id && r.IsActive);
        if (hasRooms)
            return BadRequest(new { error = "Cannot delete building that has active rooms." });

        b.IsActive = false;
        b.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
