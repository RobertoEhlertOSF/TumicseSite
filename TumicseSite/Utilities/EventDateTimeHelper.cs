using TumicseSite.Models;

namespace TumicseSite.Utilities;

public static class EventDateTimeHelper
{
    public static DateTimeOffset ToStoredStartDate(DateTime startLocal, bool isAllDay)
    {
        var normalizedStart = isAllDay ? startLocal.Date : startLocal;
        return TumicseTimeZone.FromLocalDateTime(normalizedStart);
    }

    public static DateTimeOffset? ToStoredEndDate(DateTime? endLocal, DateTime startLocal, bool isAllDay)
    {
        if (endLocal is null)
        {
            if (!isAllDay)
            {
                return null;
            }

            endLocal = startLocal.Date;
        }

        var normalizedEnd = isAllDay
            ? endLocal.Value.Date.AddDays(1).AddTicks(-1)
            : endLocal.Value;

        return TumicseTimeZone.FromLocalDateTime(normalizedEnd);
    }

    public static DateTime ToEditableStartLocal(Event item)
    {
        var localStart = TumicseTimeZone.ToLocalDateTime(item.StartDate);
        return item.IsAllDay ? localStart.Date : localStart;
    }

    public static DateTime? ToEditableEndLocal(Event item)
    {
        if (item.EndDate is null)
        {
            return null;
        }

        var localEnd = TumicseTimeZone.ToLocalDateTime(item.EndDate.Value);
        return item.IsAllDay ? localEnd.Date : localEnd;
    }

    public static DateTime ToLocalStartDate(Event item) =>
        TumicseTimeZone.ToLocalDateTime(item.StartDate);

    public static DateTime? ToLocalEndDate(Event item)
    {
        if (item.EndDate is null)
        {
            return null;
        }

        var localEnd = TumicseTimeZone.ToLocalDateTime(item.EndDate.Value);
        return item.IsAllDay ? localEnd.Date : localEnd;
    }
}
