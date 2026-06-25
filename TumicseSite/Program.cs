using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using TumicseSite.Data;
using TumicseSite.Identity;
using TumicseSite.Models;
using TumicseSite.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("Render"))
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port) &&
    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    DatabaseConfiguration.Configure(options, builder.Configuration));
if (builder.Environment.IsEnvironment("Render"))
{
    var keysPath = builder.Configuration["DataProtection:KeysPath"] ??
                   Path.Combine(Path.GetTempPath(), "tumicse-keys");

    Directory.CreateDirectory(keysPath);
    builder.Services.AddDataProtection()
        .SetApplicationName("TumicseSite.Render")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    if (builder.Environment.IsEnvironment("Render"))
    {
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    }
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<ActiveUserSignInManager>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IEventExportService, EventExportService>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();

var app = builder.Build();

app.UseForwardedHeaders();

await using (var scope = app.Services.CreateAsyncScope())
{
    await ApplicationDbInitializer.InitializeAsync(scope.ServiceProvider, builder.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    if (!app.Environment.IsEnvironment("Render"))
    {
        app.UseHsts();
    }
}

if (!app.Environment.IsEnvironment("Render"))
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/healthz", () => Results.Ok("OK"));

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "calendar",
    pattern: "Calendar/{action=Index}/{id?}",
    defaults: new { controller = "Agenda" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
