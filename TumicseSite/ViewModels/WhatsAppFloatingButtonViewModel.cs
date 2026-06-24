namespace TumicseSite.ViewModels;

public sealed class WhatsAppFloatingButtonViewModel
{
    public string Url { get; private init; } = string.Empty;

    public string Label { get; private init; } = "Conversar no WhatsApp";

    public static WhatsAppFloatingButtonViewModel? Create(string? phoneNumber, string? defaultMessage)
    {
        var url = BuildUrl(phoneNumber, defaultMessage);

        return string.IsNullOrWhiteSpace(url)
            ? null
            : new WhatsAppFloatingButtonViewModel { Url = url };
    }

    public static string? BuildUrl(string? phoneNumber, string? defaultMessage)
    {
        var sanitizedPhoneNumber = SanitizePhoneNumber(phoneNumber);
        if (string.IsNullOrWhiteSpace(sanitizedPhoneNumber))
        {
            return null;
        }

        var link = $"https://wa.me/{sanitizedPhoneNumber}";
        if (string.IsNullOrWhiteSpace(defaultMessage))
        {
            return link;
        }

        return $"{link}?text={Uri.EscapeDataString(defaultMessage.Trim())}";
    }

    public static string SanitizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }

        return string.Concat(phoneNumber.Where(char.IsDigit));
    }
}
