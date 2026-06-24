using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Data;
using TumicseSite.Models;
using TumicseSite.Services;
using TumicseSite.Utilities;
using TumicseSite.ViewModels;

namespace TumicseSite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = IdentityRoles.Admin)]
public class EventosController(ApplicationDbContext context, IEventExportService eventExportService) : Controller
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

    public async Task<IActionResult> Index(
        string? eventType = null,
        string period = AdminEventosFilters.AllValue,
        string visibility = AdminEventosFilters.AllValue,
        string state = AdminEventosFilters.AllValue,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var query = context.Events
            .AsNoTracking()
            .AsQueryable()
            .ApplyEventTypeFilter(eventType);

        query = period switch
        {
            AdminEventosFilters.FutureValue => query.ApplyFutureOnlyFilter(now),
            AdminEventosFilters.PastValue => query.Where(item => (item.EndDate ?? item.StartDate) < now),
            _ => query
        };

        query = visibility switch
        {
            AdminEventosFilters.PublicValue => query.Where(item => item.IsPublic),
            AdminEventosFilters.PrivateValue => query.Where(item => !item.IsPublic),
            _ => query
        };

        query = state switch
        {
            AdminEventosFilters.ActiveValue => query.Where(item => item.IsActive && !item.IsCancelled),
            AdminEventosFilters.InactiveValue => query.Where(item => !item.IsActive),
            AdminEventosFilters.CancelledValue => query.Where(item => item.IsCancelled),
            _ => query
        };

        var events = await query
            .OrderBy(item => item.StartDate)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var model = new AdminEventosIndexViewModel
        {
            EventTypeFilter = eventType,
            PeriodFilter = period,
            VisibilityFilter = visibility,
            StateFilter = state,
            EventTypeOptions = BuildEventTypeOptions(eventType),
            PeriodOptions = BuildFilterOptions(
                period,
                ("Todos os periodos", AdminEventosFilters.AllValue),
                ("Futuros", AdminEventosFilters.FutureValue),
                ("Passados", AdminEventosFilters.PastValue)),
            VisibilityOptions = BuildFilterOptions(
                visibility,
                ("Todas as visibilidades", AdminEventosFilters.AllValue),
                ("Publicos", AdminEventosFilters.PublicValue),
                ("Privados", AdminEventosFilters.PrivateValue)),
            StateOptions = BuildFilterOptions(
                state,
                ("Todos os estados", AdminEventosFilters.AllValue),
                ("Ativos", AdminEventosFilters.ActiveValue),
                ("Inativos", AdminEventosFilters.InactiveValue),
                ("Cancelados", AdminEventosFilters.CancelledValue)),
            Events = events
                .Select(MapListItem)
                .ToArray()
        };

        return View(model);
    }

    public IActionResult Create()
    {
        return View(BuildFormViewModel(new AdminEventoFormViewModel
        {
            IsActive = true,
            IsPublic = true
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminEventoFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ValidateEventForm(model))
        {
            return View(BuildFormViewModel(model));
        }

        context.Events.Add(new Event
        {
            Title = model.Title.Trim(),
            Description = NormalizeOptionalText(model.Description),
            EventType = model.EventType!.Value,
            StartDate = EventDateTimeHelper.ToStoredStartDate(model.StartDateLocal!.Value, model.IsAllDay),
            EndDate = EventDateTimeHelper.ToStoredEndDate(model.EndDateLocal, model.StartDateLocal.Value, model.IsAllDay),
            IsAllDay = model.IsAllDay,
            Location = NormalizeOptionalText(model.Location),
            IsPublic = model.IsPublic,
            IsActive = model.IsActive,
            IsCancelled = model.IsCancelled,
            InternalNotes = NormalizeOptionalText(model.InternalNotes),
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Evento cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var item = await context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(eventItem => eventItem.Id == id, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return View(MapDetails(item));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var item = await context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(eventItem => eventItem.Id == id, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        var model = new AdminEventoFormViewModel
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            EventType = item.EventType,
            StartDateLocal = EventDateTimeHelper.ToEditableStartLocal(item),
            EndDateLocal = EventDateTimeHelper.ToEditableEndLocal(item),
            IsAllDay = item.IsAllDay,
            Location = item.Location,
            IsPublic = item.IsPublic,
            IsActive = item.IsActive,
            IsCancelled = item.IsCancelled,
            InternalNotes = item.InternalNotes
        };

        return View(BuildFormViewModel(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AdminEventoFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var item = await context.Events
            .FirstOrDefaultAsync(eventItem => eventItem.Id == id, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        if (!ValidateEventForm(model))
        {
            return View(BuildFormViewModel(model));
        }

        item.Title = model.Title.Trim();
        item.Description = NormalizeOptionalText(model.Description);
        item.EventType = model.EventType!.Value;
        item.StartDate = EventDateTimeHelper.ToStoredStartDate(model.StartDateLocal!.Value, model.IsAllDay);
        item.EndDate = EventDateTimeHelper.ToStoredEndDate(model.EndDateLocal, model.StartDateLocal.Value, model.IsAllDay);
        item.IsAllDay = model.IsAllDay;
        item.Location = NormalizeOptionalText(model.Location);
        item.IsPublic = model.IsPublic;
        item.IsActive = model.IsActive;
        item.IsCancelled = model.IsCancelled;
        item.InternalNotes = NormalizeOptionalText(model.InternalNotes);
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Evento atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(
        Guid id,
        string? eventType = null,
        string period = AdminEventosFilters.AllValue,
        string visibility = AdminEventosFilters.AllValue,
        string state = AdminEventosFilters.AllValue,
        CancellationToken cancellationToken = default)
    {
        var item = await context.Events
            .FirstOrDefaultAsync(eventItem => eventItem.Id == id, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        item.IsActive = !item.IsActive;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = item.IsActive
            ? "Evento ativado com sucesso."
            : "Evento desativado com sucesso.";

        return RedirectToAction(nameof(Index), new
        {
            eventType,
            period,
            visibility,
            state
        });
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var item = await context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(eventItem => eventItem.Id == id, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return View(new AdminEventoDeleteViewModel
        {
            Id = item.Id,
            Title = item.Title,
            EventTypeLabel = EventTypeCatalog.GetDisplayName(item.EventType),
            StartDateLabel = FormatDateTimeLabel(item),
            VisibilityLabel = item.IsPublic ? "Publico" : "Privado",
            IsActive = item.IsActive,
            IsCancelled = item.IsCancelled
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var item = await context.Events
            .FirstOrDefaultAsync(eventItem => eventItem.Id == id, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        context.Events.Remove(item);
        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Evento excluido com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportIcs(
        string? eventType = null,
        string period = AdminEventosFilters.AllValue,
        string visibility = AdminEventosFilters.AllValue,
        string state = AdminEventosFilters.AllValue,
        DateOnly? from = null,
        DateOnly? to = null,
        bool birthdaysOnly = false,
        CancellationToken cancellationToken = default)
    {
        var events = await BuildExportQuery(eventType, period, visibility, state, from, to, birthdaysOnly)
            .OrderBy(item => item.StartDate)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var fileName = BuildExportFileName("ics", eventType, birthdaysOnly, visibility);
        return File(eventExportService.BuildIcs(events), "text/calendar; charset=utf-8", fileName);
    }

    public async Task<IActionResult> ExportCsv(
        string? eventType = null,
        string period = AdminEventosFilters.AllValue,
        string visibility = AdminEventosFilters.AllValue,
        string state = AdminEventosFilters.AllValue,
        DateOnly? from = null,
        DateOnly? to = null,
        bool birthdaysOnly = false,
        CancellationToken cancellationToken = default)
    {
        var events = await BuildExportQuery(eventType, period, visibility, state, from, to, birthdaysOnly)
            .OrderBy(item => item.StartDate)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var fileName = BuildExportFileName("csv", eventType, birthdaysOnly, visibility);
        return File(eventExportService.BuildCsv(events), "text/csv; charset=utf-8", fileName);
    }

    public async Task<IActionResult> ExportPdf(
        string? eventType = null,
        string period = AdminEventosFilters.AllValue,
        string visibility = AdminEventosFilters.AllValue,
        string state = AdminEventosFilters.AllValue,
        DateOnly? from = null,
        DateOnly? to = null,
        bool birthdaysOnly = false,
        CancellationToken cancellationToken = default)
    {
        var events = await BuildExportQuery(eventType, period, visibility, state, from, to, birthdaysOnly)
            .OrderBy(item => item.StartDate)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var fileName = BuildExportFileName("pdf", eventType, birthdaysOnly, visibility);
        return File(
            eventExportService.BuildPdf(events, "Agenda administrativa TUMICSE", includeAdministrativeFields: true),
            "application/pdf",
            fileName);
    }

    private IQueryable<Event> BuildExportQuery(
        string? eventType,
        string period,
        string visibility,
        string state,
        DateOnly? from,
        DateOnly? to,
        bool birthdaysOnly)
    {
        var now = DateTimeOffset.UtcNow;
        var query = context.Events
            .AsNoTracking()
            .AsQueryable()
            .ApplyEventTypeFilter(eventType, birthdaysOnly)
            .ApplyDateRangeFilter(from, to);

        query = period switch
        {
            AdminEventosFilters.FutureValue => query.ApplyFutureOnlyFilter(now),
            AdminEventosFilters.PastValue => query.Where(item => (item.EndDate ?? item.StartDate) < now),
            _ => query
        };

        query = visibility switch
        {
            AdminEventosFilters.PublicValue => query.Where(item => item.IsPublic),
            AdminEventosFilters.PrivateValue => query.Where(item => !item.IsPublic),
            _ => query
        };

        query = state switch
        {
            AdminEventosFilters.ActiveValue => query.Where(item => item.IsActive && !item.IsCancelled),
            AdminEventosFilters.InactiveValue => query.Where(item => !item.IsActive),
            AdminEventosFilters.CancelledValue => query.Where(item => item.IsCancelled),
            _ => query
        };

        return query;
    }

    private static AdminEventoFormViewModel BuildFormViewModel(AdminEventoFormViewModel model)
    {
        model.EventTypes = EventTypeCatalog.All
            .Select(eventType => new SelectListItem
            {
                Text = EventTypeCatalog.GetDisplayName(eventType),
                Value = eventType.ToString(),
                Selected = model.EventType == eventType
            })
            .ToArray();

        return model;
    }

    private bool ValidateEventForm(AdminEventoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return false;
        }

        if (model.EventType is null)
        {
            ModelState.AddModelError(nameof(model.EventType), "Selecione o tipo do evento.");
            return false;
        }

        return true;
    }

    private static AdminEventoListItemViewModel MapListItem(Event item)
    {
        return new AdminEventoListItemViewModel
        {
            Id = item.Id,
            Title = item.Title,
            EventTypeLabel = EventTypeCatalog.GetDisplayName(item.EventType),
            StartDateLabel = FormatDateTimeLabel(item),
            VisibilityLabel = item.IsPublic ? "Publico" : "Privado",
            IsPublic = item.IsPublic,
            IsActive = item.IsActive,
            IsCancelled = item.IsCancelled,
            Location = NormalizeOptionalText(item.Location)
        };
    }

    private static AdminEventoDetailsViewModel MapDetails(Event item)
    {
        return new AdminEventoDetailsViewModel
        {
            Id = item.Id,
            Title = item.Title,
            EventTypeLabel = EventTypeCatalog.GetDisplayName(item.EventType),
            StartDateLabel = FormatDateTimeLabel(item, includeEndDate: false),
            EndDateLabel = item.EndDate is null ? null : FormatEndDateLabel(item),
            IsAllDay = item.IsAllDay,
            VisibilityLabel = item.IsPublic ? "Publico" : "Privado",
            IsActive = item.IsActive,
            IsCancelled = item.IsCancelled,
            Description = NormalizeOptionalText(item.Description),
            Location = NormalizeOptionalText(item.Location),
            InternalNotes = NormalizeOptionalText(item.InternalNotes),
            CreatedAtLabel = FormatAuditDateTimeLabel(item.CreatedAt),
            UpdatedAtLabel = item.UpdatedAt is null ? null : FormatAuditDateTimeLabel(item.UpdatedAt.Value)
        };
    }

    private static string FormatDateTimeLabel(Event item, bool includeEndDate = true)
    {
        var localStart = EventDateTimeHelper.ToLocalStartDate(item);
        var localEnd = EventDateTimeHelper.ToLocalEndDate(item);

        if (item.IsAllDay)
        {
            if (includeEndDate && localEnd is not null && localEnd.Value.Date > localStart.Date)
            {
                return $"{localStart:dd/MM/yyyy} ate {localEnd.Value:dd/MM/yyyy} (dia inteiro)";
            }

            return $"{PtBrCulture.TextInfo.ToTitleCase(localStart.ToString("dddd", PtBrCulture))}, {localStart:dd/MM/yyyy} (dia inteiro)";
        }

        if (localEnd is null || localEnd.Value == localStart)
        {
            return $"{PtBrCulture.TextInfo.ToTitleCase(localStart.ToString("dddd", PtBrCulture))}, {localStart:dd/MM/yyyy HH:mm}";
        }

        if (localEnd.Value.Date == localStart.Date)
        {
            return $"{PtBrCulture.TextInfo.ToTitleCase(localStart.ToString("dddd", PtBrCulture))}, {localStart:dd/MM/yyyy HH:mm} as {localEnd.Value:HH:mm}";
        }

        return $"{localStart:dd/MM/yyyy HH:mm} ate {localEnd.Value:dd/MM/yyyy HH:mm}";
    }

    private static string FormatEndDateLabel(Event item)
    {
        var localEnd = EventDateTimeHelper.ToLocalEndDate(item);
        if (localEnd is null)
        {
            return string.Empty;
        }

        if (item.IsAllDay)
        {
            return $"{PtBrCulture.TextInfo.ToTitleCase(localEnd.Value.ToString("dddd", PtBrCulture))}, {localEnd.Value:dd/MM/yyyy}";
        }

        return $"{PtBrCulture.TextInfo.ToTitleCase(localEnd.Value.ToString("dddd", PtBrCulture))}, {localEnd.Value:dd/MM/yyyy HH:mm}";
    }

    private static string FormatAuditDateTimeLabel(DateTimeOffset value)
    {
        var localDateTime = TumicseTimeZone.ToLocalDateTime(value);
        return localDateTime.ToString("dd/MM/yyyy HH:mm", PtBrCulture);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static IReadOnlyList<SelectListItem> BuildFilterOptions(
        string selectedValue,
        params (string Label, string Value)[] options)
    {
        return options
            .Select(option => new SelectListItem
            {
                Text = option.Label,
                Value = option.Value,
                Selected = string.Equals(option.Value, selectedValue, StringComparison.OrdinalIgnoreCase)
            })
            .ToArray();
    }

    private static IReadOnlyList<SelectListItem> BuildEventTypeOptions(string? selectedEventType)
    {
        var options = new List<SelectListItem>
        {
            new()
            {
                Text = "Todos os tipos",
                Value = string.Empty,
                Selected = string.IsNullOrWhiteSpace(selectedEventType)
            }
        };

        options.AddRange(EventTypeCatalog.All.Select(eventType => new SelectListItem
        {
            Text = EventTypeCatalog.GetDisplayName(eventType),
            Value = eventType.ToString(),
            Selected = string.Equals(selectedEventType, eventType.ToString(), StringComparison.OrdinalIgnoreCase)
        }));

        return options;
    }

    private static string BuildExportFileName(
        string extension,
        string? eventType,
        bool birthdaysOnly,
        string visibility)
    {
        var scope = visibility switch
        {
            AdminEventosFilters.PublicValue => "publicos",
            AdminEventosFilters.PrivateValue => "privados",
            _ => "todos"
        };

        var filter = birthdaysOnly
            ? "aniversarios"
            : EventTypeCatalog.TryParse(eventType, out var parsedEventType)
                ? parsedEventType.ToString().ToLowerInvariant()
                : "tipos";

        return $"tumicse-eventos-admin-{scope}-{filter}.{extension}";
    }
}
