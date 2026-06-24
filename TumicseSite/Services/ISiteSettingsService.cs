namespace TumicseSite.Services;

public interface ISiteSettingsService
{
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, string>> GetValuesAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);
}
