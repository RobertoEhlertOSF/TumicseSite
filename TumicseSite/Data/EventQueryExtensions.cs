using TumicseSite.Models;
using TumicseSite.Utilities;

namespace TumicseSite.Data;

public static class EventQueryExtensions
{
    public static IQueryable<Event> ApplyEventTypeFilter(
        this IQueryable<Event> query,
        string? eventType,
        bool birthdaysOnly = false)
    {
        if (birthdaysOnly)
        {
            return query.Where(item => item.EventType == CalendarEventType.Birthday);
        }

        if (!EventTypeCatalog.TryParse(eventType, out var parsedEventType))
        {
            return query;
        }

        return query.Where(item => item.EventType == parsedEventType);
    }

    public static IQueryable<Event> ApplyDateRangeFilter(
        this IQueryable<Event> query,
        DateOnly? from,
        DateOnly? to)
    {
        if (from is not null)
        {
            var fromBoundary = TumicseTimeZone.FromLocalDateTime(from.Value.ToDateTime(TimeOnly.MinValue));
            query = query.Where(item => (item.EndDate ?? item.StartDate) >= fromBoundary);
        }

        if (to is not null)
        {
            var toBoundary = TumicseTimeZone.FromLocalDateTime(to.Value.ToDateTime(TimeOnly.MinValue).AddDays(1).AddTicks(-1));
            query = query.Where(item => item.StartDate <= toBoundary);
        }

        return query;
    }

    public static IQueryable<Event> ApplyFutureOnlyFilter(
        this IQueryable<Event> query,
        DateTimeOffset referenceDate)
    {
        return query.Where(item => (item.EndDate ?? item.StartDate) >= referenceDate);
    }
}
