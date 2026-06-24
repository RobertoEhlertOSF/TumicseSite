namespace TumicseSite.ViewModels;

public sealed class AgendaIndexViewModel
{
    public string Eyebrow { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Subtitle { get; init; } = string.Empty;

    public string EmptyTitle { get; init; } = string.Empty;

    public string EmptyMessage { get; init; } = string.Empty;

    public string? HelperLinkLabel { get; init; }

    public string? HelperLinkUrl { get; init; }

    public bool IsInternalView { get; init; }

    public IReadOnlyList<AgendaMonthSectionViewModel> Months { get; init; } = [];

    public int EventCount { get; init; }
}

public sealed class AgendaMonthSectionViewModel
{
    public string MonthLabel { get; init; } = string.Empty;

    public IReadOnlyList<AgendaEventCardViewModel> Events { get; init; } = [];
}

public sealed class AgendaEventCardViewModel
{
    public Guid Id { get; init; }

    public string DayLabel { get; init; } = string.Empty;

    public string MonthShortLabel { get; init; } = string.Empty;

    public string WeekdayLabel { get; init; } = string.Empty;

    public string TimeLabel { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string EventTypeLabel { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? Location { get; init; }

    public bool IsPublic { get; init; }

    public bool IsCancelled { get; init; }

    public string? InternalNotes { get; init; }
}

public sealed class AgendaEventDetailsViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string EventTypeLabel { get; init; } = string.Empty;

    public string DateLabel { get; init; } = string.Empty;

    public string TimeLabel { get; init; } = string.Empty;

    public string? Location { get; init; }

    public string? Description { get; init; }
}
