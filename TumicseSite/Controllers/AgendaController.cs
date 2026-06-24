using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Data;
using TumicseSite.Models;
using TumicseSite.Services;
using TumicseSite.Utilities;
using TumicseSite.ViewModels;

namespace TumicseSite.Controllers;

public class AgendaController(
    ApplicationDbContext context,
    ISiteSettingsService siteSettingsService,
    IEventExportService eventExportService) : Controller
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");
    private const string DefaultInstagramUrl = "https://www.instagram.com/tumicse_oficial/";

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var events = await context.Events
            .AsNoTracking()
            .Where(item =>
                item.IsPublic &&
                item.IsActive &&
                !item.IsCancelled &&
                (item.EndDate ?? item.StartDate) >= now)
            .OrderBy(item => item.StartDate)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var instagramUrl = await GetInstagramUrlAsync(cancellationToken);
        var model = new AgendaIndexViewModel
        {
            Eyebrow = "Calendario publico",
            Title = "Calendario e eventos abertos da casa.",
            Subtitle = "Acompanhe os proximos compromissos publicos do TUMICSE com datas, horarios e informacoes essenciais em ordem cronologica.",
            EmptyTitle = "Nenhum evento publico futuro no momento.",
            EmptyMessage = "Quando a casa publicar novas giras, estudos, aniversarios ou encontros abertos, eles aparecerao aqui separados por mes.",
            HelperLinkLabel = "Instagram oficial",
            HelperLinkUrl = instagramUrl,
            Months = BuildMonthSections(events, showInternalNotes: false),
            EventCount = events.Count
        };

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var item = await context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(eventItem =>
                eventItem.Id == id &&
                eventItem.IsPublic &&
                eventItem.IsActive &&
                !eventItem.IsCancelled,
                cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        var localStart = EventDateTimeHelper.ToLocalStartDate(item);
        var localEnd = EventDateTimeHelper.ToLocalEndDate(item);
        var model = new AgendaEventDetailsViewModel
        {
            Id = item.Id,
            Title = item.Title,
            EventTypeLabel = EventTypeCatalog.GetDisplayName(item.EventType),
            DateLabel = FormatDetailsDateLabel(localStart, localEnd),
            TimeLabel = FormatDetailsTimeLabel(item.IsAllDay, localStart, localEnd),
            Location = NormalizeOptionalText(item.Location),
            Description = NormalizeOptionalText(item.Description)
        };

        return View(model);
    }

    public async Task<IActionResult> ExportIcs(
        string? eventType = null,
        DateOnly? from = null,
        DateOnly? to = null,
        bool birthdaysOnly = false,
        bool futureOnly = false,
        CancellationToken cancellationToken = default)
    {
        var events = await BuildPublicExportQuery(eventType, from, to, birthdaysOnly, futureOnly)
            .OrderBy(item => item.StartDate)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var fileName = BuildExportFileName("ics", eventType, birthdaysOnly, futureOnly, isAdminExport: false);
        return File(eventExportService.BuildIcs(events), "text/calendar; charset=utf-8", fileName);
    }

    public async Task<IActionResult> ExportCsv(
        string? eventType = null,
        DateOnly? from = null,
        DateOnly? to = null,
        bool birthdaysOnly = false,
        bool futureOnly = false,
        CancellationToken cancellationToken = default)
    {
        var events = await BuildPublicExportQuery(eventType, from, to, birthdaysOnly, futureOnly)
            .OrderBy(item => item.StartDate)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var fileName = BuildExportFileName("csv", eventType, birthdaysOnly, futureOnly, isAdminExport: false);
        return File(eventExportService.BuildCsv(events), "text/csv; charset=utf-8", fileName);
    }

    public async Task<IActionResult> ExportPdf(
        string? eventType = null,
        DateOnly? from = null,
        DateOnly? to = null,
        bool birthdaysOnly = false,
        bool futureOnly = false,
        CancellationToken cancellationToken = default)
    {
        var events = await BuildPublicExportQuery(eventType, from, to, birthdaysOnly, futureOnly)
            .OrderBy(item => item.StartDate)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var fileName = BuildExportFileName("pdf", eventType, birthdaysOnly, futureOnly, isAdminExport: false);
        return File(
            eventExportService.BuildPdf(events, "Agenda publica TUMICSE"),
            "application/pdf",
            fileName);
    }

    [Authorize(Roles = $"{IdentityRoles.Admin},{IdentityRoles.Medium}")]
    public async Task<IActionResult> Interna(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var events = await context.Events
            .AsNoTracking()
            .Where(item =>
                (item.IsActive || item.IsCancelled) &&
                (item.EndDate ?? item.StartDate) >= now)
            .OrderBy(item => item.StartDate)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var model = new AgendaIndexViewModel
        {
            Eyebrow = "Calendario interno",
            Title = "Compromissos publicos e privados para mediuns e administracao.",
            Subtitle = "Aqui voce acompanha a agenda futura da casa com eventos publicos, internos, cancelados ou reservados para organizacao interna.",
            EmptyTitle = "Nenhum compromisso futuro cadastrado.",
            EmptyMessage = "Assim que novos eventos forem registrados pela administracao, eles aparecerao aqui com os detalhes internos necessarios.",
            IsInternalView = true,
            Months = BuildMonthSections(events, showInternalNotes: true),
            EventCount = events.Count
        };

        return View(model);
    }

    private IQueryable<Event> BuildPublicExportQuery(
        string? eventType,
        DateOnly? from,
        DateOnly? to,
        bool birthdaysOnly,
        bool futureOnly)
    {
        var query = context.Events
            .AsNoTracking()
            .Where(item => item.IsPublic && item.IsActive && !item.IsCancelled)
            .ApplyEventTypeFilter(eventType, birthdaysOnly)
            .ApplyDateRangeFilter(from, to);

        if (futureOnly)
        {
            query = query.ApplyFutureOnlyFilter(DateTimeOffset.UtcNow);
        }

        return query;
    }

    private async Task<string> GetInstagramUrlAsync(CancellationToken cancellationToken)
    {
        var settings = await siteSettingsService.GetValuesAsync(
            [SiteSettingKeys.InstagramUrl],
            cancellationToken);

        if (settings.TryGetValue(SiteSettingKeys.InstagramUrl, out var instagramUrl) &&
            !string.IsNullOrWhiteSpace(instagramUrl))
        {
            return instagramUrl.Trim();
        }

        return DefaultInstagramUrl;
    }

    private static IReadOnlyList<AgendaMonthSectionViewModel> BuildMonthSections(
        IReadOnlyList<Event> events,
        bool showInternalNotes)
    {
        return events
            .Select(item => new
            {
                Event = item,
                LocalStart = EventDateTimeHelper.ToLocalStartDate(item),
                LocalEnd = EventDateTimeHelper.ToLocalEndDate(item)
            })
            .GroupBy(item => new { item.LocalStart.Year, item.LocalStart.Month })
            .Select(group => new AgendaMonthSectionViewModel
            {
                MonthLabel = FormatMonthLabel(group.Key.Year, group.Key.Month),
                Events = group
                    .Select(item => MapEventCard(item.Event, item.LocalStart, item.LocalEnd, showInternalNotes))
                    .ToArray()
            })
            .ToArray();
    }

    private static AgendaEventCardViewModel MapEventCard(
        Event item,
        DateTime localStart,
        DateTime? localEnd,
        bool showInternalNotes)
    {
        return new AgendaEventCardViewModel
        {
            Id = item.Id,
            DayLabel = localStart.ToString("dd", PtBrCulture),
            MonthShortLabel = localStart.ToString("MMM", PtBrCulture).Replace(".", string.Empty, StringComparison.Ordinal).ToUpper(PtBrCulture),
            WeekdayLabel = PtBrCulture.TextInfo.ToTitleCase(localStart.ToString("dddd", PtBrCulture)),
            TimeLabel = FormatCardTimeLabel(item.IsAllDay, localStart, localEnd),
            Title = item.Title,
            EventTypeLabel = EventTypeCatalog.GetDisplayName(item.EventType),
            Description = TruncateText(item.Description, 220),
            Location = NormalizeOptionalText(item.Location),
            IsPublic = item.IsPublic,
            IsCancelled = item.IsCancelled,
            InternalNotes = showInternalNotes ? NormalizeOptionalText(item.InternalNotes) : null
        };
    }

    private static string FormatMonthLabel(int year, int month)
    {
        var monthDate = new DateTime(year, month, 1);
        return PtBrCulture.TextInfo.ToTitleCase(monthDate.ToString("MMMM 'de' yyyy", PtBrCulture));
    }

    private static string FormatCardTimeLabel(bool isAllDay, DateTime localStart, DateTime? localEnd)
    {
        if (isAllDay)
        {
            if (localEnd is not null && localEnd.Value.Date > localStart.Date)
            {
                return $"Dia inteiro ate {localEnd.Value:dd/MM}";
            }

            return "Dia inteiro";
        }

        if (localEnd is null || localEnd.Value == localStart)
        {
            return $"{localStart:HH:mm}";
        }

        if (localEnd.Value.Date == localStart.Date)
        {
            return $"{localStart:HH:mm} as {localEnd.Value:HH:mm}";
        }

        return $"{localStart:dd/MM HH:mm} ate {localEnd.Value:dd/MM HH:mm}";
    }

    private static string FormatDetailsDateLabel(DateTime localStart, DateTime? localEnd)
    {
        if (localEnd is not null && localEnd.Value.Date > localStart.Date)
        {
            return $"{PtBrCulture.TextInfo.ToTitleCase(localStart.ToString("dddd", PtBrCulture))}, {localStart:dd/MM/yyyy} ate {PtBrCulture.TextInfo.ToTitleCase(localEnd.Value.ToString("dddd", PtBrCulture))}, {localEnd.Value:dd/MM/yyyy}";
        }

        return $"{PtBrCulture.TextInfo.ToTitleCase(localStart.ToString("dddd", PtBrCulture))}, {localStart:dd/MM/yyyy}";
    }

    private static string FormatDetailsTimeLabel(bool isAllDay, DateTime localStart, DateTime? localEnd)
    {
        if (isAllDay)
        {
            return "Dia inteiro";
        }

        if (localEnd is null || localEnd.Value == localStart)
        {
            return $"{localStart:HH:mm}";
        }

        if (localEnd.Value.Date == localStart.Date)
        {
            return $"{localStart:HH:mm} as {localEnd.Value:HH:mm}";
        }

        return $"{localStart:dd/MM HH:mm} ate {localEnd.Value:dd/MM HH:mm}";
    }

    private static string BuildExportFileName(
        string extension,
        string? eventType,
        bool birthdaysOnly,
        bool futureOnly,
        bool isAdminExport)
    {
        var scope = isAdminExport ? "admin" : "publicos";
        var filter = birthdaysOnly
            ? "aniversarios"
            : EventTypeCatalog.TryParse(eventType, out var parsedEventType)
                ? parsedEventType.ToString().ToLowerInvariant()
                : "todos";
        var futureSuffix = futureOnly ? "-futuros" : string.Empty;

        return $"tumicse-eventos-{scope}-{filter}{futureSuffix}.{extension}";
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string? TruncateText(string? value, int maxLength)
    {
        var normalized = NormalizeOptionalText(value);
        if (normalized is null || normalized.Length <= maxLength)
        {
            return normalized;
        }

        return $"{normalized[..maxLength].TrimEnd()}...";
    }
}
