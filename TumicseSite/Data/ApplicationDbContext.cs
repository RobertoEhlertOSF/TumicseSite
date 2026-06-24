using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Models;

namespace TumicseSite.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<VideoCategory> VideoCategories => Set<VideoCategory>();
    public DbSet<LessonVideo> LessonVideos => Set<LessonVideo>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<LandingSection> LandingSections => Set<LandingSection>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Keep Identity's internal key lengths deterministic across runtime and design-time.
        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.Property(login => login.LoginProvider)
                .HasMaxLength(128);

            entity.Property(login => login.ProviderKey)
                .HasMaxLength(128);
        });

        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.Property(token => token.LoginProvider)
                .HasMaxLength(128);

            entity.Property(token => token.Name)
                .HasMaxLength(128);
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.DisplayName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(user => user.IsActive)
                .HasDefaultValue(true);

            entity.Property(user => user.CreatedAt)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
        });

        builder.Entity<VideoCategory>(entity =>
        {
            entity.ToTable("VideoCategories");

            entity.Property(category => category.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(category => category.CreatedAt)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity.HasIndex(category => category.Name)
                .IsUnique();

            entity.HasMany(category => category.LessonVideos)
                .WithOne(video => video.VideoCategory)
                .HasForeignKey(video => video.VideoCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<LessonVideo>(entity =>
        {
            entity.ToTable("LessonVideos");

            entity.Property(video => video.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(video => video.YouTubeVideoId)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(video => video.CreatedAt)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity.HasIndex(video => new { video.VideoCategoryId, video.DisplayOrder });
        });

        builder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");

            entity.Property(item => item.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(item => item.Description)
                .HasMaxLength(4000);

            entity.Property(item => item.EventType)
                .HasConversion<string>()
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(item => item.IsAllDay)
                .HasDefaultValue(false);

            entity.Property(item => item.Location)
                .HasMaxLength(200);

            entity.Property(item => item.Address)
                .HasMaxLength(500);

            entity.Property(item => item.GoogleMapsUrl)
                .HasMaxLength(500);

            entity.Property(item => item.IsPublic)
                .HasDefaultValue(true);

            entity.Property(item => item.IsActive)
                .HasDefaultValue(true);

            entity.Property(item => item.IsCancelled)
                .HasDefaultValue(false);

            entity.Property(item => item.InternalNotes)
                .HasMaxLength(4000);

            entity.Property(item => item.CreatedAt)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity.HasIndex(item => item.StartDate);
        });

        builder.Entity<LandingSection>(entity =>
        {
            entity.ToTable("LandingSections");

            entity.Property(section => section.Key)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(section => section.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(section => section.Content)
                .HasMaxLength(4000);

            entity.Property(section => section.CreatedAt)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity.HasIndex(section => section.Key)
                .IsUnique();
        });

        builder.Entity<SiteSetting>(entity =>
        {
            entity.ToTable("SiteSettings");

            entity.Property(setting => setting.Key)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(setting => setting.Value)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(setting => setting.CreatedAt)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity.HasIndex(setting => setting.Key)
                .IsUnique();
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");

            entity.Property(log => log.Action)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(log => log.EntityName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(log => log.EntityId)
                .HasMaxLength(100);

            entity.Property(log => log.Details)
                .HasMaxLength(4000);

            entity.Property(log => log.CreatedAt)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity.HasOne(log => log.User)
                .WithMany(user => user.AuditLogs)
                .HasForeignKey(log => log.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
