using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DnD.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating ApplicationDbContext instances.
/// Used by Entity Framework Core CLI tools for migrations.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Read connection string from environment or use default
        var connectionString = Environment.GetEnvironmentVariable("DbConnection");        
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
