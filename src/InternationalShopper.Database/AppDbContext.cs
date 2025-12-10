using Microsoft.EntityFrameworkCore;
using InternationalShopper.Database.Models;

namespace InternationalShopper.Database;

public class AppDbContext : DbContext
{
    public AppDbContext() { }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TelegramUser> TelegramUsers => Set<TelegramUser>();
    public DbSet<TrackedCountry> TrackedCountries => Set<TrackedCountry>();
    public DbSet<Keyword> Keywords => Set<Keyword>();
    public DbSet<BotSettings> BotSettings => Set<BotSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelegramUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasMany(e => e.TrackedCountries)
                .WithOne(c => c.TelegramUser)
                .HasForeignKey(c => c.TelegramUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrackedCountry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();

            entity.HasMany(e => e.Keywords)
                .WithOne(k => k.TrackedCountry)
                .HasForeignKey(k => k.TrackedCountryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Keyword>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.ListType).HasConversion<string>();
            entity.Property(e => e.EntityType).HasConversion<string>();
        });

        modelBuilder.Entity<BotSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}