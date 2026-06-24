namespace TumicseSite.Models;

public static class EventTypeCatalog
{
    public const string GiraDeUmbanda = "Gira de Umbanda";
    public const string Estudo = "Estudo";
    public const string DesenvolvimentoMediunico = "Desenvolvimento Mediunico";
    public const string Reuniao = "Reuniao";
    public const string EventoEspecial = "Evento Especial";
    public const string Atendimento = "Atendimento";
    public const string Outros = "Outros";

    public static IReadOnlyList<string> All { get; } =
    [
        GiraDeUmbanda,
        Estudo,
        DesenvolvimentoMediunico,
        Reuniao,
        EventoEspecial,
        Atendimento,
        Outros
    ];

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        All.Contains(value.Trim(), StringComparer.Ordinal);
}
