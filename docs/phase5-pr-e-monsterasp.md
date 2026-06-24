# Fase 5 - Calendario, Eventos e Publicacao no MonsterASP

## Funcionalidades entregues

- Entidade de eventos evoluida para o modelo de calendario com `StartDate`, `EndDate`, `IsAllDay`, `EventType`, `IsPublic`, `IsActive`, `CreatedAt` e `UpdatedAt`.
- Tipagem forte de eventos com `CalendarEventType`.
- Tratamento de aniversarios como evento comum via `EventType = Birthday` (opcao A).
- Seed institucional de 2026 mapeado a partir do calendario base do TUMICSE.
- Query helpers para filtro por tipo, periodo e somente eventos futuros.
- Pagina publica de agenda com agrupamento por mes e exibicao apenas de eventos publicos, ativos e futuros.
- Pagina publica de detalhe de evento com bloqueio para eventos privados, inativos ou cancelados.
- Pagina interna de agenda para `Admin` e `Medium`.
- CRUD administrativo de eventos para `Admin`.
- Filtros administrativos por tipo, periodo, visibilidade e estado.
- Acao para ativar e desativar eventos.
- Exportacao publica em `.ics`.
- Exportacao administrativa em `.ics`.
- Exportacao publica em PDF com layout visual de calendario anual.
- Exportacao administrativa em PDF com o mesmo layout visual, incluindo eventos internos quando o filtro permitir.
- Views atualizadas para destacar PDF e manter `.ics` disponivel.
- Migration `Phase5CalendarEvents`.

## Titulo sugerido do PR

`Fase 5 - calendario, gestao de eventos e exportacao institucional`

## Branch sugerida

A branch atual esta como `codex/phase-4-agenda-eventos`.  
Para publicar com nome coerente, a sugestao e usar algo como:

`feature/phase-5-calendario-eventos`

## Corpo sugerido do PR

```md
## Resumo

Implementa a Fase 5 do TUMICSE com calendario institucional, gestao administrativa de eventos, seed inicial de 2026 e exportacao institucional em `.ics` e PDF.

## O que entrou

- novo modelo de calendario com `StartDate`, `EndDate`, `IsAllDay` e `CalendarEventType`
- aniversarios tratados como eventos do proprio calendario
- migration `Phase5CalendarEvents`
- seed inicial com calendario anual de 2026
- agenda publica agrupada por mes
- detalhe publico de evento
- agenda interna para `Admin` e `Medium`
- CRUD administrativo de eventos para `Admin`
- filtros por tipo, periodo, visibilidade e estado
- exportacao publica e administrativa em `.ics`
- exportacao publica e administrativa em PDF no formato de calendario visual anual

## Arquivos-chave

- `TumicseSite/Models/Event.cs`
- `TumicseSite/Models/CalendarEventType.cs`
- `TumicseSite/Models/EventTypeCatalog.cs`
- `TumicseSite/Data/ApplicationDbContext.cs`
- `TumicseSite/Data/ApplicationDbInitializer.cs`
- `TumicseSite/Data/EventQueryExtensions.cs`
- `TumicseSite/Controllers/AgendaController.cs`
- `TumicseSite/Areas/Admin/Controllers/EventosController.cs`
- `TumicseSite/Services/IEventExportService.cs`
- `TumicseSite/Services/EventExportService.cs`
- `TumicseSite/Data/Migrations/20260624160953_Phase5CalendarEvents.cs`

## Como testar

1. Rodar `dotnet build TumicseSite\\TumicseSite.csproj -p:UseAppHost=false`
2. Aplicar a migration em uma base valida
3. Verificar agenda publica em `/Calendar`
4. Verificar detalhe publico clicando em um evento
5. Verificar exportacao publica:
   - `/Calendar/ExportIcs`
   - `/Calendar/ExportPdf`
6. Logar como `Admin` e validar:
   - `/Admin/Eventos`
   - filtros
   - criar, editar, ativar, desativar e excluir
   - `/Admin/Eventos/ExportIcs`
   - `/Admin/Eventos/ExportPdf`

## Checklist

- [x] calendario publico
- [x] detalhe publico
- [x] agenda interna
- [x] CRUD administrativo
- [x] filtros administrativos
- [x] seed inicial de 2026
- [x] exportacao `.ics`
- [x] exportacao PDF visual
- [x] migration criada
- [x] build executado
```

## Tutorial de publicacao no MonsterASP

### 1. Confirmar compatibilidade

No momento desta preparacao, a homepage do MonsterASP informa que a plataforma esta `".NET 10 ready"` e suporta `.NET Core 10/9/8`, alem de deploy direto pelo Visual Studio.

Como o projeto esta em `net10.0`, isso esta alinhado com o host.

### 2. Criar o website no painel

1. Criar a conta no MonsterASP.
2. Entrar no painel administrativo.
3. Criar um novo website.
4. Escolher o datacenter adequado.

Observacao:

- plano Free usa subdominio do MonsterASP
- dominio proprio exige plano Premium

### 3. Ativar WebDeploy e baixar o publish profile

1. No painel do website, ativar o WebDeploy.
2. Baixar o arquivo `.publishSettings`.
3. Guardar esse arquivo com cuidado, porque ele contem credenciais de publicacao.

