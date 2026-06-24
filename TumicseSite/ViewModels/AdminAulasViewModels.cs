using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TumicseSite.ViewModels;

public sealed class AdminAulasIndexViewModel
{
    public IReadOnlyList<AdminLessonVideoListItemViewModel> Lessons { get; init; } = [];
}

public sealed class AdminLessonVideoListItemViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string YouTubeVideoId { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public string CreatedAtLabel { get; init; } = string.Empty;
}

public sealed class AdminLessonVideoFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Informe o titulo da aula.")]
    [StringLength(200)]
    [Display(Name = "Titulo")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione a categoria da aula.")]
    [Display(Name = "Categoria")]
    public Guid? VideoCategoryId { get; set; }

    [Required(ErrorMessage = "Informe a URL ou o ID do video do YouTube.")]
    [Display(Name = "URL ou ID do YouTube")]
    public string YouTubeReference { get; set; } = string.Empty;

    [Range(0, 9999, ErrorMessage = "Informe uma ordem de exibicao valida.")]
    [Display(Name = "Ordem de exibicao")]
    public int DisplayOrder { get; set; }

    public IReadOnlyList<SelectListItem> Categories { get; set; } = [];
}

public sealed class AdminLessonVideoDeleteViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string YouTubeVideoId { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }
}

public sealed class AdminCategoriasIndexViewModel
{
    public IReadOnlyList<AdminVideoCategoryListItemViewModel> Categories { get; init; } = [];
}

public sealed class AdminVideoCategoryListItemViewModel
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public int LessonCount { get; init; }

    public string CreatedAtLabel { get; init; } = string.Empty;
}

public sealed class AdminVideoCategoryFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Informe o nome da categoria.")]
    [StringLength(120)]
    [Display(Name = "Nome da categoria")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 9999, ErrorMessage = "Informe uma ordem de exibicao valida.")]
    [Display(Name = "Ordem de exibicao")]
    public int DisplayOrder { get; set; }
}

public sealed class AdminVideoCategoryDeleteViewModel
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public int LessonCount { get; init; }
}
