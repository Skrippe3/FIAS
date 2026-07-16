using Fias.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fias.Infrastructure.Persistence;

public sealed class FiasDbContext : DbContext
{
    public FiasDbContext(
        DbContextOptions<FiasDbContext> options)
        : base(options)
    {
    }

    public DbSet<FiasObject> FiasObjects => Set<FiasObject>();

    public DbSet<FiasAddressObject> FiasAddressObjects =>
        Set<FiasAddressObject>();

    public DbSet<FiasVersion> FiasVersions => Set<FiasVersion>();

    public DbSet<FiasDownload> FiasDownloads => Set<FiasDownload>();

    public DbSet<FiasImportLog> FiasImportLogs => Set<FiasImportLog>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FiasDbContext).Assembly);
    }
}
