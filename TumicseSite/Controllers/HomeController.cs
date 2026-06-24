using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TumicseSite.Models;
using TumicseSite.Services;
using TumicseSite.ViewModels;

namespace TumicseSite.Controllers;

public class HomeController(ISiteSettingsService siteSettingsService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var siteSettings = await siteSettingsService.GetValuesAsync(
            new[]
            {
                SiteSettingKeys.SiteName,
                SiteSettingKeys.WhatsAppNumber,
                SiteSettingKeys.WhatsAppDefaultMessage,
                SiteSettingKeys.InstagramUrl,
                SiteSettingKeys.Address,
                SiteSettingKeys.GoogleMapsUrl
            },
            cancellationToken);

        const string defaultAddress = "Rua Assis, 96 - Baeta Neves - São Bernardo do Campo/SP";
        const string defaultInstagramUrl = "https://www.instagram.com/tumicse_oficial/";
        const string defaultMapsUrl = "https://www.google.com/maps/search/?api=1&query=Rua%20Assis%2C%2096%20-%20Baeta%20Neves%20-%20S%C3%A3o%20Bernardo%20do%20Campo%2FSP";

        var model = new HomeIndexViewModel
        {
            SiteName = GetSetting(siteSettings, SiteSettingKeys.SiteName, "TUMICSE")!,
            WhatsAppUrl = WhatsAppFloatingButtonViewModel.BuildUrl(
                GetSetting(siteSettings, SiteSettingKeys.WhatsAppNumber),
                GetSetting(siteSettings, SiteSettingKeys.WhatsAppDefaultMessage)),
            InstagramUrl = GetSetting(siteSettings, SiteSettingKeys.InstagramUrl, defaultInstagramUrl)!,
            Address = GetSetting(siteSettings, SiteSettingKeys.Address, defaultAddress)!,
            GoogleMapsUrl = GetSetting(siteSettings, SiteSettingKeys.GoogleMapsUrl, defaultMapsUrl)!,
            HeroHighlights =
            [
                "Umbanda Sagrada",
                "Acolhimento e caridade",
                "São Bernardo do Campo/SP"
            ],
            AboutCards =
            [
                new SectionCardViewModel
                {
                    Title = "Casa de Umbanda Sagrada",
                    Description = "O TUMICSE é uma casa de Umbanda Sagrada localizada em São Bernardo do Campo, construída sobre acolhimento, respeito aos fundamentos e compromisso com a caridade."
                },
                new SectionCardViewModel
                {
                    Title = "Atuação desde 2011",
                    Description = "Com fundação cadastral pública em 22/07/2011, a casa mantém uma presença viva, organizada e comprometida com a caminhada espiritual de quem a procura."
                },
                new SectionCardViewModel
                {
                    Title = "Ancestralidade e desenvolvimento",
                    Description = "A vivência da casa valoriza ancestralidade, fé, desenvolvimento espiritual, disciplina coletiva e conexão respeitosa com os Orixás e Guias Espirituais."
                }
            ],
            GiraCards =
            [
                new SectionCardViewModel
                {
                    Title = "Trabalho espiritual e acolhimento",
                    Description = "As giras são momentos de trabalho espiritual, oração e acolhimento fraterno, conduzidos com serenidade, organização e respeito à tradição da casa."
                },
                new SectionCardViewModel
                {
                    Title = "Agenda pública e avisos",
                    Description = "Horários, avisos, orientações e eventuais atualizações devem sempre ser consultados nos canais oficiais do TUMICSE, especialmente no Instagram da casa."
                },
                new SectionCardViewModel
                {
                    Title = "Informação oficial da casa",
                    Description = "Sempre que houver dúvidas sobre presença, atendimento, funcionamento ou agenda, a referência deve ser a equipe da casa e seus canais públicos oficiais."
                }
            ],
            OpenGiraScheduleItems =
            [
                "Giras abertas em todos os últimos domingos do mês.",
                "Abertura dos portões às 8h.",
                "Início da gira às 9h.",
                "Os portões se fecham às 10h."
            ],
            VisitorGuidelines =
            [
                "Respeite o silêncio e a organização da gira.",
                "Siga as orientações dos cambonos e da equipe da casa.",
                "Mantenha o celular no silencioso durante os trabalhos.",
                "Não fotografe ou filme sem autorização.",
                "Chegue no horário indicado para a gira ou atendimento.",
                "Procure a equipe da casa em caso de dúvidas."
            ]
        };

        return View(model);
    }

    [Authorize(Roles = "Admin,Medium")]
    public IActionResult Classes()
    {
        return RedirectToAction("Index", "Aulas");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static IReadOnlyList<LessonGroupDefinition> BuildLessonCatalog() =>
    [
        new LessonGroupDefinition(
            "Aulas 2020",
            [
                new LessonDefinition("historia-da-umbanda", "História da Umbanda", "2020", "Uma aula de base para compreender origem, identidade e caminho espiritual da Umbanda.", "sRPx6VTLMmw"),
                new LessonDefinition("boiadeiros", "Boiadeiros", "2020", "Um olhar sobre firmeza, simbolismo e presença dessa linha de trabalho na Umbanda.", "rfKETMoerGw"),
                new LessonDefinition("marinheiros", "Marinheiros", "2020", "Reflexões sobre movimento, adaptação e ensinamentos espirituais dessa corrente.", "A1kBBB4UuoY"),
                new LessonDefinition("baianos", "Baianos", "2020", "Uma aula dedicada à alegria, à sabedoria popular e ao acolhimento dos Baianos.", "X2YD18cUrg0")
            ]),
        new LessonGroupDefinition(
            "Aulas 2021",
            [
                new LessonDefinition("coroa-mediunica", "Coroa Mediúnica", "2021", "Estudo introdutório sobre estrutura espiritual, sensibilidade e responsabilidade mediúnica.", "QZiU7TSoeJk"),
                new LessonDefinition("educacao-mediunica", "Educação Mediúnica", "2021", "Uma abordagem cuidadosa sobre disciplina, ética e amadurecimento no desenvolvimento.", "EBsQ0Tuw4Rg"),
                new LessonDefinition("desenvolvimento-mediunico", "Desenvolv. Mediúnico", "2021", "Aspectos centrais da caminhada mediúnica para quem busca constância e fundamento.", "lysOndPYrMo"),
                new LessonDefinition("desenvolvimento-mediunico-ii", "Desenvolv. Mediúnico II", "2021", "Continuidade do estudo com aprofundamento sobre conduta, equilíbrio e preparo.", "EBsQ0Tuw4Rg")
            ])
    ];

    private static string? GetSetting(IReadOnlyDictionary<string, string> settings, string key, string? fallback = null)
    {
        if (!settings.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return value.Trim();
    }

    private sealed record LessonGroupDefinition(string Title, IReadOnlyList<LessonDefinition> Lessons);

    private sealed record LessonDefinition(string Slug, string Title, string YearLabel, string Summary, string YouTubeVideoId);
}
