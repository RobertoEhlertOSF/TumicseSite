namespace TumicseSite.Models;

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsAllDay { get; set; }
    public string? Location { get; set; }
    public CalendarEventType EventType { get; set; } = CalendarEventType.Other;
    public string? Address { get; set; }
    public string? GoogleMapsUrl { get; set; }
    public bool IsPublic { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool IsCancelled { get; set; }
    public string? InternalNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
