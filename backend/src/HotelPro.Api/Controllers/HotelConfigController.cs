using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/admin/config")]
[Authorize(Roles = "Admin")]
public class HotelConfigController : ControllerBase
{
    private readonly IConfigurationService _configurationService;

    public HotelConfigController(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HotelConfigDto>>> GetAll()
    {
        return Ok(await _configurationService.GetAllAsync());
    }

    [HttpGet("{category}")]
    public async Task<ActionResult<IReadOnlyList<HotelConfigDto>>> GetByCategory(string category)
    {
        return Ok(await _configurationService.GetByCategoryAsync(category));
    }

    [HttpPut("{key}")]
    public async Task<ActionResult<HotelConfigDto>> Update(string key, [FromBody] UpdateHotelConfigDto dto)
    {
        try
        {
            return Ok(await _configurationService.UpdateAsync(key, dto));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "ConfigNotFound", message = $"Configuration key '{key}' was not found." });
        }
    }

    [HttpPost("{key}/test")]
    public async Task<ActionResult<HotelConfigTestResultDto>> TestConnection(string key)
    {
        var config = await _configurationService.GetByKeyAsync(key, includeSecrets: true);
        if (config == null)
        {
            return NotFound(new HotelConfigTestResultDto("not_found", $"Configuration key '{key}' was not found."));
        }

        if (!config.IsEnabled)
        {
            return Ok(new HotelConfigTestResultDto("not_configured", "Configure in Admin > Settings"));
        }

        if (string.IsNullOrWhiteSpace(config.Value))
        {
            return Ok(new HotelConfigTestResultDto("missing_api_key", "Enter API key in Admin > Settings"));
        }

        return Ok(new HotelConfigTestResultDto("configured", "Configuration value is present. Real connection test is implemented by each integration."));
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyDictionary<string, object?>>> GetPublic()
    {
        return Ok(await _configurationService.GetPublicSettingsAsync());
    }
}
