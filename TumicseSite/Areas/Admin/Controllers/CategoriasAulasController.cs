using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Data;
using TumicseSite.Models;
using TumicseSite.ViewModels;

namespace TumicseSite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = IdentityRoles.Admin)]
public class CategoriasAulasController(ApplicationDbContext context) : Controller
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var categories = await context.VideoCategories
            .AsNoTracking()
            .Include(category => category.LessonVideos)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);

        var model = new AdminCategoriasIndexViewModel
        {
            Categories = categories
                .Select(category => new AdminVideoCategoryListItemViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    DisplayOrder = category.DisplayOrder,
                    LessonCount = category.LessonVideos.Count,
                    CreatedAtLabel = category.CreatedAt.ToString("dd/MM/yyyy", PtBrCulture)
                })
                .ToArray()
        };

        return View(model);
    }

    public IActionResult Create()
    {
        return View(new AdminVideoCategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminVideoCategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (!await ValidateCategoryFormAsync(model, null, cancellationToken))
        {
            return View(model);
        }

        context.VideoCategories.Add(new VideoCategory
        {
            Name = model.Name.Trim(),
            DisplayOrder = model.DisplayOrder
        });

        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Categoria cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var category = await context.VideoCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        return View(new AdminVideoCategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            DisplayOrder = category.DisplayOrder
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AdminVideoCategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var category = await context.VideoCategories
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        if (!await ValidateCategoryFormAsync(model, id, cancellationToken))
        {
            return View(model);
        }

        category.Name = model.Name.Trim();
        category.DisplayOrder = model.DisplayOrder;

        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Categoria atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var category = await context.VideoCategories
            .AsNoTracking()
            .Select(item => new AdminVideoCategoryDeleteViewModel
            {
                Id = item.Id,
                Name = item.Name,
                DisplayOrder = item.DisplayOrder,
                LessonCount = item.LessonVideos.Count
            })
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var category = await context.VideoCategories
            .Include(item => item.LessonVideos)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        if (category.LessonVideos.Count > 0)
        {
            ModelState.AddModelError(string.Empty, "Nao e possivel excluir uma categoria que possui aulas vinculadas.");
            return View(new AdminVideoCategoryDeleteViewModel
            {
                Id = category.Id,
                Name = category.Name,
                DisplayOrder = category.DisplayOrder,
                LessonCount = category.LessonVideos.Count
            });
        }

        context.VideoCategories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Categoria excluida com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ValidateCategoryFormAsync(
        AdminVideoCategoryFormViewModel model,
        Guid? currentCategoryId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return false;
        }

        var normalizedName = model.Name.Trim().ToUpperInvariant();
        var duplicatedCategory = await context.VideoCategories
            .AsNoTracking()
            .AnyAsync(category =>
                category.Id != currentCategoryId &&
                category.Name.ToUpper() == normalizedName,
                cancellationToken);

        if (duplicatedCategory)
        {
            ModelState.AddModelError(nameof(model.Name), "Ja existe uma categoria cadastrada com este nome.");
            return false;
        }

        return true;
    }
}
