using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Models;

namespace TumicseSite.Data;

public static class ApplicationDbInitializer
{
    private static readonly IReadOnlyDictionary<string, string> InitialSiteSettings = new Dictionary<string, string>
    {
        ["SiteName"] = "TUMICSE",
        ["WhatsAppNumber"] = string.Empty,
        ["WhatsAppDefaultMessage"] = "Ola! Gostaria de falar com o TUMICSE.",
        ["InstagramUrl"] = "https://www.instagram.com/tumicse_oficial/",
        ["Address"] = "Rua Assis, 96 - Baeta Neves - Sao Bernardo do Campo/SP",
        ["GoogleMapsUrl"] = "https://www.google.com/maps/search/?api=1&query=Rua%20Assis%2C%2096%20-%20Baeta%20Neves%20-%20Sao%20Bernardo%20do%20Campo%2FSP"
    };

    private static readonly (string Name, int DisplayOrder)[] InitialCategories =
    [
        ("Aulas 2020", 1),
        ("Aulas 2021", 2)
    ];

    private static readonly (string Title, string YouTubeVideoId, string CategoryName, int DisplayOrder)[] InitialLessonVideos =
    [
        ("Historia da Umbanda", "sRPx6VTLMmw", "Aulas 2020", 1),
        ("Boiadeiros", "rfKETMoerGw", "Aulas 2020", 2),
        ("Marinheiros", "A1kBBB4UuoY", "Aulas 2020", 3),
        ("Baianos", "X2YD18cUrg0", "Aulas 2020", 4),
        ("Coroa Mediunica", "QZiU7TSoeJk", "Aulas 2021", 1),
        ("Educacao Mediunica", "EBsQ0Tuw4Rg", "Aulas 2021", 2),
        ("Desenvolvimento Mediunico", "lysOndPYrMo", "Aulas 2021", 3),
        ("Desenvolvimento Mediunico II", "EBsQ0Tuw4Rg", "Aulas 2021", 4)
    ];

    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        await SeedRolesAsync(services);
        await SeedSiteSettingsAsync(context);
        await SeedVideoCatalogAsync(context);
        await SeedAdminAsync(services, configuration);
    }

    private static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in IdentityRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            EnsureSuccess(
                await roleManager.CreateAsync(new IdentityRole(roleName)),
                $"criar a role '{roleName}'");
        }
    }

    private static async Task SeedSiteSettingsAsync(ApplicationDbContext context)
    {
        var existingKeys = new HashSet<string>(
            await context.SiteSettings
                .Select(setting => setting.Key)
                .ToListAsync(),
            StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in InitialSiteSettings)
        {
            if (existingKeys.Contains(key))
            {
                continue;
            }

            context.SiteSettings.Add(new SiteSetting
            {
                Key = key,
                Value = value
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedVideoCatalogAsync(ApplicationDbContext context)
    {
        var existingCategoryNames = new HashSet<string>(
            await context.VideoCategories
                .Select(category => category.Name)
                .ToListAsync(),
            StringComparer.OrdinalIgnoreCase);

        foreach (var (name, displayOrder) in InitialCategories)
        {
            if (existingCategoryNames.Contains(name))
            {
                continue;
            }

            context.VideoCategories.Add(new VideoCategory
            {
                Name = name,
                DisplayOrder = displayOrder
            });
        }

        await context.SaveChangesAsync();

        var categories = await context.VideoCategories.ToListAsync();
        var categoriesByName = categories.ToDictionary(category => category.Name, StringComparer.OrdinalIgnoreCase);

        var existingVideos = (await context.LessonVideos
                .Select(video => new { video.Title, video.VideoCategoryId })
                .ToListAsync())
            .Select(video => (video.Title, video.VideoCategoryId))
            .ToHashSet();

        foreach (var (title, youTubeVideoId, categoryName, displayOrder) in InitialLessonVideos)
        {
            var category = categoriesByName[categoryName];
            if (existingVideos.Contains((title, category.Id)))
            {
                continue;
            }

            context.LessonVideos.Add(new LessonVideo
            {
                Title = title,
                YouTubeVideoId = youTubeVideoId,
                VideoCategoryId = category.Id,
                DisplayOrder = displayOrder
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminAsync(IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["SeedAdmin:Email"]?.Trim();
        var password = configuration["SeedAdmin:Password"];
        var displayName = configuration["SeedAdmin:DisplayName"]?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminUser = await userManager.FindByEmailAsync(email);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Administrador" : displayName,
                IsActive = true
            };

            EnsureSuccess(
                await userManager.CreateAsync(adminUser, password),
                "criar o usuario administrador inicial");
        }
        else
        {
            var shouldUpdate = false;

            if (string.IsNullOrWhiteSpace(adminUser.DisplayName) && !string.IsNullOrWhiteSpace(displayName))
            {
                adminUser.DisplayName = displayName;
                shouldUpdate = true;
            }

            if (!adminUser.EmailConfirmed)
            {
                adminUser.EmailConfirmed = true;
                shouldUpdate = true;
            }

            if (!adminUser.IsActive)
            {
                adminUser.IsActive = true;
                shouldUpdate = true;
            }

            if (shouldUpdate)
            {
                EnsureSuccess(
                    await userManager.UpdateAsync(adminUser),
                    "atualizar o usuario administrador inicial");
            }
        }

        var currentRoles = await userManager.GetRolesAsync(adminUser);
        var missingRoles = IdentityRoles.All
            .Where(role => !currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        if (missingRoles.Length > 0)
        {
            EnsureSuccess(
                await userManager.AddToRolesAsync(adminUser, missingRoles),
                "atribuir roles ao usuario administrador inicial");
        }
    }

    private static void EnsureSuccess(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Nao foi possivel {operation}: {errors}");
    }
}
