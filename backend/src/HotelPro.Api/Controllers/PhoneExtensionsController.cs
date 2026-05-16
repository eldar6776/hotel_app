using Asp.Versioning;
using HotelPro.Core.Entities;
using HotelPro.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/phone-extensions"), Authorize]
public class PhoneExtensionsController : ControllerBase
{
    private readonly HotelProDbContext _db;

    public PhoneExtensionsController(HotelProDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var exts = await _db.Set<PhoneExtension>()
            .Include(x => x.Room)
            .Select(x => new { x.Extension, x.RoomId, RoomNumber = x.Room!.RoomNumber, x.Description })
            .OrderBy(x => x.Extension)
            .ToListAsync();

        return Ok(exts);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Add([FromBody] PhoneExtensionDto dto)
    {
        if (await _db.Set<PhoneExtension>().AnyAsync(x => x.Extension == dto.Extension))
            return Conflict(new { error = "Extension already exists" });

        _db.Set<PhoneExtension>().Add(new PhoneExtension
        {
            Extension = dto.Extension,
            RoomId = dto.RoomId,
            Description = dto.Description
        });

        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPut("{extension}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Update(string extension, [FromBody] PhoneExtensionDto dto)
    {
        var ext = await _db.Set<PhoneExtension>().FindAsync(extension);
        if (ext == null) return NotFound();

        ext.RoomId = dto.RoomId;
        ext.Description = dto.Description;
        await _db.SaveChangesAsync();

        return Ok(new { ext.Extension, ext.RoomId, ext.Description });
    }
}

public record PhoneExtensionDto(string Extension, Guid? RoomId, string? Description);
