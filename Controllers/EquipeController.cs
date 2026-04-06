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

    public EquipeController(IEquipeService equipeService, IEcoleService ecoleService,
        IEcoleAccessService access, IJoueurService joueurService,
        IStaffService staffService, IMatchService matchService,
        IEvenementService evenementService, IDictionnaireService dictionnaireService)
    {
        _equipeService = equipeService;
        _ecoleService = ecoleService;
        _access = access;
        _joueurService = joueurService;
        _staffService = staffService;
        _matchService = matchService;
        _evenementService = evenementService;
        _dictionnaireService = dictionnaireService;
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
            e => _joueurService.GetJoueursByEquipe(e.Id).Count);
        ViewBag.StatsEquipes = equipes.ToDictionary(
            e => e.Id,
            e => _matchService.GetStatistiques(e.Id));
        var aujourd = DateTime.Today;
        ViewBag.AnneeCourante = aujourd.Month >= 7
            ? $"{aujourd.Year}-{aujourd.Year + 1}"
            : $"{aujourd.Year - 1}-{aujourd.Year}";
        return View(equipes);
    }

    public IActionResult Details(int id)
    {
        var equipe = _equipeService.GetEquipeById(id);
        if (equipe == null) return NotFound();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        equipe.Ecole = ecole;

        var joueurs = _joueurService.GetJoueursByEquipe(id);
        var staff = _staffService.GetStaffByEquipe(id);
        var matchs = _matchService.GetMatchsByEquipe(id);
        var stats = _matchService.GetStatistiques(id);

        var evenements = _evenementService.GetEvenementsByEquipe(id);

        ViewBag.PeutModifier = _access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId);
        ViewBag.NbJoueurs = joueurs.Count;
        ViewBag.Joueurs = joueurs;
        ViewBag.Staff = staff;
        ViewBag.Matchs = matchs;
        ViewBag.Stats = stats;
        ViewBag.Evenements = evenements;
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
        ViewBag.Joueurs = _joueurService.GetJoueursByEquipe(id);
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
            ViewBag.Joueurs = _joueurService.GetJoueursByEquipe(id);
            ViewBag.Staff = _staffService.GetStaffByEquipe(id);
            return View(vm);
        }

        var nouvelleEquipe = _equipeService.CreateEquipe(vm);

        if (joueursIds.Length > 0)
            _joueurService.CopierVersEquipe(joueursIds, nouvelleEquipe.Id);

        if (staffIds.Length > 0)
            _staffService.CopierVersEquipe(staffIds, nouvelleEquipe.Id);

        TempData["Success"] = $"Équipe créée depuis \"{sourceEquipe.Nom}\" avec {joueursIds.Length} joueur(s) et {staffIds.Length} membre(s) de l'équipe école transféré(s).";
        return RedirectToAction(nameof(Details), new { id = nouvelleEquipe.Id });
    }

    [HttpGet]
    public JsonResult GetNiveaux(string sport)
    {
        var niveaux = _dictionnaireService.GetNiveaux(sport);
        return Json(niveaux.Select(n => new { value = n, text = GetNiveauDisplayName(n) }));
    }

    private EquipeViewModel BuildViewModel(int ecoleId)
    {
        var vm = new EquipeViewModel { EcoleId = ecoleId };
        RebuildLists(vm);
        return vm;
    }

    private void RebuildLists(EquipeViewModel vm)
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
            Value = n,
            Text = GetNiveauDisplayName(n),
            Selected = n == vm.Niveau.ToString()
        }).ToList();

        vm.AnneesList = _equipeService.GetAnnesScolaires().Select(a => new SelectListItem
        {
            Value = a,
            Text = a,
            Selected = a == vm.AnneeScolaire
        }).ToList();
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

    private static string GetNiveauDisplayName(string niveau) => niveau switch
    {
        "Benjamin" => "Benjamin",
        "Cadet" => "Cadet",
        "Juvenil" => "Juvénile",
        "Atome" => "Atome",
        "PeeWee" => "Pee-Wee",
        "Bantam" => "Bantam",
        _ => niveau
    };

    private static string SuggestNextYear(string annee)
    {
        var parts = annee.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[0], out var debut) && int.TryParse(parts[1], out var fin))
            return $"{debut + 1}-{fin + 1}";
        return annee;
    }

    private void SetTheme(Ecole ecole)
    {
        ViewBag.CouleurPrimaire = ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = ecole.CouleurSecondaire;
        ViewBag.EcoleId = ecole.Id;
    }
}
