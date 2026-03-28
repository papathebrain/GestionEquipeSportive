using GestionEquipeSportive.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Enregistrement des services
builder.Services.AddSingleton<IExcelRepository, ExcelRepository>();
builder.Services.AddScoped<IEcoleService, EcoleService>();
builder.Services.AddScoped<IEquipeService, EquipeService>();
builder.Services.AddScoped<IJoueurService, JoueurService>();
builder.Services.AddScoped<IGalerieService, GalerieService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Créer les dossiers nécessaires au démarrage
var uploadPaths = new[]
{
    Path.Combine(app.Environment.WebRootPath, "uploads", "logos"),
    Path.Combine(app.Environment.WebRootPath, "uploads", "photos"),
    Path.Combine(app.Environment.WebRootPath, "uploads", "galerie")
};

foreach (var path in uploadPaths)
{
    Directory.CreateDirectory(path);
}

var dataPath = Path.Combine(app.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dataPath);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
