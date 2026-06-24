using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Models;

namespace TumicseSite.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SiteSetting>(entity =>
        {
            entity.HasKey(setting => setting.Key);
            entity.Property(setting => setting.Key).HasMaxLength(100);
            entity.Property(setting => setting.Value).HasMaxLength(2000);

            entity.HasData(
                new SiteSetting { Key = SiteSettingKeys.SiteName, Value = "TUMICSE" },
                new SiteSetting { Key = SiteSettingKeys.WhatsAppNumber, Value = string.Empty },
                new SiteSetting { Key = SiteSettingKeys.WhatsAppDefaultMessage, Value = "Ola! Gostaria de mais informacoes sobre o terreiro." },
                new SiteSetting { Key = SiteSettingKeys.InstagramUrl, Value = string.Empty },
                new SiteSetting { Key = SiteSettingKeys.Address, Value = "Endereco a confirmar." },
                new SiteSetting { Key = SiteSettingKeys.GoogleMapsUrl, Value = string.Empty });
        });
    }
}
