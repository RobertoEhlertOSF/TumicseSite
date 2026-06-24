using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Data;
using TumicseSite.Models;
using TumicseSite.Utilities;
using TumicseSite.ViewModels;

namespace TumicseSite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = IdentityRoles.Admin)]
public class EventosController(ApplicationDbContext context) : Controller
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

    public async Task<IActionResult> Index(
        string period = AdminEventosFilters.AllValue,
        string visibility = AdminEventosFilters.AllValue,
        string state = AdminEventosFilters.AllValue,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var query = context.Events.AsNoTracking().AsQueryable();

        query = period switch
        {
            AdminEventosFilters.FutureValue => query.Where(item => (item.EndsAt ?? item.StartsAt) >= now),
            AdminEventosFilters.PastValue => query.Where(item => (item.EndsAt ?? item.StartsAt) < now),
            _ => query
        };

        query = visibility switch
        {
            AdminEventosFilters.PublicValue => query.Where(item => item.IsPublic),
            AdminEventosFilters.InternalValue => query.Where(item => !item.IsPublic),
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
            .OrderBy(item => item.StartsAt)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);

        var model = new AdminEventosIndexViewModel
        {
            PeriodFilter = period,
            VisibilityFilter = visibility,
            StateFilter = state,
            PeriodOptions = BuildFilterOptions(
                period,
                ("Todos os periodos", AdminEventosFilters.AllValue),
                ("Futuros", AdminEventosFilters.FutureValue),
                ("Passados", AdminEventosFilters.PastValue)),
            VisibilityOptions = BuildFilterOptions(
                visibility,
                ("Todas as visibilidades", AdminEventosFilters.AllValue),
                ("Publicos", AdminEventosFilters.PublicValue),
                ("Internos", AdminEventosFilters.InternalValue)),
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
        return View(BuildFormViewModel(new AdminEventoFormViewModel()));
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
            EventType = model.EventType.Trim(),
            StartsAt = TumicseTimeZone.FromLocalDateTime(model.StartsAtLocal!.Value),
            EndsAt = model.EndsAtLocal is null ? null : TumicseTimeZone.FromLocalDateTime(model.EndsAtLocal.Value),
            Location = NormalizeOptionalText(model.Location),
            IsPublic = model.IsPublic,
            IsActive = model.IsActive,
            IsCancelled = model.IsCancelled,
            InternalNotes = NormalizeOptionalText(model.InternalNotes)
        });

        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Evento cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var item = await context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(evento => evento.Id == id, cancellationToken);

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
            .FirstOrDefaultAsync(evento => evento.Id == id, cancellationToken);

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
            StartsAtLocal = TumicseTimeZone.ToLocalDateTime(item.StartsAt),
            EndsAtLocal = item.EndsAt is null ? null : TumicseTimeZone.ToLocalDateTime(item.EndsAt.Value),
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
            .FirstOrDefaultAsync(evento => evento.Id == id, cancellationToken);

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
        item.EventType = model.EventType.Trim();
        item.StartsAt = TumicseTimeZone.FromLocalDateTime(model.StartsAtLocal!.Value);
        item.EndsAt = model.EndsAtLocal is null ? null : TumicseTimeZone.FromLocalDateTime(model.EndsAtLocal.Value);
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

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var item = await context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(evento => evento.Id == id, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return View(new AdminEventoDeleteViewModel
        {
            Id = item.Id,
            Title = item.Title,
            EventType = item.EventType,
            StartsAtLabel = FormatDateTimeLabel(item.StartsAt),
            VisibilityLabel = item.IsPublic ? "Publico" : "Interno",
            IsCancelled = item.IsCancelled
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var item = await context.Events
            .FirstOrDefaultAsync(evento => evento.Id == id, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        context.Events.Remove(item);
        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Evento excluido com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private static AdminEventoFormViewModel BuildFormViewModel(AdminEventoFormViewModel model)
    {
        model.EventTypes = EventTypeCatalog.All
            .Select(eventType => new SelectListItem
            {
                Text = eventType,
                Value = eventType,
                Selected = string.Equals(model.EventType, eventType, StringComparison.Ordinal)
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

        if (!EventTypeCatalog.IsValid(model.EventType))
        {
            ModelState.AddModelError(nameof(model.EventType), "Selecione um tipo de evento valido.");
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
            EventType = item.EventType,
            StartsAtLabel = FormatDateTimeLabel(item.StartsAt),
            VisibilityLabel = item.IsPublic ? "Publico" : "Interno",
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
            EventType = item.EventType,
            StartsAtLabel = FormatDateTimeLabel(item.StartsAt),
            EndsAtLabel = item.EndsAt is null ? null : FormatDateTimeLabel(item.EndsAt.Value),
            VisibilityLabel = item.IsPublic ? "Publico" : "Interno",
            IsActive = item.IsActive,
            IsCancelled = item.IsCancelled,
            Description = NormalizeOptionalText(item.Description),
            Location = NormalizeOptionalText(item.Location),
            InternalNotes = NormalizeOptionalText(item.InternalNotes),
            CreatedAtLabel = FormatAuditDateTimeLabel(item.CreatedAt),
            UpdatedAtLabel = item.UpdatedAt is null ? null : FormatAuditDateTimeLabel(item.UpdatedAt.Value)
        };
    }

    private static string FormatDateTimeLabel(DateTimeOffset value)
    {
        var localDateTime = TumicseTimeZone.ToLocalDateTime(value);
        return $"{PtBrCulture.TextInfo.ToTitleCase(localDateTime.ToString("dddd", PtBrCulture))}, {localDateTime:dd/MM/yyyy HH:mm}";
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
}
