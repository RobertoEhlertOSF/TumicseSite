using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TumicseSite.Models;
using TumicseSite.Utilities;

namespace TumicseSite.Services;

public sealed class EventExportService(IWebHostEnvironment environment) : IEventExportService
{
    private const int MaxVisibleEventsPerDay = 4;

    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly string[] WeekdayHeaders = ["SEG", "TER", "QUA", "QUI", "SEX", "SAB", "DOM"];
    private static readonly string[] ImportantNotes =
    [
        "Horario de chegada no terreiro: Ate as 7h30",
        "Horario de inicio dos trabalhos: 8h",
        "A aula teorica e OBRIGATORIA para todos os mediuns da casa.",
        "Adicionamos ao calendario datas consagradas aos Orixas e as linhas de trabalho.",
        "Estas datas podem variar uma vez que podem ser celebradas em mais de uma data em nosso pais.",
        "Desta forma, optamos por colocar neste calendario apenas as datas adotadas pelo nosso Templo."
    ];

    private static readonly Color BrandGreen = Color.FromHex("#009846");
    private static readonly Color BrandGreenDeep = Color.FromHex("#0A6F3C");
    private static readonly Color BrandGold = Color.FromHex("#F2B705");
    private static readonly Color BrandGoldSoft = Color.FromHex("#F7E3A3");
    private static readonly Color BrandNavy = Color.FromHex("#29276F");
    private static readonly Color BrandRed = Color.FromHex("#D71920");
    private static readonly Color BrandBackground = Color.FromHex("#FAFAF7");
    private static readonly Color BrandSurface = Color.FromHex("#FFFFFF");
    private static readonly Color BrandSurfaceSoft = Color.FromHex("#F3F6EE");
    private static readonly Color BrandText = Color.FromHex("#1F2933");
    private static readonly Color BrandMuted = Color.FromHex("#5B6875");
    private static readonly Color BorderSoft = Color.FromHex("#E1E5EA");
    private static readonly Color BorderStrong = Color.FromHex("#CFD6DD");
    private static readonly Color BirthdayBackground = Color.FromHex("#E9F1FF");
    private static readonly Color BirthdayText = Color.FromHex("#1F4C90");
    private static readonly Color BirthdayBorder = Color.FromHex("#5B8DEF");
    private static readonly Color SacredBackground = Color.FromHex("#FDEBE8");
    private static readonly Color SacredText = Color.FromHex("#9D3B2B");
    private static readonly Color SacredBorder = Color.FromHex("#E76F51");
    private static readonly Color PublicBackground = Color.FromHex("#EAF0FA");
    private static readonly Color PublicText = Color.FromHex("#1D3557");
    private static readonly Color PublicBorder = Color.FromHex("#355C96");
    private static readonly Color InternalBackground = Color.FromHex("#ECEFF3");
    private static readonly Color InternalText = Color.FromHex("#374151");
    private static readonly Color InternalBorder = Color.FromHex("#6B7280");
    private static readonly Color StudyBackground = Color.FromHex("#E8F6EE");
    private static readonly Color StudyText = Color.FromHex("#0A6F3C");
    private static readonly Color StudyBorder = Color.FromHex("#2F855A");
    private static readonly Color FeastBackground = Color.FromHex("#FFF1D6");
    private static readonly Color FeastText = Color.FromHex("#8C5A00");
    private static readonly Color FeastBorder = Color.FromHex("#C78914");

    private readonly string? logoPath = ResolveLogoPath(environment);