### 4. Criar a base MSSQL

1. No painel, abrir `Databases`.
2. Criar uma base `MSSQL`.
3. Copiar a connection string fornecida pelo painel.

Observacao importante para este projeto:

- a aplicacao sobe lendo `ConnectionStrings:DefaultConnection`
- na inicializacao, `ApplicationDbInitializer.InitializeAsync(...)` executa `Database.MigrateAsync()`
- portanto, a base precisa existir e a connection string precisa estar correta ja no primeiro boot

### 5. Preparar a configuracao de producao

Este projeto hoje vem com `appsettings.json` apontando para `localdb`.
Antes de publicar em producao, voce precisa fornecer uma `DefaultConnection` valida para o MonsterASP.

Opcao recomendada:

1. Criar um `appsettings.Production.json` localmente.
2. Nao versionar segredos no Git.
3. Publicar esse arquivo junto com o site.

Exemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "USE_A_CONNECTION_STRING_EXATA_DO_MONSTERASP_AQUI"
  },
  "SeedAdmin": {
    "Email": "admin@seu-dominio.com",
    "Password": "SENHA_FORTE_AQUI",
    "DisplayName": "Administrador"
  }
}
```

Notas:

- use a string exata do painel do MonsterASP em vez de montar a mao
- se quiser que o admin inicial seja criado automaticamente, preencha `SeedAdmin`
- se deixar `SeedAdmin` vazio, nenhum usuario admin sera criado pelo seed

Se preferir, voce tambem pode publicar primeiro e depois editar `appsettings.json` ou subir `appsettings.Production.json` via WebFTP antes de colocar o site em uso.

### 6. Publicar pelo Visual Studio

1. Abrir a solution no Visual Studio.
2. Clicar com o botao direito no projeto web.
3. Escolher `Publish`.
4. Importar o `.publishSettings` baixado do MonsterASP.
5. Em `Show all settings`, conferir o perfil e a connection string se voce estiver usando o fluxo de publish com MSSQL do Visual Studio.
6. Publicar.

### 7. Validar o primeiro boot

Assim que o publish terminar:

1. Abrir a URL do website.
2. Confirmar que a aplicacao sobe sem erro.
3. Confirmar que as migrations foram aplicadas.
4. Confirmar que o seed rodou.

Checklist de smoke test:

- home publica
- `/Calendar`
- `/Calendar/ExportIcs`
- `/Calendar/ExportPdf`
- login
- `/Admin/Eventos`
- `/Admin/Eventos/ExportPdf`

### 8. Ativar HTTPS

No painel de `Domains/HTTPS`:

1. Ativar o certificado Let's Encrypt.
2. Ativar o redirect HTTP -> HTTPS depois de confirmar que o site subiu corretamente.

Observacoes oficiais do host:

- no plano Free, a renovacao do Let's Encrypt e manual a cada 90 dias
- em planos pagos, a renovacao e automatica

### 9. Dominio proprio

Se for usar dominio proprio:

1. garantir plano Premium
2. apontar o DNS conforme o painel do MonsterASP
3. adicionar o dominio no website
4. ativar HTTPS para o dominio raiz e, se aplicavel, para `www`

### 10. Troubleshooting

Se o site nao subir ou retornar erro:

1. verificar `Logs` no Control Panel
2. verificar `Resources` para CPU, RAM, DISK e requests
3. se necessario, ativar `ASP.NET Core debug logging`
4. abrir `Files` -> `WebFTP`
5. inspecionar `./wwwroot/logs/stdout*`

### 11. Fluxo minimo recomendado para esta aplicacao

1. `dotnet build TumicseSite\\TumicseSite.csproj -p:UseAppHost=false`
2. criar website no MonsterASP
3. ativar WebDeploy
4. criar MSSQL
5. preparar `appsettings.Production.json` com `DefaultConnection`
6. opcionalmente preencher `SeedAdmin`
7. publicar pelo Visual Studio usando `.publishSettings`
8. validar home, agenda e exportacoes
9. ativar HTTPS

## Fontes oficiais consultadas

- MonsterASP homepage: https://www.monsterasp.net/
- Deploy .NET Core via Visual Studio: https://help.monsterasp.net/books/deploy/page/how-to-deploy-net-core-web-application-using-visual-studio
- Deploy .NET Core com MSSQL via Visual Studio: https://help.monsterasp.net/books/deploy/page/how-to-deploy-net-core-web-application-with-mssql-using-visual-studio
- Create database: https://help.monsterasp.net/books/databases/page/create-database
- Remote Access for database: https://help.monsterasp.net/books/databases/page/remote-access-for-database
- SQL Server Management Studio: https://help.monsterasp.net/books/databases/page/sql-server-management-studio-ssms
- HTTPS / Let's Encrypt: https://help.monsterasp.net/books/https/page/how-to-activate-https-with-lets-encrypt-certificate
- ASP.NET Core debug logging: https://help.monsterasp.net/books/debugging/page/aspnet-core-debug-logging
- Logs and Resources: https://help.monsterasp.net/books/debugging/page/logs-and-resources
