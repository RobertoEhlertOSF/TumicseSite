using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Data;
using TumicseSite.ViewModels;

namespace TumicseSite.Controllers;

[Authorize(Roles = $"{IdentityRoles.Admin},{IdentityRoles.Medium}")]
public class AulasController(ApplicationDbContext context) : Controller
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

    public async Task<IActionResult> Index(string? categoria, CancellationToken cancellationToken)
    {
        var categories = await context.VideoCategories
            .AsNoTracking()
            .Include(category => category.LessonVideos)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);

        var categorySections = categories
            .Select(category => new AulasCategorySectionViewModel
            {
                Name = category.Name,
                Slug = CreateSlug(category.Name),
                LessonCount = category.LessonVideos.Count,
                Lessons = category.LessonVideos
                    .OrderBy(video => video.DisplayOrder)
                    .ThenBy(video => video.CreatedAt)
                    .ThenBy(video => video.Title)
                    .Select(video => new AulasLessonCardViewModel
                    {
                        Id = video.Id,
                        Title = video.Title,
                        CategoryName = category.Name,
                        CategorySlug = CreateSlug(category.Name),
                        YouTubeVideoId = video.YouTubeVideoId,
                        PublishedAtLabel = video.CreatedAt.ToString("dd/MM/yyyy", PtBrCulture)
                    })
                    .ToArray()
            })
            .Where(category => category.LessonCount > 0)
            .ToArray();

        var selectedCategory = categorySections.FirstOrDefault(category =>
            string.Equals(category.Slug, categoria, StringComparison.OrdinalIgnoreCase));

        var model = new AulasIndexViewModel
        {
            SelectedCategoryName = selectedCategory?.Name,
            SelectedCategorySlug = selectedCategory?.Slug,
            Filters =
            [
                new AulasCategoryFilterViewModel
                {
                    Label = "Todas",
                    Slug = null,
                    IsSelected = selectedCategory is null,
                    LessonCount = categorySections.Sum(category => category.LessonCount)
                },
                .. categorySections.Select(category => new AulasCategoryFilterViewModel
                {
                    Label = category.Name,
                    Slug = category.Slug,
                    IsSelected = selectedCategory?.Slug == category.Slug,
                    LessonCount = category.LessonCount
                })
            ],
            Categories = categorySections
        };

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id, string? categoria, CancellationToken cancellationToken)
    {
        var lesson = await context.LessonVideos
            .AsNoTracking()
            .Include(video => video.VideoCategory)
            .FirstOrDefaultAsync(video => video.Id == id, cancellationToken);

        if (lesson is null)
        {
            return NotFound();
        }

        var categorySlug = CreateSlug(lesson.VideoCategory.Name);

        var relatedLessons = (await context.LessonVideos
            .AsNoTracking()
            .Where(video => video.VideoCategoryId == lesson.VideoCategoryId && video.Id != lesson.Id)
            .OrderBy(video => video.DisplayOrder)
            .ThenBy(video => video.CreatedAt)
            .ThenBy(video => video.Title)
            .ToListAsync(cancellationToken))
            .Select(video => new AulasLessonCardViewModel
            {
                Id = video.Id,
                Title = video.Title,
                CategoryName = lesson.VideoCategory.Name,
                CategorySlug = categorySlug,
                YouTubeVideoId = video.YouTubeVideoId,
                PublishedAtLabel = video.CreatedAt.ToString("dd/MM/yyyy", PtBrCulture)
            })
            .ToArray();

        ViewData["SelectedCategory"] = categoria;

        var model = new AulasDetailsViewModel
        {
            Id = lesson.Id,
            Title = lesson.Title,
            CategoryName = lesson.VideoCategory.Name,
            CategorySlug = categorySlug,
            YouTubeVideoId = lesson.YouTubeVideoId,
            PublishedAtLabel = lesson.CreatedAt.ToString("dd/MM/yyyy", PtBrCulture),
            RelatedLessons = relatedLessons
        };

        return View(model);
    }

    private static string CreateSlug(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var previousWasSeparator = false;

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
                previousWasSeparator = false;
                continue;
            }

            if (previousWasSeparator || builder.Length == 0)
            {
                continue;
            }

            builder.Append('-');
            previousWasSeparator = true;
        }

        return builder.ToString().Trim('-');
    }
}
