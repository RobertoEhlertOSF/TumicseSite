namespace TumicseSite.ViewModels;

public sealed class ClassesIndexViewModel
{
    public string SiteName { get; init; } = "TUMICSE";

    public string TempleName { get; init; } = "Templo de Umbanda Mãe Iansã e Caboclo Sete Estrelas";

    public string HeroTitle { get; init; } = "Aulas de Teologia de Umbanda Sagrada";

    public string HeroSubtitle { get; init; } = "Área de estudos reservada para usuários autenticados, com acesso organizado às aulas da casa.";

    public string HeroDescription { get; init; } = "Selecione a aula desejada para assistir com tranquilidade. O conteúdo permanece acessível apenas para quem estiver logado.";

    public IReadOnlyList<string> HeroHighlights { get; init; } = [];

    public IReadOnlyList<LessonGroupViewModel> LessonGroups { get; init; } = [];

    public LessonItemViewModel SelectedLesson { get; init; } = new();
}

public sealed class LessonGroupViewModel
{
    public string Title { get; init; } = string.Empty;

    public bool IsExpanded { get; init; }

    public IReadOnlyList<LessonItemViewModel> Lessons { get; init; } = [];
}

public sealed class LessonItemViewModel
{
    public string Slug { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string YearLabel { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string YouTubeVideoId { get; init; } = string.Empty;

    public bool IsSelected { get; init; }

    public string PlayerUrl => $"https://www.youtube-nocookie.com/embed/{YouTubeVideoId}?rel=0";
}
