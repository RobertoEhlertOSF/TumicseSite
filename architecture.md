# Arquitetura

O projeto será um monólito ASP.NET Core MVC com Razor Views, Identity, EF Core e SQL Server.

## Camadas

- Controllers: recebem requisições HTTP e orquestram serviços
- Services: regras de negócio simples
- Data: ApplicationDbContext, migrations e seed
- Models/Entities: entidades persistidas
- ViewModels: modelos específicos para telas
- Views: Razor Views
- Areas/Admin: área administrativa protegida

## Autorização

Roles:

- Admin
- Medium

Áreas:

- Público: landing page e eventos publicados
- Medium/Admin: aulas
- Admin: painel administrativo

## Vídeos

As aulas devem armazenar apenas o YouTubeVideoId.

Nunca renderizar vídeo a partir de URL recebida por query string.

O player deve montar o embed usando youtube-nocookie.com dentro de uma partial view.