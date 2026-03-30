using GestionEquipeSportive.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ─── Authentification par cookies ────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccesDenie";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Name = "GES.Auth";
    });

// ─── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IExcelRepository, ExcelRepository>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddScoped<IEcoleAccessService, EcoleAccessService>();
builder.Services.AddScoped<IEcoleService, EcoleService>();
builder.Services.AddScoped<IEquipeService, EquipeService>();
builder.Services.AddScoped<IJoueurService, JoueurService>();
builder.Services.AddScoped<IGalerieService, GalerieService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IEvenementService, EvenementService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Créer les dossiers nécessaires au démarrage
var uploadPaths = new[]
{
    Path.Combine(app.Environment.WebRootPath, "uploads", "logos"),
    Path.Combine(app.Environment.WebRootPath, "uploads", "photos"),
    Path.Combine(app.Environment.WebRootPath, "uploads", "galerie"),
    Path.Combine(app.Environment.WebRootPath, "uploads", "matchs", "photos"),
    Path.Combine(app.Environment.WebRootPath, "uploads", "matchs", "videos"),
    Path.Combine(app.Environment.WebRootPath, "uploads", "staff")
};

foreach (var path in uploadPaths)
    Directory.CreateDirectory(path);

Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "Data"));

// Initialiser UserService au démarrage (affiche le mot de passe admin si premier démarrage)
app.Services.GetRequiredService<IUserService>();

app.MapControllerRoute(
    name: "public-equipe",
    pattern: "p/{ecoleSlug}/{annee}/{sport}/{niveau}",
    defaults: new { controller = "Public", action = "Equipe" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
