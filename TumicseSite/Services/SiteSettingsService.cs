using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Data;

namespace TumicseSite.Services;

public sealed class SiteSettingsService(
    ApplicationDbContext dbContext,
    ILogger<SiteSettingsService> logger) : ISiteSettingsService
{
    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        try
        {
            return await dbContext.SiteSettings
                .AsNoTracking()
                .Where(setting => setting.Key == key)
                .Select(setting => setting.Value)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (DbException exception)
        {
            logger.LogWarning(exception, "Unable to read site setting {SettingKey}.", key);
            return null;
        }
    }

    public async Task<IReadOnlyDictionary<string, string>> GetValuesAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var normalizedKeys = keys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedKeys.Length == 0)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var settings = await dbContext.SiteSettings
                .AsNoTracking()
                .Where(setting => normalizedKeys.Contains(setting.Key))
                .ToListAsync(cancellationToken);

            return settings.ToDictionary(setting => setting.Key, setting => setting.Value, StringComparer.OrdinalIgnoreCase);
        }
        catch (DbException exception)
        {
            logger.LogWarning(exception, "Unable to read site settings.");
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
