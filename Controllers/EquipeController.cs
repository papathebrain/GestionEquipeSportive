using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class EquipeController : Controller
{
    private readonly IEquipeService _equipeService;
    private readonly IEcoleService _ecoleService;
    private readonly IEcoleAccessService _access;
    private readonly IJoueurService _joueurService;
    private readonly IStaffService _staffService;
    private readonly IMatchService _matchService;
    private readonly IEvenementService _evenementService;
    private readonly IDictionnaireService _dictionnaireService;
    private readonly IWebHostEnvironment _env;

    public EquipeController(IEquipeService equipeService, IEcoleService ecoleService,
        IEcoleAccessService access, IJoueurService joueurService,
        IStaffService staffService, IMatchService matchService,
        IEvenementService evenementService, IDictionnaireService dictionnaireService,
        IWebHostEnvironment env)
    {
        _equipeService = equipeService;
        _ecoleService = ecoleService;
        _access = access;
        _joueurService = joueurService;
        _staffService = staffService;
        _matchService = matchService;
        _evenementService = evenementService;
        _dictionnaireService = dictionnaireService;
        _env = env;
    }

    public IActionResult Index(int ecoleId)
    {
        var ecole = _ecoleService.GetEcoleById(ecoleId);
        if (ecole == null) return NotFound();

        SetTheme(ecole);
        ViewBag.Ecole = ecole;
        ViewBag.PeutModifier = _access.PeutModifier(User, ecoleId);
        var equipes = _equipeService.GetEquipesByEcole(ecoleId);
        equipes = _access.FiltrerEquipes(User, equipes, ecoleId).ToList();
        ViewBag.NbJoueurs = equipes.ToDictionary(
            e => e.Id,
            e => _joueurService.GetJoueurEquipesByEquipe(e.Id).Count);
        ViewBag.StatsEquipes = equipes.ToDictionary(
            e => e.Id,
            e => _matchService.GetStatistiques(e.Id));
        var aujourd = DateTime.Today;
        ViewBag.AnneeCourante = aujourd.Month >= 7
            ? $"{aujourd.Year}-{aujourd.Year + 1}"
            : $"{aujourd.Year - 1}-{aujourd.Year}";
        return View(equipes);
    }

    public IActionResult Details(int id, string? tab = null)
    {
        var equipe = _equipeService.GetEquipeById(id);
        if (equipe == null) return NotFound();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        ThemeEcole? theme = equipe.ThemeId.HasValue ? _ecoleService.GetThemeById(equipe.ThemeId.Value) : null;
        if (ecole != null) SetTheme(ecole, theme);

        equipe.Ecole = ecole;
        equipe.Theme = theme;

        var joueurs = _joueurService.GetJoueurEquipesByEquipe(id);
        var staff = _staffService.GetStaffByEquipe(id);
        var matchs = _matchService.GetMatchsByEquipe(id);
        var stats = _matchService.GetStatistiques(id);

        var evenements = _evenementService.GetEvenementsByEquipe(id);

        var advLogos = matchs
            .Where(m => m.AdversaireId.HasValue)
            .Select(m => m.AdversaireId!.Value)
            .Distinct()
            .Select(aid => _ecoleService.GetEquipeAdverseById(aid))
            .Where(a => a != null && !string.IsNullOrEmpty(a.LogoPath))
            .ToDictionary(a => a!.Id, a => a!.LogoPath!);

        // Bandeau : logo du thème
        ViewBag.ImageEquipe = theme?.LogoPath;

        // Photo par joueur : photo d'assignation → JoueurMedia la plus récente
        var photoParJoueur = joueurs.ToDictionary(
            j => j.JoueurId,
            j => string.IsNullOrEmpty(j.PhotoPath)
                ? _joueurService.GetMediasByJoueur(j.JoueurId)
                    .OrderByDescending(m => m.DateAjout)
                    .FirstOrDefault()?.CheminFichier
                : j.PhotoPath);
        ViewBag.PhotoParJoueur = photoParJoueur;
        ViewBag.PeutModifier = _access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId);
        ViewBag.NbJoueurs = joueurs.Count;
        ViewBag.Joueurs = joueurs; // List<JoueurEquipe>
        ViewBag.Staff = staff;
        ViewBag.Matchs = matchs;
        ViewBag.Stats = stats;
        ViewBag.Evenements = evenements;
        ViewBag.AdvLogos = advLogos;
        ViewBag.ActiveTab = tab ?? "matchs";
        ViewBag.EquipesEcole = _equipeService.GetEquipesByEcole(equipe.EcoleId)
            .Where(e => e.Id != id)
            .OrderBy(e => e.TypeSport.ToString())
            .ThenBy(e => e.Niveau.ToString())
            .ThenBy(e => e.AnneeScolaire)
            .ToList();
        return View(equipe);
    }

    public IActionResult Create(int ecoleId)
    {
        if (!_access.PeutModifier(User, ecoleId))
            return Forbid();

        var ecole = _ecoleService.GetEcoleById(ecoleId);
        if (ecole == null) return NotFound();

        SetTheme(ecole);
        var vm = BuildViewModel(ecoleId);
        vm.NomEcole = ecole.Nom;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(EquipeViewModel vm)
    {
        if (!_access.PeutModifier(User, vm.EcoleId))
            return Forbid();

        var nomEffectif = !string.IsNullOrWhiteSpace(vm.Nom) ? vm.Nom.Trim() : null;
        if (nomEffectif != null && _equipeService.NomDejaUtilise(vm.EcoleId, nomEffectif, vm.AnneeScolaire))
            ModelState.AddModelError("Nom", "Une équipe avec ce nom existe déjà pour cette année scolaire.");

        if (!ModelState.IsValid)
        {
            var ecole = _ecoleService.GetEcoleById(vm.EcoleId);
            if (ecole != null) SetTheme(ecole);
            RebuildLists(vm);
            return View(vm);
        }

        _equipeService.CreateEquipe(vm);
        TempData["Success"] = "Équipe créée avec succès.";
        return RedirectToAction(nameof(Index), new { ecoleId = vm.EcoleId });
    }

    public IActionResult Edit(int id)
    {
        var equipe = _equipeService.GetEquipeById(id);
        if (equipe == null) return NotFound();

        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId))
            return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        var vm = _equipeService.ToViewModel(equipe);
        vm.NomEcole = ecole?.Nom;
        RebuildLists(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, EquipeViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!_access.PeutModifierEquipe(User, vm.Id, vm.EcoleId))
            return Forbid();

        var nomEff = !string.IsNullOrWhiteSpace(vm.Nom) ? vm.Nom.Trim() : null;
        if (nomEff != null && _equipeService.NomDejaUtilise(vm.EcoleId, nomEff, vm.AnneeScolaire, vm.Id))
            ModelState.AddModelError("Nom", "Une équipe avec ce nom existe déjà pour cette année scolaire.");

        if (!ModelState.IsValid)
        {
            var ecole2 = _ecoleService.GetEcoleById(vm.EcoleId);
            if (ecole2 != null) SetTheme(ecole2);
            RebuildLists(vm);
            return View(vm);
        }

        _equipeService.UpdateEquipe(vm);
        TempData["Success"] = "Équipe modifiée avec succès.";
        return RedirectToAction(nameof(Index), new { ecoleId = vm.EcoleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id, int ecoleId)
    {
        if (!_access.PeutModifierEquipe(User, id, ecoleId))
            return Forbid();

        _equipeService.DeleteEquipe(id);
        TempData["Success"] = "Équipe supprimée avec succès.";
        return RedirectToAction(nameof(Index), new { ecoleId });
    }

    public IActionResult Copier(int id)
    {
        var equipe = _equipeService.GetEquipeById(id);
        if (equipe == null) return NotFound();

        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId))
            return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        var vm = _equipeService.ToViewModel(equipe);
        vm.Id = 0;
        vm.AnneeScolaire = SuggestNextYear(equipe.AnneeScolaire);
        vm.NomEcole = ecole?.Nom;
        RebuildLists(vm);

        ViewBag.SourceEquipeId = id;
        ViewBag.SourceEquipeNom = equipe.Nom;
        ViewBag.Joueurs = _joueurService.GetJoueurEquipesByEquipe(id);
        ViewBag.Staff = _staffService.GetStaffByEquipe(id);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Copier(int id, EquipeViewModel vm, int[] joueursIds, int[] staffIds)
    {
        var sourceEquipe = _equipeService.GetEquipeById(id);
        if (sourceEquipe == null) return NotFound();

        if (!_access.PeutModifierEquipe(User, sourceEquipe.Id, sourceEquipe.EcoleId))
            return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = _ecoleService.GetEcoleById(vm.EcoleId);
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEcole = ecole2?.Nom;
            RebuildLists(vm);
            ViewBag.SourceEquipeId = id;
            ViewBag.SourceEquipeNom = sourceEquipe.Nom;
            ViewBag.Joueurs = _joueurService.GetJoueurEquipesByEquipe(id);
            ViewBag.Staff = _staffService.GetStaffByEquipe(id);
            return View(vm);
        }

        var nouvelleEquipe = _equipeService.CreateEquipe(vm);

        // Copier les assignations: assigner les mêmes joueurs à la nouvelle équipe
        if (joueursIds.Length > 0)
        {
            var dejaAssignes = new HashSet<int>();
            foreach (var jeId in joueursIds)
            {
                var sourceJe = _joueurService.GetJoueurEquipeById(jeId);
                if (sourceJe == null || dejaAssignes.Contains(sourceJe.JoueurId)) continue;
                dejaAssignes.Add(sourceJe.JoueurId);
                _joueurService.AssignerAEquipe(new GestionEquipeSportive.ViewModels.JoueurEquipeViewModel
                {
                    JoueurId = sourceJe.JoueurId,
                    EquipeId = nouvelleEquipe.Id,
                    PositionPrincipale = sourceJe.Position,
                    PositionPairsRaw = sourceJe.PositionSpecifique ?? "",
                    Numero = sourceJe.Numero,
                    Description = sourceJe.Description,
                    Actif = sourceJe.Actif
                }, null, _env.WebRootPath);
            }
        }

        if (staffIds.Length > 0)
            _staffService.CopierVersEquipe(staffIds, nouvelleEquipe.Id);

        TempData["Success"] = $"Équipe créée depuis \"{sourceEquipe.Nom}\" avec {joueursIds.Length} joueur(s) et {staffIds.Length} membre(s) de l'équipe école transféré(s).";
        return RedirectToAction(nameof(Details), new { id = nouvelleEquipe.Id });
    }

    [HttpGet]
    public JsonResult GetNiveaux(string sport)
    {
        var niveaux = _dictionnaireService.GetNiveaux(sport);
        return Json(niveaux.Select(n => new { value = ToNiveauEnumName(n), text = GetNiveauDisplayName(n) }));
    }

    private EquipeViewModel BuildViewModel(int ecoleId)
    {
        var vm = new EquipeViewModel { EcoleId = ecoleId };
        RebuildLists(vm);
        return vm;
    }

    private void RebuildLists(EquipeViewModel vm, bool loadThemesData = true)
    {
        var sports = _dictionnaireService.GetSports();
        vm.SportsList = sports.Select(s => new SelectListItem
        {
            Value = s.Key,
            Text = s.Label,
            Selected = s.Key == vm.TypeSport.ToString()
        }).ToList();

        var niveaux = _dictionnaireService.GetNiveaux(vm.TypeSport.ToString());
        vm.NiveauxList = niveaux.Select(n => new SelectListItem
        {
            Value = ToNiveauEnumName(n),
            Text = GetNiveauDisplayName(n),
            Selected = ToNiveauEnumName(n) == vm.Niveau.ToString()
        }).ToList();

        var anneesScolaires = _ecoleService.GetAnneesScolairesByEcole(vm.EcoleId)
            .Select(a => a.AnneeScolaire).ToList();
        vm.AnneesList = anneesScolaires.Select(a => new SelectListItem
        {
            Value = a,
            Text = a,
            Selected = a == vm.AnneeScolaire
        }).ToList();

        var themes = _ecoleService.GetThemesByEcole(vm.EcoleId);
        vm.ThemesList = themes.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = t.NomEquipe,
            Selected = t.Id == vm.ThemeId
        }).ToList();

        if (loadThemesData)
            ViewBag.ThemesData = themes;
    }

    private static string GetSportDisplayName(TypeSport sport) => sport switch
    {
        TypeSport.FootballAmericain => "Football",
        TypeSport.FlagFootball => "Flag Football",
        TypeSport.Soccer => "Soccer",
        TypeSport.Volleyball => "Volleyball",
        TypeSport.Hockey => "Hockey",
        _ => sport.ToString()
    };

    // Normalise une valeur de dictionnaire vers le nom exact de l'enum NiveauEquipe
    private static string ToNiveauEnumName(string valeur) => valeur switch
    {
        "Juvenil" or "Juvénile" or "juvénile" => "Juvenil",
        "PeeWee"  or "Pee-Wee"  or "pee-wee"  => "PeeWee",
        _ => valeur
    };

    private static string GetNiveauDisplayName(string niveau) => ToNiveauEnumName(niveau) switch
    {
        "Benjamin" => "Benjamin",
        "Cadet"    => "Cadet",
        "Juvenil"  => "Juvénile",
        "Atome"    => "Atome",
        "PeeWee"   => "Pee-Wee",
        "Bantam"   => "Bantam",
        _ => niveau
    };

    private static string SuggestNextYear(string annee)
    {
        var parts = annee.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[0], out var debut) && int.TryParse(parts[1], out var fin))
            return $"{debut + 1}-{fin + 1}";
        return annee;
    }

    private void SetTheme(Ecole ecole, ThemeEcole? theme = null)
    {
        ViewBag.CouleurPrimaire = theme?.CouleurPrimaire ?? ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = theme?.CouleurSecondaire ?? ecole.CouleurSecondaire;
        ViewBag.EcoleId = ecole.Id;
    }
}
