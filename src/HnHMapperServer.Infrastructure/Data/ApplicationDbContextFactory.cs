using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HnHMapperServer.Infrastructure.Data;

/// <summary>
/// Design-time factory to enable EF Core migration scaffolding without a startup project.
/// Uses a SQLite database under the configured GridStorage (default: "map").
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var gridStorage = Environment.GetEnvironmentVariable("GridStorage");
        if (string.IsNullOrWhiteSpace(gridStorage))
        {
            gridStorage = "map";
        }

        // Ensure directory exists
        try
        {
            Directory.CreateDirectory(gridStorage);
        }
        catch
        {
            // ignore
        }

        var dbPath = Path.Combine(gridStorage, "grids.db");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}













