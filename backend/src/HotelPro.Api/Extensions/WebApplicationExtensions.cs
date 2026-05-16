using HotelPro.Infrastructure.Data;

namespace HotelPro.Api.Extensions;

public static class WebApplicationExtensions
{
    public static void InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HotelProDbContext>();
        SeedData.Initialize(db);
        TestDataSeeder.Seed(db);
    }
}
