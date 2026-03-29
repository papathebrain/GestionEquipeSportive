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

    public EcoleController(IEcoleService ecoleService, IEquipeService equipeService,
        IEcoleAccessService access, IWebHostEnvironment env)
    {
        _ecoleService = ecoleService;
        _equipeService = equipeService;
        _access = access;
        _env = env;
    }

    public IActionResult Index()
    {
        var toutes = _ecoleService.GetAllEcoles();
        var visibles = _access.GetEcolesVisibles(User, toutes.Select(e => e.Id)).ToHashSet();
        var ecoles = toutes.Where(e => visibles.Contains(e.Id)).ToList();

        // Redirection automatique si 1 seule école accessible
        if (ecoles.Count == 1)
            return RedirectToAction(nameof(Details), new { id = ecoles[0].Id });

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

        ViewBag.Equipes = equipes;
        ViewBag.Annees = annees;
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
