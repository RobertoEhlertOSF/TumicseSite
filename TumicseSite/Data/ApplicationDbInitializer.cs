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

    private static readonly VideoCategorySeed[] InitialCategories =
    [
        new("Giras de Umbanda", 1),
        new("Mediunidade", 2),
        new("Linhas de trabalho", 3),
        new("Outros assuntos", 4)
    ];

    private static readonly LessonVideoSeed[] InitialLessonVideos =
    [
        new("Hierarquias na Umbanda", "DNqTjhZ88Es", "Giras de Umbanda", 1),
        new("Ritualística de Umbanda", "h07Od3Kit_M", "Giras de Umbanda", 2),
        new("Educação Mediúnica", "smMoJZV2n2I", "Mediunidade", 1),
        new("Desenvolvimento Mediúnico I", "lysOndPYrMo", "Mediunidade", 2),
        new("Desenvolvimento Mediúnico II", "EBsQ0Tuw4Rg", "Mediunidade", 3),
        new("Transporte e desobsessão", "mXkzeJxwsws", "Mediunidade", 4),
        new("Manifestações mediúnicas", "WjEKgI4o6QQ", "Mediunidade", 5),
        new("Linha de trabalho: Boiadeiros", "rfKETMoerGw", "Linhas de trabalho", 1),
        new("Linha de trabalho: Marinheiros", "A1kBBB4UuoY", "Linhas de trabalho", 2),
        new("Linha de trabalho: Baianos", "X2YD18cUrg0", "Linhas de trabalho", 3),
        new("Linha de trabalho: Pretos Velhos", "OCtN77xsGD0", "Linhas de trabalho", 4),
        new("Linha de trabalho: Caboclos", "fFpyvhSfjZY", "Linhas de trabalho", 5),
        new("Linha de trabalho: Erês", "v9JYCmTNPfE", "Linhas de trabalho", 6),
        new("Linha de trabalho: Ciganos", "wQZXNQICfcc", "Linhas de trabalho", 7),
        new("Novas linhas de trabalho", "HkM9PhZnROk", "Linhas de trabalho", 8),
        new("Linhas de trabalho: Exus e Pombagiras", "6PhshyVAdIM", "Linhas de trabalho", 9),
        new("Linhas de trabalho: Exus Mirins e Pombagiras Mirins", "z0knNs0sR0w", "Linhas de trabalho", 10),
        new("História da Umbanda", "sRPx6VTLMmw", "Outros assuntos", 1),
        new("Coroa Mediúnica", "QZiU7TSoeJk", "Outros assuntos", 2)
    ];

    private static readonly string[] LegacyCategoryNames =
    [
        "Aulas 2020",
        "Aulas 2021"
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
        var categories = await context.VideoCategories.ToListAsync();

        foreach (var seed in InitialCategories)
        {
            var category = categories.FirstOrDefault(item =>
                string.Equals(item.Name, seed.Name, StringComparison.OrdinalIgnoreCase));

            if (category is null)
            {
                category = new VideoCategory
                {
                    Name = seed.Name,
                    DisplayOrder = seed.DisplayOrder
                };

                context.VideoCategories.Add(category);
                categories.Add(category);
                continue;
            }

            if (category.DisplayOrder != seed.DisplayOrder)
            {
                category.DisplayOrder = seed.DisplayOrder;
            }
        }

        await context.SaveChangesAsync();

        var categoriesByName = categories.ToDictionary(category => category.Name, StringComparer.OrdinalIgnoreCase);
        var videos = await context.LessonVideos.ToListAsync();

        foreach (var seed in InitialLessonVideos)
        {
            var category = categoriesByName[seed.CategoryName];
            var matchingVideos = videos
                .Where(video => string.Equals(video.YouTubeVideoId, seed.YouTubeVideoId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingVideos.Count == 0)
            {
                var newVideo = new LessonVideo
                {
                    Title = seed.Title,
                    YouTubeVideoId = seed.YouTubeVideoId,
                    VideoCategoryId = category.Id,
                    DisplayOrder = seed.DisplayOrder
                };

                context.LessonVideos.Add(newVideo);
                videos.Add(newVideo);
                continue;
            }

            var primaryVideo = SelectPrimaryVideo(matchingVideos, seed.Title);
            primaryVideo.Title = seed.Title;
            primaryVideo.YouTubeVideoId = seed.YouTubeVideoId;
            primaryVideo.VideoCategoryId = category.Id;
            primaryVideo.DisplayOrder = seed.DisplayOrder;

            foreach (var duplicateVideo in matchingVideos.Where(video => video.Id != primaryVideo.Id))
            {
                context.LessonVideos.Remove(duplicateVideo);
                videos.Remove(duplicateVideo);
            }
        }

        await context.SaveChangesAsync();

        var removableLegacyCategories = await context.VideoCategories
            .Include(category => category.LessonVideos)
            .Where(category =>
                LegacyCategoryNames.Contains(category.Name) &&
                category.LessonVideos.Count == 0)
            .ToListAsync();

        if (removableLegacyCategories.Count > 0)
        {
            context.VideoCategories.RemoveRange(removableLegacyCategories);
            await context.SaveChangesAsync();
        }
    }

    private static LessonVideo SelectPrimaryVideo(IReadOnlyList<LessonVideo> videos, string expectedTitle)
    {
        return videos
            .OrderByDescending(video => string.Equals(video.Title, expectedTitle, StringComparison.OrdinalIgnoreCase))
            .ThenBy(video => video.DisplayOrder)
            .ThenBy(video => video.CreatedAt)
            .First();
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

    private sealed record VideoCategorySeed(string Name, int DisplayOrder);

    private sealed record LessonVideoSeed(string Title, string YouTubeVideoId, string CategoryName, int DisplayOrder);
}
