namespace TumicseSite.Data;

public static class IdentityRoles
{
    public const string Admin = "Admin";
    public const string Medium = "Medium";

    public static IReadOnlyList<string> All { get; } = [Admin, Medium];
}
