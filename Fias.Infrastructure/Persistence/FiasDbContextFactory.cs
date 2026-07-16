using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fias.Infrastructure.Persistence;

public sealed class FiasDbContextFactory
    : IDesignTimeDbContextFactory<FiasDbContext>
{
    public FiasDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<FiasDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;" +
            "Port=5432;" +
            "Database=fias_db;" +
            "Username=postgres;" +
            "Password=654Ko321");

        return new FiasDbContext(
            optionsBuilder.Options);
    }
}
