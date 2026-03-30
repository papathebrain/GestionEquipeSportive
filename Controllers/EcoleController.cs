using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class EcoleController : Controller
{
    private readonly IEcoleService _ecoleService;
    private readonly IEquipeService _equipeService;
    private readonly IEcoleAccessService _access;
    private readonly IWebHostEnvironment _env;
    private readonly IJoueurService _joueurService;

    public EcoleController(IEcoleService ecoleService, IEquipeService equipeService,
        IEcoleAccessService access, IWebHostEnvironment env, IJoueurService joueurService)
    {
        _ecoleService = ecoleService;
        _equipeService = equipeService;
        _access = access;
        _env = env;
        _joueurService = joueurService;
    }

    public IActionResult Index()
    {
        var toutes = _ecoleService.GetAllEcoles();
        var visibles = _access.GetEcolesVisibles(User, toutes.Select(e => e.Id)).ToHashSet();
        var ecoles = toutes.Where(e => visibles.Contains(e.Id)).ToList();

        // Redirection automatique si 1 seule école accessible
        if (ecoles.Count == 1)
        {
            // AdminEcole → page de gestion des équipes directement
            if (User.IsInRole(Roles.AdminEcole) && !User.IsInRole(Roles.Admin))
                return RedirectToAction("Index", "Equipe", new { ecoleId = ecoles[0].Id });
            return RedirectToAction(nameof(Details), new { id = ecoles[0].Id });
        }

        return View(ecoles);
    }

    public IActionResult Details(int id)
    {
        var ecole = _ecoleService.GetEcoleById(id);
        if (ecole == null) return NotFound();

        // Vérifier que l'utilisateur a accès à cette école
        var visibles = _access.GetEcolesVisibles(User, [id]).ToList();
        if (!visibles.Contains(id)) return Forbid();

        SetTheme(ecole);

        var toutesEquipes = _equipeService.GetEquipesByEcole(id);
        var equipes = _access.FiltrerEquipes(User, toutesEquipes, id).ToList();

        var annees = equipes.Select(e => e.AnneeScolaire)
            .Where(a => !string.IsNullOrEmpty(a))
            .Distinct().OrderByDescending(a => a).ToList();

        var nbJoueurs = equipes.ToDictionary(
            e => e.Id,
            e => _joueurService.GetJoueursByEquipe(e.Id).Count);

        // Année scolaire courante (1 juillet → 30 juin)
        var aujourd = DateTime.Today;
        var anneeCourante = aujourd.Month >= 7
            ? $"{aujourd.Year}-{aujourd.Year + 1}"
            : $"{aujourd.Year - 1}-{aujourd.Year}";

        ViewBag.Equipes = equipes;
        ViewBag.Annees = annees;
        ViewBag.NbJoueurs = nbJoueurs;
        ViewBag.AnneeCourante = anneeCourante;
        ViewBag.PeutModifier = _access.PeutModifier(User, id);
        return View(ecole);
    }

    // ─── Actions réservées aux admins ─────────────────────────────────────────

    [Authorize(Roles = Roles.Admin)]
    public IActionResult Create()
        => View(new EcoleViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Create(EcoleViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        _ecoleService.CreateEcole(vm, vm.LogoFile, _env.WebRootPath);
        TempData["Success"] = "École créée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    public IActionResult Edit(int id)
    {
        var ecole = _ecoleService.GetEcoleById(id);
        if (ecole == null) return NotFound();
        return View(_ecoleService.ToViewModel(ecole));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Edit(int id, EcoleViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);
        _ecoleService.UpdateEcole(vm, vm.LogoFile, _env.WebRootPath);
        TempData["Success"] = "École modifiée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Delete(int id)
    {
        _ecoleService.DeleteEcole(id);
        TempData["Success"] = "École supprimée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    private void SetTheme(Ecole ecole)
    {
        ViewBag.CouleurPrimaire = ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = ecole.CouleurSecondaire;
    }
}
