using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelPro.Infrastructure.Data;

public class HotelProDbContextFactory : IDesignTimeDbContextFactory<HotelProDbContext>
{
    public HotelProDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HotelProDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("HOTEL_DB_CONN")
            ?? "Host=localhost;Port=5432;Database=hotelpro;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new HotelProDbContext(optionsBuilder.Options, null);
    }
}
