using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

public class EcoleController : Controller
{
    private readonly IEcoleService _ecoleService;
    private readonly IEquipeService _equipeService;
    private readonly IWebHostEnvironment _env;

    public EcoleController(IEcoleService ecoleService, IEquipeService equipeService, IWebHostEnvironment env)
    {
        _ecoleService = ecoleService;
        _equipeService = equipeService;
        _env = env;
    }

    public IActionResult Index()
    {
        var ecoles = _ecoleService.GetAllEcoles();
        return View(ecoles);
    }

    public IActionResult Details(int id)
    {
        var ecole = _ecoleService.GetEcoleById(id);
        if (ecole == null) return NotFound();

        SetTheme(ecole);
        var equipes = _equipeService.GetEquipesByEcole(id);
        ViewBag.Equipes = equipes;
        ViewBag.Ecole = ecole;
        return View(ecole);
    }

    public IActionResult Create()
    {
        return View(new EcoleViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(EcoleViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        _ecoleService.CreateEcole(vm, vm.LogoFile, _env.WebRootPath);
        TempData["Success"] = "École créée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var ecole = _ecoleService.GetEcoleById(id);
        if (ecole == null) return NotFound();
        return View(_ecoleService.ToViewModel(ecole));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
