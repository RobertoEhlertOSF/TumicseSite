namespace TumicseSite.Models;

public class LessonVideo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string YouTubeVideoId { get; set; } = string.Empty;
    public Guid VideoCategoryId { get; set; }
    public VideoCategory VideoCategory { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
