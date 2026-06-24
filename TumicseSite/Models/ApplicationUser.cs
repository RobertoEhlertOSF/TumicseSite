using Microsoft.AspNetCore.Identity;

namespace TumicseSite.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
