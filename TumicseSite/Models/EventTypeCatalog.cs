namespace TumicseSite.Models;

public static class EventTypeCatalog
{
    public static IReadOnlyList<CalendarEventType> All { get; } = Enum.GetValues<CalendarEventType>();

    public static bool TryParse(string? value, out CalendarEventType eventType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            eventType = default;
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out eventType) &&
               All.Contains(eventType);
    }

    public static string GetDisplayName(CalendarEventType eventType) =>
        eventType switch
        {
            CalendarEventType.PublicWork => "Trabalho publico",
            CalendarEventType.PrivateWork => "Trabalho privado",
            CalendarEventType.Gira => "Gira",
            CalendarEventType.Development => "Desenvolvimento",
            CalendarEventType.Study => "Estudo",
            CalendarEventType.Lecture => "Palestra",
            CalendarEventType.Feast => "Festa",
            CalendarEventType.Birthday => "Aniversario",
            CalendarEventType.Maintenance => "Manutencao",
            _ => "Outro"
        };

    public static bool IsPrivateByDefault(CalendarEventType eventType) =>
        eventType is CalendarEventType.PrivateWork or CalendarEventType.Maintenance;
}
