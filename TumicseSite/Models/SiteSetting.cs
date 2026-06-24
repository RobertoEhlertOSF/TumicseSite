using System.ComponentModel.DataAnnotations;

namespace TumicseSite.Models;

public sealed class SiteSetting
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Value { get; set; } = string.Empty;
}
