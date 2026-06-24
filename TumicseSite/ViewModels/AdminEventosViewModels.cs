using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TumicseSite.Models;

namespace TumicseSite.ViewModels;

public sealed class AdminEventosIndexViewModel
{
    public string? EventTypeFilter { get; init; }

    public string PeriodFilter { get; init; } = AdminEventosFilters.AllValue;

    public string VisibilityFilter { get; init; } = AdminEventosFilters.AllValue;

    public string StateFilter { get; init; } = AdminEventosFilters.AllValue;

    public IReadOnlyList<SelectListItem> EventTypeOptions { get; init; } = [];

    public IReadOnlyList<SelectListItem> PeriodOptions { get; init; } = [];

    public IReadOnlyList<SelectListItem> VisibilityOptions { get; init; } = [];

    public IReadOnlyList<SelectListItem> StateOptions { get; init; } = [];

    public IReadOnlyList<AdminEventoListItemViewModel> Events { get; init; } = [];
}

public sealed class AdminEventoListItemViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string EventTypeLabel { get; init; } = string.Empty;

    public string StartDateLabel { get; init; } = string.Empty;

    public string VisibilityLabel { get; init; } = string.Empty;

    public bool IsPublic { get; init; }

    public bool IsActive { get; init; }

    public bool IsCancelled { get; init; }

    public string? Location { get; init; }
}

public sealed class AdminEventoFormViewModel : IValidatableObject
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Informe o titulo do evento.")]
    [StringLength(200)]
    [Display(Name = "Titulo")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione o tipo do evento.")]
    [Display(Name = "Tipo de evento")]
    public CalendarEventType? EventType { get; set; }

    [Required(ErrorMessage = "Informe a data e hora de inicio.")]
    [Display(Name = "Inicio")]
    public DateTime? StartDateLocal { get; set; }

    [Display(Name = "Fim")]
    public DateTime? EndDateLocal { get; set; }

    [Display(Name = "Dia inteiro")]
    public bool IsAllDay { get; set; }

    [StringLength(4000)]
    [Display(Name = "Descricao")]
    public string? Description { get; set; }

    [StringLength(200)]
    [Display(Name = "Local")]
    public string? Location { get; set; }

    [Display(Name = "Evento publico")]
    public bool IsPublic { get; set; } = true;

    [Display(Name = "Ativo")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Cancelado")]
    public bool IsCancelled { get; set; }

    [StringLength(4000)]
    [Display(Name = "Observacoes internas")]
    public string? InternalNotes { get; set; }

    public IReadOnlyList<SelectListItem> EventTypes { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDateLocal is null)
        {
            yield return new ValidationResult(
                "Informe a data e hora de inicio.",
                [nameof(StartDateLocal)]);
        }

        if (EndDateLocal is not null && StartDateLocal is not null)
        {
            var normalizedStart = IsAllDay ? StartDateLocal.Value.Date : StartDateLocal.Value;
            var normalizedEnd = IsAllDay ? EndDateLocal.Value.Date : EndDateLocal.Value;

            if (normalizedEnd < normalizedStart)
            {
                yield return new ValidationResult(
                    "A data de termino nao pode ser menor que a data de inicio.",
                    [nameof(EndDateLocal)]);
            }
        }
    }
}

public sealed class AdminEventoDetailsViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string EventTypeLabel { get; init; } = string.Empty;

    public string StartDateLabel { get; init; } = string.Empty;

    public string? EndDateLabel { get; init; }

    public bool IsAllDay { get; init; }

    public string VisibilityLabel { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool IsCancelled { get; init; }

    public string? Description { get; init; }

    public string? Location { get; init; }

    public string? InternalNotes { get; init; }

    public string CreatedAtLabel { get; init; } = string.Empty;

    public string? UpdatedAtLabel { get; init; }
}

public sealed class AdminEventoDeleteViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string EventTypeLabel { get; init; } = string.Empty;

    public string StartDateLabel { get; init; } = string.Empty;

    public string VisibilityLabel { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool IsCancelled { get; init; }
}

public static class AdminEventosFilters
{
    public const string AllValue = "all";
    public const string FutureValue = "future";
    public const string PastValue = "past";
    public const string PublicValue = "public";
    public const string PrivateValue = "private";
    public const string ActiveValue = "active";
    public const string InactiveValue = "inactive";
    public const string CancelledValue = "cancelled";
}
