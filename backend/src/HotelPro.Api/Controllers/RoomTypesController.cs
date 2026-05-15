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
[Route("api/v{version:apiVersion}/room-types")]
[Authorize]
public class RoomTypesController : ControllerBase
{
    private readonly HotelProDbContext _dbContext;

    public RoomTypesController(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Reception,Housekeeping")]
    public async Task<ActionResult<IEnumerable<RoomTypeDto>>> GetRoomTypes()
    {
        var types = await _dbContext.RoomTypes
            .Where(rt => rt.IsActive)
            .Select(rt => new RoomTypeDto(
                rt.Id,
                rt.Name,
                rt.Code,
                rt.BaseCapacity,
                rt.MaxCapacity,
                rt.DefaultPrice,
                rt.Description,
                rt.IsActive
            ))
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Reception,Housekeeping")]
    public async Task<ActionResult<RoomTypeDto>> GetRoomType(Guid id)
    {
        var rt = await _dbContext.RoomTypes.FindAsync(id);
        if (rt == null)
            return NotFound();

        return Ok(new RoomTypeDto(
            rt.Id,
            rt.Name,
            rt.Code,
            rt.BaseCapacity,
            rt.MaxCapacity,
            rt.DefaultPrice,
            rt.Description,
            rt.IsActive
        ));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<RoomTypeDto>> CreateRoomType(CreateRoomTypeDto dto)
    {
        var exists = await _dbContext.RoomTypes.AnyAsync(rt => rt.Code == dto.Code && rt.IsActive);
        if (exists)
            return BadRequest(new { error = $"Room type code '{dto.Code}' already exists." });

        var rt = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            BaseCapacity = dto.BaseCapacity,
            MaxCapacity = dto.MaxCapacity,
            DefaultPrice = dto.DefaultPrice,
            Description = dto.Description,
            IsActive = true
        };

        _dbContext.RoomTypes.Add(rt);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoomType), new { id = rt.Id, version = "2.0" },
            new RoomTypeDto(rt.Id, rt.Name, rt.Code, rt.BaseCapacity, rt.MaxCapacity, rt.DefaultPrice, rt.Description, rt.IsActive));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<RoomTypeDto>> UpdateRoomType(Guid id, UpdateRoomTypeDto dto)
    {
        var rt = await _dbContext.RoomTypes.FindAsync(id);
        if (rt == null)
            return NotFound();

        if (dto.Code != null && dto.Code != rt.Code)
        {
            var exists = await _dbContext.RoomTypes.AnyAsync(r => r.Code == dto.Code && r.Id != id && r.IsActive);
            if (exists)
                return BadRequest(new { error = $"Room type code '{dto.Code}' already exists." });
            rt.Code = dto.Code;
        }

        if (dto.Name != null) rt.Name = dto.Name;
        if (dto.BaseCapacity.HasValue) rt.BaseCapacity = dto.BaseCapacity.Value;
        if (dto.MaxCapacity.HasValue) rt.MaxCapacity = dto.MaxCapacity.Value;
        if (dto.DefaultPrice.HasValue) rt.DefaultPrice = dto.DefaultPrice.Value;
        if (dto.Description != null) rt.Description = dto.Description;
        if (dto.IsActive.HasValue) rt.IsActive = dto.IsActive.Value;

        await _dbContext.SaveChangesAsync();

        return Ok(new RoomTypeDto(rt.Id, rt.Name, rt.Code, rt.BaseCapacity, rt.MaxCapacity, rt.DefaultPrice, rt.Description, rt.IsActive));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteRoomType(Guid id)
    {
        var rt = await _dbContext.RoomTypes.FindAsync(id);
        if (rt == null)
            return NotFound();

        var hasRooms = await _dbContext.Rooms.AnyAsync(r => r.RoomTypeId == id && r.IsActive);
        if (hasRooms)
            return BadRequest(new { error = "Cannot delete room type that has active rooms." });

        rt.IsActive = false;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
