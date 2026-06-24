namespace TumicseSite.Utilities;

public static class TumicseTimeZone
{
    public static TimeZoneInfo SaoPaulo { get; } = ResolveSaoPauloTimeZone();

    public static DateTimeOffset FromLocalDateTime(DateTime localDateTime)
    {
        var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        return new DateTimeOffset(unspecified, SaoPaulo.GetUtcOffset(unspecified));
    }

    public static DateTime ToLocalDateTime(DateTimeOffset dateTimeOffset) =>
        TimeZoneInfo.ConvertTime(dateTimeOffset, SaoPaulo).DateTime;

    private static TimeZoneInfo ResolveSaoPauloTimeZone()
    {
        foreach (var timeZoneId in new[] { "E. South America Standard Time", "America/Sao_Paulo" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Local;
    }
}
