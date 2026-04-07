using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;

namespace GestionEquipeSportive.Services;

public class EcoleService : IEcoleService
{
    private readonly IExcelRepository _repo;

    public EcoleService(IExcelRepository repo)
    {
        _repo = repo;
    }

    public List<Ecole> GetAllEcoles() => _repo.GetAllEcoles();

    public Ecole? GetEcoleById(int id) => _repo.GetEcoleById(id);

    public Ecole CreateEcole(EcoleViewModel vm, IFormFile? logoFile, string webRootPath)
    {
        var ecole = new Ecole
        {
            Nom = vm.Nom,
            NomEquipe = vm.NomEquipe,
            CouleurPrimaire = vm.CouleurPrimaire,
            CouleurSecondaire = vm.CouleurSecondaire,
            LiensSociaux = vm.LiensSociaux.Where(l => !string.IsNullOrWhiteSpace(l.Url)).ToList()
        };

        if (logoFile != null && logoFile.Length > 0)
        {
            ecole.LogoPath = SaveFile(logoFile, Path.Combine(webRootPath, "uploads", "logos"));
        }

        return _repo.AddEcole(ecole);
    }

    public Ecole UpdateEcole(EcoleViewModel vm, IFormFile? logoFile, string webRootPath)
    {
        var ecole = _repo.GetEcoleById(vm.Id) ?? new Ecole();
        ecole.Id = vm.Id;
        ecole.Nom = vm.Nom;
        ecole.NomEquipe = vm.NomEquipe;
        ecole.CouleurPrimaire = vm.CouleurPrimaire;
        ecole.CouleurSecondaire = vm.CouleurSecondaire;
        ecole.LiensSociaux = vm.LiensSociaux.Where(l => !string.IsNullOrWhiteSpace(l.Url)).ToList();

        if (logoFile != null && logoFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(ecole.LogoPath))
            {
                var oldPath = Path.Combine(webRootPath, ecole.LogoPath.TrimStart('/'));
                if (File.Exists(oldPath)) File.Delete(oldPath);
            }
            ecole.LogoPath = SaveFile(logoFile, Path.Combine(webRootPath, "uploads", "logos"));
        }

        return _repo.UpdateEcole(ecole);
    }

    public bool DeleteEcole(int id) => _repo.DeleteEcole(id);

    public EcoleViewModel ToViewModel(Ecole ecole) => new EcoleViewModel
    {
        Id = ecole.Id,
        Nom = ecole.Nom,
        NomEquipe = ecole.NomEquipe,
        LogoPathActuel = ecole.LogoPath,
        CouleurPrimaire = ecole.CouleurPrimaire,
        CouleurSecondaire = ecole.CouleurSecondaire,
        LiensSociaux = ecole.LiensSociaux
    };

    // ── Thèmes ──────────────────────────────────────────────────────────────

    public List<ThemeEcole> GetThemesByEcole(int ecoleId) => _repo.GetThemesByEcole(ecoleId);

    public ThemeEcole? GetThemeById(int id) => _repo.GetThemeById(id);

    public ThemeEcole CreateTheme(ThemeEcoleViewModel vm, IFormFile? logoFile, string webRootPath)
    {
        var theme = new ThemeEcole
        {
            EcoleId = vm.EcoleId,
            NomEquipe = vm.NomEquipe.Trim(),
            CouleurPrimaire = vm.CouleurPrimaire,
            CouleurSecondaire = vm.CouleurSecondaire
        };

        if (logoFile != null && logoFile.Length > 0)
            theme.LogoPath = SaveFile(logoFile, Path.Combine(webRootPath, "uploads", "logos"));

        return _repo.AddTheme(theme);
    }

    public ThemeEcole UpdateTheme(ThemeEcoleViewModel vm, IFormFile? logoFile, string webRootPath)
    {
        var theme = _repo.GetThemeById(vm.Id) ?? new ThemeEcole();
        theme.Id = vm.Id;
        theme.EcoleId = vm.EcoleId;
        theme.NomEquipe = vm.NomEquipe.Trim();
        theme.CouleurPrimaire = vm.CouleurPrimaire;
        theme.CouleurSecondaire = vm.CouleurSecondaire;

        if (logoFile != null && logoFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(theme.LogoPath))
            {
                var oldPath = Path.Combine(webRootPath, theme.LogoPath.TrimStart('/'));
                if (File.Exists(oldPath)) File.Delete(oldPath);
            }
            theme.LogoPath = SaveFile(logoFile, Path.Combine(webRootPath, "uploads", "logos"));
        }

        return _repo.UpdateTheme(theme);
    }

    public bool DeleteTheme(int id, string webRootPath)
    {
        var theme = _repo.GetThemeById(id);
        if (theme != null && !string.IsNullOrEmpty(theme.LogoPath))
        {
            var path = Path.Combine(webRootPath, theme.LogoPath.TrimStart('/'));
            if (File.Exists(path)) File.Delete(path);
        }
        return _repo.DeleteTheme(id);
    }

    public ThemeEcoleViewModel ToThemeViewModel(ThemeEcole theme) => new ThemeEcoleViewModel
    {
        Id = theme.Id,
        EcoleId = theme.EcoleId,
        NomEquipe = theme.NomEquipe,
        CouleurPrimaire = theme.CouleurPrimaire,
        CouleurSecondaire = theme.CouleurSecondaire,
        LogoPathActuel = theme.LogoPath
    };

    private static string SaveFile(IFormFile file, string directory)
    {
        Directory.CreateDirectory(directory);
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(directory, fileName);
        using var stream = new FileStream(fullPath, FileMode.Create);
        file.CopyTo(stream);
        return $"/uploads/logos/{fileName}";
    }
}
