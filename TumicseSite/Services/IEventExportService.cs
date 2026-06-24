using TumicseSite.Models;

namespace TumicseSite.Services;

public interface IEventExportService
{
    byte[] BuildIcs(IEnumerable<Event> events);

    byte[] BuildCsv(IEnumerable<Event> events);

    byte[] BuildPdf(IEnumerable<Event> events, string documentTitle, bool includeAdministrativeFields = false);
}
