namespace TumicseSite.ViewModels;

public sealed class MembrosIndexViewModel
{
    public string MemberName { get; init; } = string.Empty;

    public bool IsAdmin { get; init; }

    public IReadOnlyList<MembrosLinkCardViewModel> PrimaryLinks { get; init; } = [];

    public IReadOnlyList<MembrosLinkCardViewModel> AdminLinks { get; init; } = [];
}

public sealed class MembrosLinkCardViewModel
{
    public string Eyebrow { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Controller { get; init; } = string.Empty;

    public string Action { get; init; } = string.Empty;

    public string? Area { get; init; }

    public string CtaLabel { get; init; } = string.Empty;
}
