namespace TumicseSite.ViewModels;

public sealed class HomeIndexViewModel
{
    public string SiteName { get; init; } = "TUMICSE";

    public string TempleName { get; init; } = "Templo de Umbanda Mãe Iansã e Caboclo Sete Estrelas";

    public string HeroTitle { get; init; } = "Templo de Umbanda Mãe Iansã e Caboclo Sete Estrelas";

    public string HeroSubtitle { get; init; } = "Uma casa de Umbanda Sagrada em São Bernardo do Campo/SP, dedicada ao acolhimento, à fé, à caridade e ao respeito aos fundamentos espirituais.";

    public string FoundationLabel { get; init; } = "Desde 22/07/2011";

    public string CityState { get; init; } = "São Bernardo do Campo/SP";

    public string Address { get; init; } = "Rua Assis, 96 - Baeta Neves - São Bernardo do Campo/SP";

    public string GoogleMapsUrl { get; init; } = string.Empty;

    public string InstagramUrl { get; init; } = "https://www.instagram.com/tumicse_oficial/";

    public string InstagramHandle { get; init; } = "@tumicse_oficial";

    public string StoreUrl { get; init; } = "https://www.instagram.com/venhadeaxe/";

    public string StoreHandle { get; init; } = "@venhadeaxe";

    public string? WhatsAppUrl { get; init; }

    public IReadOnlyList<string> HeroHighlights { get; init; } = [];

    public IReadOnlyList<SectionCardViewModel> AboutCards { get; init; } = [];

    public IReadOnlyList<SectionCardViewModel> GiraCards { get; init; } = [];

    public IReadOnlyList<string> OpenGiraScheduleItems { get; init; } = [];

    public IReadOnlyList<string> VisitorGuidelines { get; init; } = [];

    public bool HasWhatsApp => !string.IsNullOrWhiteSpace(WhatsAppUrl);
}

public sealed class SectionCardViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}
