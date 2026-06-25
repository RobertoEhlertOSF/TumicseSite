using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Models;
using TumicseSite.Utilities;

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

    private static readonly EventSeed[] InitialEvents =
    [
        CreateBirthday("Aniversario Fabio", 2026, 1, 4),
        CreateBirthday("Aniversario Bruna", 2026, 1, 8),
        CreateBirthday("Aniversario Victor", 2026, 1, 11),
        CreateSacredDate("Dia Consagrado a Pai Oxossi", 2026, 1, 20),
        CreateBirthday("Aniversario Rafael", 2026, 1, 30),

        CreatePrivateMaintenance("Retorno aos trabalhos", 2026, 2, 1),
        CreatePublicWork("Trabalho de Cura", 2026, 2, 7),
        CreatePrivateWork("Oblacao Pai Oxossi", 2026, 2, 8),
        CreatePrivateMaintenance("Folga", 2026, 2, 15),
        CreateBirthday("Aniversario Paula", 2026, 2, 17),
        CreatePrivateMaintenance("Folga", 2026, 2, 22),
        CreateBirthday("Aniversario Wesley", 2026, 2, 27),

        CreatePublicGira("Gira Aberta Marinheiros", 2026, 3, 1),
        CreateBirthday("Aniversario Nina", 2026, 3, 2),
        CreatePublicWork("Trabalho de Cura", 2026, 3, 7),
        CreatePrivateWork("Amaci Pai Ze", 2026, 3, 8),
        CreatePrivateWork("Guia do Terreiro", 2026, 3, 8),
        CreateSacredDate("Dia Consagrado as Pombagiras", 2026, 3, 8),
        CreateBirthday("Aniversario Elisangela", 2026, 3, 10),
        CreateDevelopment("Desenvolvimento", 2026, 3, 15),
        CreateDevelopment("Desenvolvimento", 2026, 3, 22),
        CreatePublicGira("Gira Aberta Ze Pilintra", 2026, 3, 29),

        CreatePrivateMaintenance("Folga", 2026, 4, 5),
        CreateBirthday("Aniversario Noeli", 2026, 4, 10),
        CreatePublicWork("Trabalho de Cura", 2026, 4, 11),
        CreateStudy("Leitura de Ancestral", 2026, 4, 12),
        CreateSacredDate("Dia Consagrado aos Caboclos", 2026, 4, 19),
        CreateDevelopment("Desenvolvimento", 2026, 4, 19),
        CreateSacredDate("Dia Consagrado a Pai Ogum", 2026, 4, 20),
        CreatePrivateWork("Oblacao Pai Ogum", 2026, 4, 26),
        CreatePublicGira("Gira Aberta Boiadeiros", 2026, 4, 26),
        CreateSpecialDate("Dia dos Sacerdotes Umbandistas", 2026, 4, 27),

        CreatePublicWork("Trabalho de Cura", 2026, 5, 2),
        CreateDevelopment("Desenvolvimento", 2026, 5, 3),
        CreatePrivateMaintenance("Folga", 2026, 5, 10),
        CreateSacredDate("Dia Consagrado aos Pretos-Velhos", 2026, 5, 13),
        CreateBirthday("Aniversario Mae Elvira", 2026, 5, 13),
        CreateBirthday("Aniversario Marcia T.", 2026, 5, 14),
        CreateBirthday("Aniversario Kathy", 2026, 5, 20),
        CreateSacredDate("Dia Consagrado a Mae Egunita e Ciganos", 2026, 5, 24),
        CreatePrivateWork("Oblacao Mae Egunita e Mae Oba", 2026, 5, 24),
        CreateSpecialDate("Dia dos Cambones", 2026, 5, 27),
        CreateBirthday("Aniversario Jussara", 2026, 5, 29),
        CreateDevelopment("Desenvolvimento", 2026, 5, 31),

        CreateBirthday("Aniversario Cassy", 2026, 6, 2),
        CreatePublicGira("Gira Aberta Pretos Velhos", 2026, 6, 7),
        CreatePublicWork("Trabalho de Cura", 2026, 6, 13),
        CreateSacredDate("Dia Consagrado aos Exus", 2026, 6, 13),
        CreatePublicWork("Mesa de Jurema", 2026, 6, 14),
        CreateBirthday("Aniversario Arthur", 2026, 6, 15),
        CreateBirthday("Aniversario Marco", 2026, 6, 22),
        CreateSacredDate("Dia Consagrado a Pai Xango", 2026, 6, 24),
        CreatePrivateWork("Oblacao Pai Xango", 2026, 6, 28),
        CreatePublicGira("Gira Aberta Baianos", 2026, 6, 28),

        CreateSacredDate("Dia Consagrado aos Boiadeiros", 2026, 7, 2),
        CreatePublicWork("Trabalho de Cura", 2026, 7, 4),
        CreateBirthday("Aniversario Renato", 2026, 7, 4),
        CreateDevelopment("Desenvolvimento", 2026, 7, 5),
        CreateSpecialDate("Dia dos Magos do Fogo", 2026, 7, 7),
        CreateBirthday("Aniversario Caio", 2026, 7, 10),
        CreatePrivateMaintenance("Folga", 2026, 7, 12),
        CreateFeast("Festa de Esquerda", 2026, 7, 19),
        CreateSacredDate("Dia Consagrado e Oblacao a Mae Nana", 2026, 7, 26),
        CreatePublicGira("Gira Aberta Caboclos", 2026, 7, 26),
        CreateBirthday("Aniversario Otilia", 2026, 7, 28),

        CreatePublicWork("Trabalho de Cura", 2026, 8, 1),
        CreateStudy("Leitura de Ancestral", 2026, 8, 2),
        CreateBirthday("Aniversario Joana", 2026, 8, 5),
        CreatePrivateMaintenance("Folga", 2026, 8, 9),
        CreateBirthday("Aniversario Josy", 2026, 8, 10),
        CreateSacredDate("Dia Consagrado a Mae Oya-Loguna", 2026, 8, 11),
        CreateBirthday("Aniversario Beatriz", 2026, 8, 12),
        CreateSacredDate("Dia Consagrado aos Baianos", 2026, 8, 15),
        CreateSacredDate("Dia Consagrado a Pai Obaluae", 2026, 8, 16),
        CreatePrivateWork("Oblacao Obaluae e Loguna", 2026, 8, 16),
        CreateBirthday("Aniversario Juliana M.", 2026, 8, 18),
        CreateBirthday("Aniversario Juliana F.", 2026, 8, 19),
        CreateDevelopment("Desenvolvimento", 2026, 8, 23),
        CreateSacredDate("Dia Consagrado a Pai Oxumare", 2026, 8, 24),
        CreateBirthday("Aniversario Alexandre", 2026, 8, 24),
        CreatePrivateWork("Oblacao Oxumare", 2026, 8, 30),
        CreatePublicGira("Gira Aberta Esquerda", 2026, 8, 30),

        CreateBirthday("Aniversario Claudinha", 2026, 9, 1),
        CreatePrivateMaintenance("Folga", 2026, 9, 6),
        CreateBirthday("Aniversario Nani", 2026, 9, 10),
        CreatePublicWork("Trabalho de Cura", 2026, 9, 12),
        CreatePublicWork("Mesa de Jurema", 2026, 9, 13),
        CreateBirthday("Aniversario Will", 2026, 9, 14),
        CreateSpecialDate("Dia dos Ogas", 2026, 9, 15),
        CreateDevelopment("Desenvolvimento", 2026, 9, 20),
        CreateBirthday("Aniversario Alvaro", 2026, 9, 20),
        CreateSacredDate("Dia Consagrado aos Eres", 2026, 9, 27),
        CreatePublicGira("Gira Aberta Ere", 2026, 9, 27),

        CreatePublicWork("Trabalho de Cura", 2026, 10, 3),
        CreatePublicWork("Atendimento aos PETS", 2026, 10, 3, 14, 0, 17, 0),
        CreatePrivateWork("Oblacao Oxum", 2026, 10, 4),
        CreateSpecialDate("Dia de Sao Francisco de Assis", 2026, 10, 4),
        CreateSacredDate("Dia Consagrado aos Cangaceiros", 2026, 10, 8),
        CreateSacredDate("Dia Consagrado a Mae Oxum", 2026, 10, 12),
        CreateBirthday("Aniversario Amalia", 2026, 10, 15),
        CreatePrivateWork("Oferenda Ancestral + Esquerda", 2026, 10, 18),
        CreateBirthday("Aniversario Sergio", 2026, 10, 19),
        CreatePublicGira("Gira Aberta Ciganos", 2026, 10, 25),
        CreateSacredDate("Dia Consagrado aos Malandros", 2026, 10, 28),

        CreatePrivateWork("Oblacao Pai Omulu (S)", 2026, 11, 1),
        CreateSacredDate("Dia Consagrado a Pai Omulu", 2026, 11, 2),
        CreatePublicWork("Trabalho de Cura", 2026, 11, 7),
        CreateBirthday("Aniversario Be e Leo", 2026, 11, 12),
        CreateSpecialDate("Dia da Umbanda", 2026, 11, 15),
        CreateFeast("Festa de Esquerda", 2026, 11, 15),
        CreateBirthday("Aniversario Lille", 2026, 11, 15),
        CreatePrivateMaintenance("Folga", 2026, 11, 22),
        CreateBirthday("Aniversario Marcia V.", 2026, 11, 26),
        CreatePrivateWork("Oblacao Mae Iansa", 2026, 11, 29),
        CreatePublicGira("Gira Aberta Pretos Velhos", 2026, 11, 29),

        CreateSacredDate("Dia Consagrado a Mae Iansa", 2026, 12, 4),
        CreatePublicWork("Trabalho de Cura", 2026, 12, 5),
        CreatePrivateWork("Oblacao Mae Iemanja", 2026, 12, 6),
        CreateBirthday("Aniversario Roberto", 2026, 12, 6),
        CreateSacredDate("Dia Consagrado a Mae Iemanja", 2026, 12, 8),
        CreateBirthday("Aniversario Caue", 2026, 12, 8),
        CreateSacredDate("Dia Consagrado aos Marinheiros", 2026, 12, 13),
        CreatePrivateWork("Oblacao Pai Oxala", 2026, 12, 13),
        CreateBirthday("Aniversario Claudia", 2026, 12, 15),
        CreatePrivateMaintenance("Recesso", 2026, 12, 20),
        CreateBirthday("Aniversario Bruno", 2026, 12, 20),
        CreateSacredDate("Dia Consagrado a Pai Oxala", 2026, 12, 25),
        CreateBirthday("Aniversario Miriam e Renata", 2026, 12, 28)
    ];

    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        if (configuration.GetValue<bool>("Database:RunMigrationsOnStartup"))
        {
            await context.Database.MigrateAsync();
        }

        if (!configuration.GetValue("Database:SeedOnStartup", true))
        {
            return;
        }

        await SeedRolesAsync(services);
        await SeedSiteSettingsAsync(context);
        await SeedVideoCatalogAsync(context);
        await SeedEventsAsync(context);
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

    private static async Task SeedEventsAsync(ApplicationDbContext context)
    {
        var existingEvents = await context.Events
            .AsNoTracking()
            .Select(item => new
            {
                item.Id,
                item.Title,
                item.EventType,
                item.StartDate
            })
            .ToListAsync();

        foreach (var seed in InitialEvents)
        {
            var seedStartDate = EventDateTimeHelper.ToStoredStartDate(seed.StartDateLocal, seed.IsAllDay);
            var alreadyExists = existingEvents.Any(item => item.Id == seed.Id) ||
                                existingEvents.Any(item =>
                                    string.Equals(item.Title, seed.Title, StringComparison.OrdinalIgnoreCase) &&
                                    item.EventType == seed.EventType &&
                                    item.StartDate == seedStartDate);

            if (alreadyExists)
            {
                continue;
            }

            context.Events.Add(new Event
            {
                Id = seed.Id,
                Title = seed.Title,
                Description = seed.Description,
                StartDate = seedStartDate,
                EndDate = EventDateTimeHelper.ToStoredEndDate(seed.EndDateLocal, seed.StartDateLocal, seed.IsAllDay),
                IsAllDay = seed.IsAllDay,
                Location = seed.Location,
                EventType = seed.EventType,
                IsPublic = seed.IsPublic,
                IsActive = seed.IsActive,
                IsCancelled = seed.IsCancelled,
                InternalNotes = seed.InternalNotes
            });
        }

        await context.SaveChangesAsync();
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

    private static EventSeed CreateBirthday(string title, int year, int month, int day) =>
        new(
            Guid.NewGuid(),
            title,
            "Data de aniversario registrada no calendario institucional da casa.",
            CalendarEventType.Birthday,
            new DateTime(year, month, day),
            null,
            true,
            "Templo TUMICSE",
            true,
            true,
            false,
            null);

    private static EventSeed CreateSacredDate(string title, int year, int month, int day) =>
        new(
            Guid.NewGuid(),
            title,
            "Data consagrada registrada no calendario institucional do TUMICSE.",
            CalendarEventType.Other,
            new DateTime(year, month, day),
            null,
            true,
            "Templo TUMICSE",
            true,
            true,
            false,
            null);

    private static EventSeed CreateSpecialDate(string title, int year, int month, int day) =>
        new(
            Guid.NewGuid(),
            title,
            "Data especial registrada no calendario institucional do TUMICSE.",
            CalendarEventType.Other,
            new DateTime(year, month, day),
            null,
            true,
            "Templo TUMICSE",
            true,
            true,
            false,
            null);

    private static EventSeed CreatePublicGira(string title, int year, int month, int day) =>
        CreateTimedEvent(title, CalendarEventType.Gira, year, month, day, 8, 0, 12, 0, true);

    private static EventSeed CreatePublicWork(
        string title,
        int year,
        int month,
        int day,
        int startHour = 8,
        int startMinute = 0,
        int endHour = 12,
        int endMinute = 0) =>
        CreateTimedEvent(title, CalendarEventType.PublicWork, year, month, day, startHour, startMinute, endHour, endMinute, true);

    private static EventSeed CreatePrivateWork(string title, int year, int month, int day) =>
        new(
            Guid.NewGuid(),
            title,
            "Atividade interna registrada no calendario institucional do TUMICSE.",
            CalendarEventType.PrivateWork,
            new DateTime(year, month, day),
            null,
            true,
            "Templo TUMICSE",
            false,
            true,
            false,
            null);

    private static EventSeed CreateDevelopment(string title, int year, int month, int day) =>
        CreateTimedEvent(title, CalendarEventType.Development, year, month, day, 8, 0, 12, 0, false);

    private static EventSeed CreateStudy(string title, int year, int month, int day) =>
        CreateTimedEvent(title, CalendarEventType.Study, year, month, day, 8, 0, 12, 0, true);

    private static EventSeed CreateFeast(string title, int year, int month, int day) =>
        CreateTimedEvent(title, CalendarEventType.Feast, year, month, day, 8, 0, 12, 0, true);

    private static EventSeed CreatePrivateMaintenance(string title, int year, int month, int day) =>
        new(
            Guid.NewGuid(),
            title,
            "Marcacao interna administrativa do calendario do TUMICSE.",
            CalendarEventType.Maintenance,
            new DateTime(year, month, day),
            null,
            true,
            "Templo TUMICSE",
            false,
            true,
            false,
            null);

    private static EventSeed CreateTimedEvent(
        string title,
        CalendarEventType eventType,
        int year,
        int month,
        int day,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute,
        bool isPublic) =>
        new(
            Guid.NewGuid(),
            title,
            "Evento mapeado a partir do calendario institucional de 2026 do TUMICSE.",
            eventType,
            new DateTime(year, month, day, startHour, startMinute, 0),
            new DateTime(year, month, day, endHour, endMinute, 0),
            false,
            "Templo TUMICSE",
            isPublic,
            true,
            false,
            null);

    private sealed record EventSeed(
        Guid Id,
        string Title,
        string? Description,
        CalendarEventType EventType,
        DateTime StartDateLocal,
        DateTime? EndDateLocal,
        bool IsAllDay,
        string? Location,
        bool IsPublic,
        bool IsActive,
        bool IsCancelled,
        string? InternalNotes);
}
