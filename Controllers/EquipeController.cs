using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionEquipeSportive.Controllers;

public class EquipeController : Controller
{
    private readonly IEquipeService _equipeService;
    private readonly IEcoleService _ecoleService;

    public EquipeController(IEquipeService equipeService, IEcoleService ecoleService)
    {
        _equipeService = equipeService;
        _ecoleService = ecoleService;
    }

    public IActionResult Index(int ecoleId)
    {
        var ecole = _ecoleService.GetEcoleById(ecoleId);
        if (ecole == null) return NotFound();

        SetTheme(ecole);
        ViewBag.Ecole = ecole;
        var equipes = _equipeService.GetEquipesByEcole(ecoleId);
        return View(equipes);
    }

    public IActionResult Details(int id)
    {
        var equipe = _equipeService.GetEquipeById(id);
        if (equipe == null) return NotFound();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        equipe.Ecole = ecole;
        return View(equipe);
    }

    public IActionResult Create(int ecoleId)
    {
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
        _equipeService.DeleteEquipe(id);
        TempData["Success"] = "Équipe supprimée avec succès.";
        return RedirectToAction(nameof(Index), new { ecoleId });
    }

    [HttpGet]
    public JsonResult GetNiveaux(string sport)
    {
        if (!Enum.TryParse<TypeSport>(sport, out var typeSport))
            return Json(new List<string>());

        var niveaux = _equipeService.GetNiveauxPourSport(typeSport);
        return Json(niveaux);
    }

    private EquipeViewModel BuildViewModel(int ecoleId)
    {
        var vm = new EquipeViewModel { EcoleId = ecoleId };
        RebuildLists(vm);
        return vm;
    }

    private void RebuildLists(EquipeViewModel vm)
    {
        vm.SportsList = Enum.GetValues<TypeSport>().Select(s => new SelectListItem
        {
            Value = s.ToString(),
            Text = GetSportDisplayName(s),
            Selected = s == vm.TypeSport
        }).ToList();

        var niveaux = _equipeService.GetNiveauxPourSport(vm.TypeSport);
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
        TypeSport.FootballAmericain => "Football américain",
        TypeSport.Soccer => "Soccer",
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

    private void SetTheme(Ecole ecole)
    {
        ViewBag.CouleurPrimaire = ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = ecole.CouleurSecondaire;
        ViewBag.EcoleId = ecole.Id;
    }
}
