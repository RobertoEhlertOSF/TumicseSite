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

public class AgendaController(ApplicationDbContext context, ISiteSettingsService siteSettingsService) : Controller
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
                (item.EndsAt ?? item.StartsAt) >= now)
            .OrderBy(item => item.StartsAt)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var instagramUrl = await GetInstagramUrlAsync(cancellationToken);
        var model = new AgendaIndexViewModel
        {
            Eyebrow = "Agenda publica",
            Title = "Giras, encontros e avisos abertos da casa.",
            Subtitle = "Acompanhe os proximos compromissos publicos do TUMICSE com a mesma serenidade e clareza da nossa comunicacao institucional.",
            EmptyTitle = "Nenhum evento publico futuro no momento.",
            EmptyMessage = "Quando a casa publicar novas giras, estudos ou encontros abertos, eles aparecerao aqui em ordem cronologica.",
            HelperLinkLabel = "Instagram oficial",
            HelperLinkUrl = instagramUrl,
            Months = BuildMonthSections(events, showInternalNotes: false),
            EventCount = events.Count
        };

        return View(model);
    }

    [Authorize(Roles = $"{IdentityRoles.Admin},{IdentityRoles.Medium}")]
    public async Task<IActionResult> Interna(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var events = await context.Events
            .AsNoTracking()
            .Where(item =>
                (item.IsActive || item.IsCancelled) &&
                (item.EndsAt ?? item.StartsAt) >= now)
            .OrderBy(item => item.StartsAt)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var model = new AgendaIndexViewModel
        {
            Eyebrow = "Agenda interna",
            Title = "Compromissos publicos e internos organizados para mediuns e administracao.",
            Subtitle = "Aqui voce acompanha giras, estudos, reunioes e atividades internas da casa, com destaque visual para o que e publico, reservado ou cancelado.",
            EmptyTitle = "Nenhum compromisso futuro cadastrado.",
            EmptyMessage = "Assim que novos eventos forem registrados pela administracao, eles aparecerao aqui com os detalhes internos necessarios.",
            IsInternalView = true,
            Months = BuildMonthSections(events, showInternalNotes: true),
            EventCount = events.Count
        };

        return View(model);
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
                LocalStart = TumicseTimeZone.ToLocalDateTime(item.StartsAt),
                LocalEnd = item.EndsAt is null ? (DateTime?)null : TumicseTimeZone.ToLocalDateTime(item.EndsAt.Value)
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
            TimeLabel = FormatTimeLabel(localStart, localEnd),
            Title = item.Title,
            EventType = item.EventType,
            Description = NormalizeOptionalText(item.Description),
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

    private static string FormatTimeLabel(DateTime localStart, DateTime? localEnd)
    {
        if (localEnd is null)
        {
            return $"{PtBrCulture.TextInfo.ToTitleCase(localStart.ToString("dddd", PtBrCulture))}, {localStart:dd/MM} as {localStart:HH:mm}";
        }

        if (localEnd.Value.Date == localStart.Date)
        {
            return $"{PtBrCulture.TextInfo.ToTitleCase(localStart.ToString("dddd", PtBrCulture))}, {localStart:dd/MM} das {localStart:HH:mm} as {localEnd.Value:HH:mm}";
        }

        return $"{PtBrCulture.TextInfo.ToTitleCase(localStart.ToString("dddd", PtBrCulture))}, {localStart:dd/MM HH:mm} ate {localEnd.Value:dd/MM HH:mm}";
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
