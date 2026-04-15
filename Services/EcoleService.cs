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

        var ecoleCreee = _repo.AddEcole(ecole);

        // Initialiser l'année scolaire courante (1 juillet → 30 juin)
        var aujourd = DateTime.Today;
        var anneeCourante = aujourd.Month >= 7
            ? $"{aujourd.Year}-{aujourd.Year + 1}"
            : $"{aujourd.Year - 1}-{aujourd.Year}";
        _repo.AddAnneeScolaire(new Models.AnneeScolaireEcole { EcoleId = ecoleCreee.Id, AnneeScolaire = anneeCourante });

        return ecoleCreee;
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

    // ── Équipes adverses ────────────────────────────────────────────────────

    public List<EquipeAdverse> GetEquipesAdversesByEcole(int ecoleId) => _repo.GetEquipesAdversesByEcole(ecoleId);

    public List<EquipeAdverse> GetEquipesAdversesByEcoleSport(int ecoleId, string typeSport)
        => _repo.GetEquipesAdversesByEcoleSport(ecoleId, typeSport);

    public EquipeAdverse? GetEquipeAdverseById(int id) => _repo.GetEquipeAdverseById(id);

    public EquipeAdverse CreateEquipeAdverse(EquipeAdverseViewModel vm, IFormFile? logoFile, string webRootPath)
    {
        var equipe = new EquipeAdverse
        {
            EcoleId = vm.EcoleId,
            TypeSport = vm.TypeSport,
            Nom = vm.Nom.Trim(),
            Lieu = string.IsNullOrWhiteSpace(vm.Lieu) ? null : vm.Lieu.Trim()
        };
        if (logoFile != null && logoFile.Length > 0)
            equipe.LogoPath = SaveAdverseLogo(logoFile, vm.EcoleId, webRootPath);
        return _repo.AddEquipeAdverse(equipe);
    }

    public EquipeAdverse UpdateEquipeAdverse(EquipeAdverseViewModel vm, IFormFile? logoFile, string webRootPath)
    {
        var equipe = _repo.GetEquipeAdverseById(vm.Id) ?? new EquipeAdverse();
        equipe.Id = vm.Id;
        equipe.EcoleId = vm.EcoleId;
        equipe.TypeSport = vm.TypeSport;
        equipe.Nom = vm.Nom.Trim();
        equipe.Lieu = string.IsNullOrWhiteSpace(vm.Lieu) ? null : vm.Lieu.Trim();
        if (logoFile != null && logoFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(equipe.LogoPath))
            {
                var oldPath = Path.Combine(webRootPath, equipe.LogoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(oldPath)) File.Delete(oldPath);
            }
            equipe.LogoPath = SaveAdverseLogo(logoFile, vm.EcoleId, webRootPath);
        }
        return _repo.UpdateEquipeAdverse(equipe);
    }

    public bool DeleteEquipeAdverse(int id, string webRootPath)
    {
        var equipe = _repo.GetEquipeAdverseById(id);
        if (equipe?.LogoPath != null)
        {
            var path = Path.Combine(webRootPath, equipe.LogoPath.TrimStart('/'));
            if (File.Exists(path)) File.Delete(path);
        }
        return _repo.DeleteEquipeAdverse(id);
    }

    public EquipeAdverseViewModel ToEquipeAdverseViewModel(EquipeAdverse equipe) => new EquipeAdverseViewModel
    {
        Id = equipe.Id,
        EcoleId = equipe.EcoleId,
        TypeSport = equipe.TypeSport,
        Nom = equipe.Nom,
        Lieu = equipe.Lieu,
        LogoPathActuel = equipe.LogoPath
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

        var musiqueDir = Path.Combine(webRootPath, "uploads", "musique");
        if (vm.MusiqueProchainMatchFile != null && vm.MusiqueProchainMatchFile.Length > 0)
            theme.MusiqueProchainMatchPath = SaveFile(vm.MusiqueProchainMatchFile, musiqueDir);
        theme.MusiqueProchainMatchDebut = vm.MusiqueProchainMatchDebut;
        theme.MusiqueProchainMatchDuree = vm.MusiqueProchainMatchDuree;

        if (vm.MusiqueVictoireFile != null && vm.MusiqueVictoireFile.Length > 0)
            theme.MusiqueVictoirePath = SaveFile(vm.MusiqueVictoireFile, musiqueDir);
        theme.MusiqueVictoireDebut = vm.MusiqueVictoireDebut;
        theme.MusiqueVictoireDuree = vm.MusiqueVictoireDuree;

        if (vm.MusiqueDefaiteFile != null && vm.MusiqueDefaiteFile.Length > 0)
            theme.MusiqueDefaitePath = SaveFile(vm.MusiqueDefaiteFile, musiqueDir);
        theme.MusiqueDefaiteDebut = vm.MusiqueDefaiteDebut;
        theme.MusiqueDefaiteDuree = vm.MusiqueDefaiteDuree;

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

        var musiqueDir = Path.Combine(webRootPath, "uploads", "musique");
        if (vm.MusiqueProchainMatchFile != null && vm.MusiqueProchainMatchFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(theme.MusiqueProchainMatchPath))
            { var op = Path.Combine(webRootPath, theme.MusiqueProchainMatchPath.TrimStart('/')); if (File.Exists(op)) File.Delete(op); }
            theme.MusiqueProchainMatchPath = SaveFile(vm.MusiqueProchainMatchFile, musiqueDir);
        }
        if (vm.MusiqueVictoireFile != null && vm.MusiqueVictoireFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(theme.MusiqueVictoirePath))
            { var op = Path.Combine(webRootPath, theme.MusiqueVictoirePath.TrimStart('/')); if (File.Exists(op)) File.Delete(op); }
            theme.MusiqueVictoirePath = SaveFile(vm.MusiqueVictoireFile, musiqueDir);
        }
        if (vm.MusiqueDefaiteFile != null && vm.MusiqueDefaiteFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(theme.MusiqueDefaitePath))
            { var op = Path.Combine(webRootPath, theme.MusiqueDefaitePath.TrimStart('/')); if (File.Exists(op)) File.Delete(op); }
            theme.MusiqueDefaitePath = SaveFile(vm.MusiqueDefaiteFile, musiqueDir);
        }
        theme.MusiqueProchainMatchDebut = vm.MusiqueProchainMatchDebut;
        theme.MusiqueProchainMatchDuree = vm.MusiqueProchainMatchDuree;
        theme.MusiqueVictoireDebut      = vm.MusiqueVictoireDebut;
        theme.MusiqueVictoireDuree      = vm.MusiqueVictoireDuree;
        theme.MusiqueDefaiteDebut       = vm.MusiqueDefaiteDebut;
        theme.MusiqueDefaiteDuree       = vm.MusiqueDefaiteDuree;

        return _repo.UpdateTheme(theme);
    }

    public bool DeleteTheme(int id, string webRootPath)
    {
        var theme = _repo.GetThemeById(id);
        if (theme != null)
        {
            foreach (var p in new[] { theme.LogoPath, theme.MusiqueProchainMatchPath, theme.MusiqueVictoirePath, theme.MusiqueDefaitePath })
                if (!string.IsNullOrEmpty(p)) { var fp = Path.Combine(webRootPath, p.TrimStart('/')); if (File.Exists(fp)) File.Delete(fp); }
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
        LogoPathActuel = theme.LogoPath,
        MusiqueProchainMatchPathActuel = theme.MusiqueProchainMatchPath,
        MusiqueProchainMatchDebut = theme.MusiqueProchainMatchDebut,
        MusiqueProchainMatchDuree = theme.MusiqueProchainMatchDuree,
        MusiqueVictoirePathActuel = theme.MusiqueVictoirePath,
        MusiqueVictoireDebut = theme.MusiqueVictoireDebut,
        MusiqueVictoireDuree = theme.MusiqueVictoireDuree,
        MusiqueDefaitePathActuel = theme.MusiqueDefaitePath,
        MusiqueDefaiteDebut = theme.MusiqueDefaiteDebut,
        MusiqueDefaiteDuree = theme.MusiqueDefaiteDuree
    };

    // ── Années scolaires ────────────────────────────────────────────────────

    public List<AnneeScolaireEcole> GetAnneesScolairesByEcole(int ecoleId)
        => _repo.GetAnneesScolairesByEcole(ecoleId);

    public AnneeScolaireEcole AddAnneeScolaire(int ecoleId, string annee)
        => _repo.AddAnneeScolaire(new AnneeScolaireEcole { EcoleId = ecoleId, AnneeScolaire = annee.Trim() });

    public bool DeleteAnneeScolaire(int id)
        => _repo.DeleteAnneeScolaire(id);

    private string SaveAdverseLogo(IFormFile file, int ecoleId, string webRootPath)
    {
        var ecole = _repo.GetEcoleById(ecoleId);
        var ecoleSlug = ecole != null ? Models.Ecole.ToSlug(ecole.Nom) : ecoleId.ToString();
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var directory = Path.Combine(webRootPath, "Image", ecoleSlug, "adverses", "logos");
        Directory.CreateDirectory(directory);
        var fullPath = Path.Combine(directory, fileName);
        using var stream = new FileStream(fullPath, FileMode.Create);
        file.CopyTo(stream);
        return $"/Image/{ecoleSlug}/adverses/logos/{fileName}";
    }

    private static string SaveFile(IFormFile file, string directory)
    {
        Directory.CreateDirectory(directory);
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(directory, fileName);
        using var stream = new FileStream(fullPath, FileMode.Create);
        file.CopyTo(stream);
        var folderName = Path.GetFileName(directory);
        return $"/uploads/{folderName}/{fileName}";
    }
}
