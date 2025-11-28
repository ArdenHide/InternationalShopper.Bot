using Microsoft.EntityFrameworkCore;
using InternationalShopper.Database.Models;

namespace InternationalShopper.Database;

public class AppDbContext : DbContext
{
    public AppDbContext() { }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TelegramUser> TelegramUsers => Set<TelegramUser>();
    public DbSet<BotSettings> BotSettings => Set<BotSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelegramUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<BotSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}