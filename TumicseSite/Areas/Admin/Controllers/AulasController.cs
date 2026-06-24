using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Data;
using TumicseSite.Models;
using TumicseSite.Utilities;
using TumicseSite.ViewModels;

namespace TumicseSite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = IdentityRoles.Admin)]
public class AulasController(ApplicationDbContext context) : Controller
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var lessons = await context.LessonVideos
            .AsNoTracking()
            .Include(video => video.VideoCategory)
            .OrderBy(video => video.VideoCategory.DisplayOrder)
            .ThenBy(video => video.DisplayOrder)
            .ThenBy(video => video.CreatedAt)
            .ThenBy(video => video.Title)
            .ToListAsync(cancellationToken);

        var model = new AdminAulasIndexViewModel
        {
            Lessons = lessons
                .Select(video => new AdminLessonVideoListItemViewModel
                {
                    Id = video.Id,
                    Title = video.Title,
                    CategoryName = video.VideoCategory.Name,
                    YouTubeVideoId = video.YouTubeVideoId,
                    DisplayOrder = video.DisplayOrder,
                    CreatedAtLabel = video.CreatedAt.ToString("dd/MM/yyyy", PtBrCulture)
                })
                .ToArray()
        };

        return View(model);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return View(await BuildFormViewModelAsync(new AdminLessonVideoFormViewModel(), cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminLessonVideoFormViewModel model, CancellationToken cancellationToken)
    {
        var category = await ValidateLessonFormAsync(model, null, cancellationToken);
        if (category is null)
        {
            return View(await BuildFormViewModelAsync(model, cancellationToken));
        }

        YouTubeVideoReferenceParser.TryExtractVideoId(model.YouTubeReference, out var videoId);

        context.LessonVideos.Add(new LessonVideo
        {
            Title = model.Title.Trim(),
            VideoCategoryId = category.Id,
            YouTubeVideoId = videoId,
            DisplayOrder = model.DisplayOrder
        });

        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Aula cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var lesson = await context.LessonVideos
            .AsNoTracking()
            .FirstOrDefaultAsync(video => video.Id == id, cancellationToken);

        if (lesson is null)
        {
            return NotFound();
        }

        var model = new AdminLessonVideoFormViewModel
        {
            Id = lesson.Id,
            Title = lesson.Title,
            VideoCategoryId = lesson.VideoCategoryId,
            YouTubeReference = lesson.YouTubeVideoId,
            DisplayOrder = lesson.DisplayOrder
        };

        return View(await BuildFormViewModelAsync(model, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AdminLessonVideoFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var lesson = await context.LessonVideos
            .FirstOrDefaultAsync(video => video.Id == id, cancellationToken);

        if (lesson is null)
        {
            return NotFound();
        }

        var category = await ValidateLessonFormAsync(model, id, cancellationToken);
        if (category is null)
        {
            return View(await BuildFormViewModelAsync(model, cancellationToken));
        }

        YouTubeVideoReferenceParser.TryExtractVideoId(model.YouTubeReference, out var videoId);

        lesson.Title = model.Title.Trim();
        lesson.VideoCategoryId = category.Id;
        lesson.YouTubeVideoId = videoId;
        lesson.DisplayOrder = model.DisplayOrder;

        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Aula atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var lesson = await context.LessonVideos
            .AsNoTracking()
            .Include(video => video.VideoCategory)
            .FirstOrDefaultAsync(video => video.Id == id, cancellationToken);

        if (lesson is null)
        {
            return NotFound();
        }

        return View(new AdminLessonVideoDeleteViewModel
        {
            Id = lesson.Id,
            Title = lesson.Title,
            CategoryName = lesson.VideoCategory.Name,
            YouTubeVideoId = lesson.YouTubeVideoId,
            DisplayOrder = lesson.DisplayOrder
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var lesson = await context.LessonVideos
            .FirstOrDefaultAsync(video => video.Id == id, cancellationToken);

        if (lesson is null)
        {
            return NotFound();
        }

        context.LessonVideos.Remove(lesson);
        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Aula excluida com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminLessonVideoFormViewModel> BuildFormViewModelAsync(
        AdminLessonVideoFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Categories = await context.VideoCategories
            .AsNoTracking()
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .Select(category => new SelectListItem
            {
                Text = category.Name,
                Value = category.Id.ToString(),
                Selected = model.VideoCategoryId == category.Id
            })
            .ToArrayAsync(cancellationToken);

        return model;
    }

    private async Task<VideoCategory?> ValidateLessonFormAsync(
        AdminLessonVideoFormViewModel model,
        Guid? currentLessonId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return null;
        }

        if (!YouTubeVideoReferenceParser.TryExtractVideoId(model.YouTubeReference, out var videoId))
        {
            ModelState.AddModelError(nameof(model.YouTubeReference), "Informe um link ou ID valido do YouTube.");
            return null;
        }

        if (model.VideoCategoryId is null || model.VideoCategoryId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(model.VideoCategoryId), "Selecione a categoria da aula.");
            return null;
        }

        var category = await context.VideoCategories
            .FirstOrDefaultAsync(item => item.Id == model.VideoCategoryId.Value, cancellationToken);

        if (category is null)
        {
            ModelState.AddModelError(nameof(model.VideoCategoryId), "A categoria selecionada nao foi encontrada.");
            return null;
        }

        var normalizedVideoId = videoId.ToUpperInvariant();
        var duplicatedVideo = await context.LessonVideos
            .AsNoTracking()
            .AnyAsync(video =>
                video.Id != currentLessonId &&
                video.YouTubeVideoId.ToUpper() == normalizedVideoId,
                cancellationToken);

        if (duplicatedVideo)
        {
            ModelState.AddModelError(nameof(model.YouTubeReference), "Ja existe uma aula cadastrada com este video.");
            return null;
        }

        return category;
    }
}
