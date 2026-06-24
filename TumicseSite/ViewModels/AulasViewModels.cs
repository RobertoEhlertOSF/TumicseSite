namespace TumicseSite.ViewModels;

public sealed class AulasIndexViewModel
{
    public string PageTitle { get; init; } = "Aulas TUMICSE";

    public string PageSubtitle { get; init; } = "Estudos internos do TUMICSE organizados por categoria, com acesso reservado aos mediuns e administradores da casa.";

    public string PageDescription { get; init; } = "Selecione uma categoria para navegar pelas aulas, revisar fundamentos e assistir ao conteudo com tranquilidade.";

    public string? SelectedCategoryName { get; init; }

    public string? SelectedCategorySlug { get; init; }

    public IReadOnlyList<AulasCategoryFilterViewModel> Filters { get; init; } = [];

    public IReadOnlyList<AulasCategorySectionViewModel> Categories { get; init; } = [];

    public bool HasLessons => Categories.Count > 0;
}

public sealed class AulasCategoryFilterViewModel
{
    public string Label { get; init; } = string.Empty;

    public string? Slug { get; init; }

    public bool IsSelected { get; init; }

    public int LessonCount { get; init; }
}

public sealed class AulasCategorySectionViewModel
{
    public string Name { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public int LessonCount { get; init; }

    public IReadOnlyList<AulasLessonCardViewModel> Lessons { get; init; } = [];
}

public sealed class AulasLessonCardViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string CategorySlug { get; init; } = string.Empty;

    public string YouTubeVideoId { get; init; } = string.Empty;

    public string PublishedAtLabel { get; init; } = string.Empty;

    public string ThumbnailUrl => $"https://img.youtube.com/vi/{YouTubeVideoId}/hqdefault.jpg";
}

public sealed class AulasDetailsViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string CategorySlug { get; init; } = string.Empty;

    public string YouTubeVideoId { get; init; } = string.Empty;

    public string PublishedAtLabel { get; init; } = string.Empty;

    public IReadOnlyList<AulasLessonCardViewModel> RelatedLessons { get; init; } = [];

    public string EmbedUrl => $"https://www.youtube.com/embed/{YouTubeVideoId}";
}
