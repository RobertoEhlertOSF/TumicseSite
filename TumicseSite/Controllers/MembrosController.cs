using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TumicseSite.Data;
using TumicseSite.ViewModels;

namespace TumicseSite.Controllers;

[Authorize(Roles = $"{IdentityRoles.Admin},{IdentityRoles.Medium}")]
public class MembrosController : Controller
{
    public IActionResult Index()
    {
        var isAdmin = User.IsInRole(IdentityRoles.Admin);
        var model = new MembrosIndexViewModel
        {
            MemberName = User.Identity?.Name ?? "Membro",
            IsAdmin = isAdmin,
            PrimaryLinks =
            [
                new MembrosLinkCardViewModel
                {
                    Eyebrow = "Estudo",
                    Title = "Aulas internas",
                    Description = "Acesse o acervo de aulas para mediuns, organizado por topicos e categorias.",
                    Controller = "Aulas",
                    Action = "Index",
                    CtaLabel = "Abrir aulas"
                },
                new MembrosLinkCardViewModel
                {
                    Eyebrow = "Agenda",
                    Title = "Agenda interna",
                    Description = "Consulte compromissos internos, eventos publicos e atividades da casa em um unico lugar.",
                    Controller = "Agenda",
                    Action = "Interna",
                    CtaLabel = "Abrir agenda interna"
                }
            ],
            AdminLinks = isAdmin
                ?
                [
                    new MembrosLinkCardViewModel
                    {
                        Eyebrow = "Administracao",
                        Title = "Gerenciar eventos",
                        Description = "Cadastre, ajuste ou remova eventos publicos e internos da agenda do TUMICSE.",
                        Area = "Admin",
                        Controller = "Eventos",
                        Action = "Index",
                        CtaLabel = "Abrir eventos"
                    },
                    new MembrosLinkCardViewModel
                    {
                        Eyebrow = "Administracao",
                        Title = "Gerenciar aulas",
                        Description = "Atualize o catalogo de aulas internas e mantenha o material de estudo organizado.",
                        Area = "Admin",
                        Controller = "Aulas",
                        Action = "Index",
                        CtaLabel = "Abrir aulas admin"
                    }
                ]
                : []
        };

        return View(model);
    }
}
