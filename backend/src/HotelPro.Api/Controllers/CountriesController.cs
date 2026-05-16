using Asp.Versioning;
using HotelPro.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/countries")]
public class CountriesController : ControllerBase
{
    private readonly HotelProDbContext _dbContext;

    public CountriesController(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult> GetCountries()
    {
        var countries = await _dbContext.Countries
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.Name,
                c.Nationality
            })
            .ToListAsync();

        return Ok(countries);
    }
}