    static EventExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] BuildIcs(IEnumerable<Event> events)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BEGIN:VCALENDAR");
        builder.AppendLine("VERSION:2.0");
        builder.AppendLine("PRODID:-//TUMICSE//Calendar//PT-BR");
        builder.AppendLine("CALSCALE:GREGORIAN");
        builder.AppendLine("METHOD:PUBLISH");
        builder.AppendLine("X-WR-CALNAME:TUMICSE");
        builder.AppendLine("X-WR-TIMEZONE:America/Sao_Paulo");

        foreach (var item in events.OrderBy(eventItem => eventItem.StartDate).ThenBy(eventItem => eventItem.Title))
        {
            builder.AppendLine("BEGIN:VEVENT");
            builder.AppendLine($"UID:{item.Id}@tumicse.local");
            builder.AppendLine($"DTSTAMP:{DateTimeOffset.UtcNow:yyyyMMdd'T'HHmmss'Z'}");

            if (item.IsAllDay)
            {
                var localStart = EventDateTimeHelper.ToLocalStartDate(item).Date;
                var localEnd = EventDateTimeHelper.ToLocalEndDate(item)?.Date ?? localStart;
                var exclusiveEnd = localEnd.AddDays(1);

                builder.AppendLine($"DTSTART;VALUE=DATE:{localStart:yyyyMMdd}");
                builder.AppendLine($"DTEND;VALUE=DATE:{exclusiveEnd:yyyyMMdd}");
            }
            else
            {
                builder.AppendLine($"DTSTART:{item.StartDate.UtcDateTime:yyyyMMdd'T'HHmmss'Z'}");

                var effectiveEnd = item.EndDate ?? item.StartDate;
                builder.AppendLine($"DTEND:{effectiveEnd.UtcDateTime:yyyyMMdd'T'HHmmss'Z'}");
            }

            builder.AppendLine($"SUMMARY:{EscapeIcsText(item.Title)}");
            builder.AppendLine($"DESCRIPTION:{EscapeIcsText(item.Description)}");
            builder.AppendLine($"LOCATION:{EscapeIcsText(item.Location)}");
            builder.AppendLine($"CATEGORIES:{item.EventType}");

            if (item.IsCancelled)
            {
                builder.AppendLine("STATUS:CANCELLED");
            }

            builder.AppendLine("END:VEVENT");
        }

        builder.AppendLine("END:VCALENDAR");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public byte[] BuildCsv(IEnumerable<Event> events)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Id,Title,Description,EventType,StartDate,EndDate,IsAllDay,Location,IsPublic,IsActive,IsCancelled");

        foreach (var item in events.OrderBy(eventItem => eventItem.StartDate).ThenBy(eventItem => eventItem.Title))
        {
            var values = new[]
            {
                item.Id.ToString(),
                item.Title,
                item.Description ?? string.Empty,
                item.EventType.ToString(),
                item.StartDate.ToString("O", InvariantCulture),
                (item.EndDate ?? item.StartDate).ToString("O", InvariantCulture),
                item.IsAllDay.ToString(InvariantCulture),
                item.Location ?? string.Empty,
                item.IsPublic.ToString(InvariantCulture),
                item.IsActive.ToString(InvariantCulture),
                item.IsCancelled.ToString(InvariantCulture)
            };

            builder.AppendLine(string.Join(",", values.Select(EscapeCsvField)));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public byte[] BuildPdf(IEnumerable<Event> events, string documentTitle, bool includeAdministrativeFields = false)
    {
        var orderedEvents = events
            .OrderBy(eventItem => eventItem.StartDate)
            .ThenBy(eventItem => eventItem.Title)
            .ToArray();

        var months = BuildCalendarMonths(orderedEvents);
        var yearLabel = ResolveYearLabel(months);
        var coverTitle = yearLabel is null
            ? "Calendario TUMICSE"
            : $"Calendario {yearLabel} TUMICSE";
        var contextLabel = includeAdministrativeFields
            ? "Versao administrativa"
            : "Versao publica";

        return Document.Create(document =>
        {
            document.Page(page => BuildCoverPage(page, coverTitle, documentTitle, contextLabel));
            document.Page(page => BuildImportantNotesPage(page));

            if (months.Count == 0)
            {
                document.Page(page => BuildEmptyCalendarPage(page, coverTitle));
                return;
            }

            foreach (var month in months)
            {
                document.Page(page => BuildMonthCalendarPage(page, month));
            }
        }).GeneratePdf();
    }

    private void BuildCoverPage(PageDescriptor page, string coverTitle, string documentTitle, string contextLabel)
    {
        page.Size(PageSizes.A4);
        page.Margin(0);
        page.PageColor(BrandBackground);
        page.DefaultTextStyle(style => style.FontColor(BrandText).FontSize(11));

        page.Content().Column(column =>
        {
            column.Item().Height(86).Background(BrandNavy);

            column.Item().Extend().PaddingHorizontal(52).PaddingVertical(36).Column(content =>
            {
                content.Spacing(18);

                if (logoPath is not null)
                {
                    content.Item()
                        .AlignCenter()
                        .Width(255)
                        .Height(255)
                        .Image(logoPath)
                        .FitArea();
                }

                content.Item()
                    .AlignCenter()
                    .Text("TUMICSE")
                    .SemiBold()
                    .FontSize(16)
                    .LetterSpacing(4)
                    .FontColor(BrandGreen);

                content.Item()
                    .AlignCenter()
                    .Text(coverTitle)
                    .Bold()
                    .FontSize(30)
                    .FontColor(BrandNavy);

                content.Item()
                    .AlignCenter()
                    .Text(documentTitle)
                    .FontSize(15)
                    .FontColor(BrandMuted);

                content.Item()
                    .AlignCenter()
                    .Text(contextLabel)
                    .FontSize(12)
                    .FontColor(BrandGreenDeep);

                content.Item()
                    .PaddingTop(8)
                    .LineHorizontal(1)
                    .LineColor(BrandGold);

                content.Item()
                    .AlignCenter()
                    .Text("Calendario institucional anual")
                    .FontSize(11)
                    .FontColor(BrandMuted);
            });

            column.Item().Height(26).Row(row =>
            {
                row.RelativeItem().Background(BrandGreen);
                row.ConstantItem(160).Background(BrandGold);
            });
        });
    }

    private static void BuildImportantNotesPage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(42);
        page.PageColor(BrandSurface);
        page.DefaultTextStyle(style => style.FontColor(BrandText).FontSize(12).LineHeight(1.35f));

        page.Content().Column(column =>
        {
            column.Spacing(20);

            column.Item()
                .Text("RECADOS IMPORTANTES")
                .Bold()
                .FontSize(28)
                .FontColor(BrandNavy);

            column.Item()
                .LineHorizontal(1)
                .LineColor(BrandGold);

            column.Item()
                .Border(1)
                .BorderColor(BrandGold)
                .Background(BrandGoldSoft)
                .Padding(18)
                .Text(text =>
                {
                    text.Line(ImportantNotes[0]).SemiBold();
                    text.Line(ImportantNotes[1]).SemiBold();
                    text.EmptyLine();
                    text.Line(ImportantNotes[2]).Bold().FontColor(BrandRed);
                });

            column.Item()
                .Border(1)
                .BorderColor(BorderStrong)
                .Background(BrandBackground)
                .Padding(20)
                .Column(notes =>
                {
                    notes.Spacing(14);

                    foreach (var line in ImportantNotes.Skip(3))
                    {
                        notes.Item().Text(line);
                    }
                });
        });
    }

    private void BuildEmptyCalendarPage(PageDescriptor page, string coverTitle)
    {
        page.Size(PageSizes.A4.Landscape());
        page.Margin(24);
        page.PageColor(BrandBackground);
        page.DefaultTextStyle(style => style.FontColor(BrandText).FontSize(12));

        page.Content().AlignCenter().AlignMiddle().Column(column =>
        {
            column.Spacing(14);

            column.Item()
                .Text(coverTitle)
                .Bold()
                .FontSize(26)
                .FontColor(BrandNavy)
                .AlignCenter();

            column.Item()
                .Text("Nenhum evento encontrado para os filtros informados.")
                .FontSize(14)
                .FontColor(BrandMuted)
                .AlignCenter();

            if (logoPath is not null)
            {
                column.Item()
                    .PaddingTop(10)
                    .AlignCenter()
                    .Width(120)
                    .Height(120)
                    .Image(logoPath)
                    .FitArea();
            }
        });
    }

    private void BuildMonthCalendarPage(PageDescriptor page, CalendarMonthModel month)
    {
        page.Size(PageSizes.A4.Landscape());
        page.Margin(18);
        page.PageColor(BrandBackground);
        page.DefaultTextStyle(style => style.FontColor(BrandText).FontSize(9).LineHeight(1.1f));

        page.Content().Row(row =>
        {
            row.ConstantItem(92).Element(container => BuildMonthSidebar(container, month));

            row.RelativeItem().PaddingLeft(12).Column(column =>
            {
                column.Item().Row(header =>
                {
                    header.RelativeItem().Column(title =>
                    {
                        title.Spacing(2);
                        title.Item()
                            .Text("CALENDARIO INSTITUCIONAL")
                            .SemiBold()
                            .FontSize(10)
                            .LetterSpacing(2)
                            .FontColor(BrandGreenDeep);

                        title.Item()
                            .Text($"{GetMonthLabel(month.Month)} de {month.Year}")
                            .Bold()
                            .FontSize(22)
                            .FontColor(BrandNavy);
                    });

                    header.ConstantItem(120).AlignRight().AlignMiddle().Column(info =>
                    {
                        info.Spacing(6);

                        if (logoPath is not null)
                        {
                            info.Item()
                                .AlignRight()
                                .Width(40)
                                .Height(40)
                                .Image(logoPath)
                                .FitArea();
                        }

                        info.Item()
                            .AlignRight()
                            .Text(month.Year.ToString(InvariantCulture))
                            .Bold()
                            .FontSize(22)
                            .FontColor(BrandGold);
                    });
                });

                column.Item().PaddingTop(10).ScaleToFit().Element(container => BuildMonthGrid(container, month));
            });
        });

        page.Footer().AlignRight().Text(text =>
        {
            text.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(BrandMuted));
            text.Span("TUMICSE  ");
            text.Span("Pagina ");
            text.CurrentPageNumber();
        });
    }

    private void BuildMonthSidebar(IContainer container, CalendarMonthModel month)
    {
        container
            .Background(BrandNavy)
            .PaddingVertical(16)
            .PaddingHorizontal(8)
            .Column(column =>
            {
                column.Item()
                    .AlignCenter()
                    .Text(month.Year.ToString(InvariantCulture))
                    .SemiBold()
                    .FontSize(11)
                    .LetterSpacing(1)
                    .FontColor(BrandGoldSoft);

                column.Item()
                    .Extend()
                    .AlignCenter()
                    .AlignMiddle()
                    .RotateLeft()
                    .Text(GetMonthLabel(month.Month).ToUpper(PtBrCulture))
                    .Bold()
                    .FontSize(28)
                    .FontColor(BrandGoldSoft);
            });
    }

    private void BuildMonthGrid(IContainer container, CalendarMonthModel month)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                for (var columnIndex = 0; columnIndex < WeekdayHeaders.Length; columnIndex++)
                {
                    columns.RelativeColumn();
                }
            });

            foreach (var weekdayHeader in WeekdayHeaders)
            {
                table.Cell().Element(StyleWeekdayHeaderCell).Height(28).AlignCenter().AlignMiddle()
                    .Text(weekdayHeader)
                    .SemiBold()
                    .FontSize(9)
                    .FontColor(BrandSurface);
            }

            foreach (var dayCell in month.Days)
            {
                table.Cell().Element(cell => StyleDayCell(cell, dayCell.IsCurrentMonth)).Height(74).Column(column =>
                {
                    column.Spacing(3);

                    column.Item().AlignRight().Text(dayCell.Date.Day.ToString(InvariantCulture))
                        .SemiBold()
                        .FontSize(9)
                        .FontColor(dayCell.IsCurrentMonth ? BrandNavy : BrandMuted);

                    var dayEvents = dayCell.Events.Take(MaxVisibleEventsPerDay).ToArray();
                    foreach (var dayEvent in dayEvents)
                    {
                        column.Item().Element(itemContainer => BuildDayEvent(itemContainer, dayEvent, dayCell.Events));
                    }

                    if (dayCell.Events.Count > MaxVisibleEventsPerDay)
                    {
                        column.Item().Text($"+{dayCell.Events.Count - MaxVisibleEventsPerDay} evento(s)")
                            .FontSize(7)
                            .FontColor(BrandMuted);
                    }
                });
            }
        });
    }

    private void BuildDayEvent(IContainer container, Event item, IReadOnlyList<Event> dayEvents)
    {
        var visualStyle = GetEventVisualStyle(item);
        var textValue = FormatEventText(item, dayEvents);

        container
            .Border(1)
            .BorderColor(visualStyle.Border)
            .Background(visualStyle.Background)
            .PaddingHorizontal(4)
            .PaddingVertical(3)
            .Text(text =>
            {
                text.ClampLines(1, "...");
                text.Span(textValue)
                    .SemiBold()
                    .FontSize(7.2f)
                    .FontColor(visualStyle.Text);
            });
    }

    private static IContainer StyleWeekdayHeaderCell(IContainer container) =>
        container
            .Background(BrandNavy)
            .Border(1)
            .BorderColor(BrandNavy)
            .PaddingVertical(5);

    private static IContainer StyleDayCell(IContainer container, bool isCurrentMonth) =>
        container
            .Border(1)
            .BorderColor(isCurrentMonth ? BorderStrong : BorderSoft)
            .Background(isCurrentMonth ? BrandSurface : BrandSurfaceSoft)
            .Padding(5);

    private static IReadOnlyList<CalendarMonthModel> BuildCalendarMonths(IReadOnlyList<Event> events)
    {
        if (events.Count == 0)
        {
            return [];
        }

        var eventLookup = BuildEventLookup(events);
        var firstLocalDate = eventLookup.Keys.Min().ToDateTime(TimeOnly.MinValue);
        var lastLocalDate = eventLookup.Keys.Max().ToDateTime(TimeOnly.MinValue);
        var monthCursor = new DateTime(firstLocalDate.Year, firstLocalDate.Month, 1);
        var lastMonth = new DateTime(lastLocalDate.Year, lastLocalDate.Month, 1);
        var months = new List<CalendarMonthModel>();

        while (monthCursor <= lastMonth)
        {
            months.Add(new CalendarMonthModel(
                monthCursor.Year,
                monthCursor.Month,
                BuildDayCells(monthCursor.Year, monthCursor.Month, eventLookup)));

            monthCursor = monthCursor.AddMonths(1);
        }

        return months;
    }

    private static Dictionary<DateOnly, List<Event>> BuildEventLookup(IEnumerable<Event> events)
    {
        var lookup = new Dictionary<DateOnly, List<Event>>();

        foreach (var item in events)
        {
            var localStartDate = DateOnly.FromDateTime(EventDateTimeHelper.ToLocalStartDate(item).Date);
            var localEnd = EventDateTimeHelper.ToLocalEndDate(item) ?? EventDateTimeHelper.ToLocalStartDate(item);
            var localEndDate = DateOnly.FromDateTime(localEnd.Date);

            for (var day = localStartDate; day <= localEndDate; day = day.AddDays(1))
            {
                if (!lookup.TryGetValue(day, out var dayEvents))
                {
                    dayEvents = [];
                    lookup[day] = dayEvents;
                }

                dayEvents.Add(item);
            }
        }

        foreach (var dayEvents in lookup.Values)
        {
            dayEvents.Sort(static (left, right) =>
            {
                var startComparison = left.StartDate.CompareTo(right.StartDate);
                if (startComparison != 0)
                {
                    return startComparison;
                }

                return string.Compare(left.Title, right.Title, StringComparison.OrdinalIgnoreCase);
            });
        }

        return lookup;
    }

    private static IReadOnlyList<CalendarDayCellModel> BuildDayCells(
        int year,
        int month,
        IReadOnlyDictionary<DateOnly, List<Event>> eventLookup)
    {
        var firstDayOfMonth = new DateTime(year, month, 1);
        var offsetFromMonday = GetMondayBasedDayOfWeek(firstDayOfMonth);
        var firstVisibleDay = firstDayOfMonth.AddDays(-offsetFromMonday);
        var cells = new List<CalendarDayCellModel>(42);

        for (var index = 0; index < 42; index++)
        {
            var currentDay = firstVisibleDay.AddDays(index);
            var date = DateOnly.FromDateTime(currentDay);
            eventLookup.TryGetValue(date, out var dayEvents);

            cells.Add(new CalendarDayCellModel(
                date,
                currentDay.Month == month,
                dayEvents?.ToArray() ?? []));
        }

        return cells;
    }

    private static int GetMondayBasedDayOfWeek(DateTime value) =>
        ((int)value.DayOfWeek + 6) % 7;

    private static string? ResolveYearLabel(IReadOnlyList<CalendarMonthModel> months)
    {
        if (months.Count == 0)
        {
            return null;
        }

        var firstYear = months.First().Year;
        var lastYear = months.Last().Year;
        return firstYear == lastYear
            ? firstYear.ToString(InvariantCulture)
            : $"{firstYear}-{lastYear}";
    }

    private static string GetMonthLabel(int month)
    {
        var monthDate = new DateTime(2026, month, 1);
        return PtBrCulture.TextInfo.ToTitleCase(monthDate.ToString("MMMM", PtBrCulture));
    }

    private static string FormatEventText(Event item, IReadOnlyList<Event> dayEvents)
    {
        var title = NormalizeWhitespace(item.Title);
        if (item.IsAllDay)
        {
            return title;
        }

        var localStart = EventDateTimeHelper.ToLocalStartDate(item);
        var showTimePrefix = dayEvents.Count > 1 || localStart.TimeOfDay != TimeSpan.FromHours(8);
        if (!showTimePrefix)
        {
            return title;
        }

        var timeLabel = localStart.Minute == 0
            ? $"{localStart:HH}h"
            : $"{localStart:HH:mm}";

        return $"{timeLabel} {title}";
    }

    private static EventVisualStyle GetEventVisualStyle(Event item)
    {
        if (item.EventType == CalendarEventType.Birthday)
        {
            return new EventVisualStyle(BirthdayBackground, BirthdayText, BirthdayBorder);
        }

        if (item.EventType == CalendarEventType.Gira || item.EventType == CalendarEventType.PublicWork)
        {
            return new EventVisualStyle(PublicBackground, PublicText, PublicBorder);
        }

        if (item.EventType == CalendarEventType.Development || item.EventType == CalendarEventType.PrivateWork)
        {
            return new EventVisualStyle(InternalBackground, InternalText, InternalBorder);
        }

        if (item.EventType == CalendarEventType.Maintenance)
        {
            return new EventVisualStyle(InternalBackground, BrandMuted, BorderStrong);
        }

        if (item.EventType == CalendarEventType.Study || item.EventType == CalendarEventType.Lecture)
        {
            return new EventVisualStyle(StudyBackground, StudyText, StudyBorder);
        }

        if (item.EventType == CalendarEventType.Feast)
        {
            return new EventVisualStyle(FeastBackground, FeastText, FeastBorder);
        }

        if (IsSacredOrSpecialDate(item))
        {
            return new EventVisualStyle(SacredBackground, SacredText, SacredBorder);
        }

        return new EventVisualStyle(BrandSurfaceSoft, BrandText, BorderStrong);
    }

    private static bool IsSacredOrSpecialDate(Event item)
    {
        if (item.EventType != CalendarEventType.Other)
        {
            return false;
        }

        var title = item.Title.AsSpan().Trim();
        return title.Contains("Consagrado", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("Dia dos", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("Dia da", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("Dia de", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeWhitespace(string value) =>
        string.Join(' ', value.Split([' '], StringSplitOptions.RemoveEmptyEntries));

    private static string? ResolveLogoPath(IWebHostEnvironment environment)
    {
        if (string.IsNullOrWhiteSpace(environment.WebRootPath))
        {
            return null;
        }

        var path = Path.Combine(environment.WebRootPath, "tumicse_logo.png");
        return File.Exists(path) ? path : null;
    }

    private static string EscapeIcsText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim()
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace(";", @"\;", StringComparison.Ordinal)
            .Replace(",", @"\,", StringComparison.Ordinal)
            .Replace("\r\n", @"\n", StringComparison.Ordinal)
            .Replace("\n", @"\n", StringComparison.Ordinal);
    }

    private static string EscapeCsvField(string value)
    {
        var escaped = value.Replace("\"", "\"\"", StringComparison.Ordinal);
        return $"\"{escaped}\"";
    }

    private sealed record CalendarMonthModel(int Year, int Month, IReadOnlyList<CalendarDayCellModel> Days);

    private sealed record CalendarDayCellModel(DateOnly Date, bool IsCurrentMonth, IReadOnlyList<Event> Events);

    private sealed record EventVisualStyle(Color Background, Color Text, Color Border);
}
