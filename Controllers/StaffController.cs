using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class StaffController : Controller
{
    private readonly IStaffService _staffService;
    private readonly IEquipeService _equipeService;
    private readonly IEcoleService _ecoleService;
    private readonly IEcoleAccessService _access;
    private readonly IWebHostEnvironment _env;

    public StaffController(IStaffService staffService, IEquipeService equipeService,
        IEcoleService ecoleService, IEcoleAccessService access, IWebHostEnvironment env)
    {
        _staffService = staffService;
        _equipeService = equipeService;
        _ecoleService = ecoleService;
        _access = access;
        _env = env;
    }

    // ─── Index ────────────────────────────────────────────────────────────────

    public IActionResult Index(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        ViewBag.Equipe = equipe;
        ViewBag.Ecole = ecole;
        ViewBag.PeutModifier = _access.PeutModifierEquipe(User, equipeId, equipe.EcoleId);

        var staff = _staffService.GetStaffByEquipe(equipeId)
            .OrderBy(s => s.Titre).ThenBy(s => s.Nom).ToList();
        return View(staff);
    }

    // ─── Create ───────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Create(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipeId, equipe.EcoleId)) return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        return View(new StaffViewModel
        {
            EquipeId = equipeId,
            EcoleId = equipe.EcoleId,
            NomEquipe = equipe.Nom
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(StaffViewModel vm)
    {
        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, vm.EquipeId, equipe.EcoleId)) return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = _ecoleService.GetEcoleById(equipe.EcoleId);
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEquipe = equipe.Nom;
            vm.EcoleId = equipe.EcoleId;
            return View(vm);
        }

        var staff = _staffService.CreateStaff(vm, vm.PhotoFile, _env.WebRootPath);
        TempData["Success"] = $"{staff.Prenom} {staff.Nom} ajouté(e) au staff.";
        return RedirectToAction(nameof(Index), new { equipeId = vm.EquipeId });
    }

    // ─── Edit ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var staff = _staffService.GetStaffById(id);
        if (staff == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(staff.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        var vm = _staffService.ToViewModel(staff);
        vm.NomEquipe = equipe.Nom;
        vm.EcoleId = equipe.EcoleId;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(StaffViewModel vm)
    {
        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = _ecoleService.GetEcoleById(equipe.EcoleId);
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEquipe = equipe.Nom;
            vm.EcoleId = equipe.EcoleId;
            return View(vm);
        }

        var staff = _staffService.UpdateStaff(vm, vm.PhotoFile, _env.WebRootPath);
        TempData["Success"] = $"{staff.Prenom} {staff.Nom} modifié(e).";
        return RedirectToAction(nameof(Index), new { equipeId = vm.EquipeId });
    }

    // ─── Delete ───────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id, int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipeId, equipe.EcoleId)) return Forbid();

        var staff = _staffService.GetStaffById(id);
        if (staff == null) return NotFound();

        _staffService.DeleteStaff(id, _env.WebRootPath);
        TempData["Success"] = $"{staff.Prenom} {staff.Nom} supprimé(e) du staff.";
        return RedirectToAction(nameof(Index), new { equipeId });
    }

    private void SetTheme(Ecole ecole)
    {
        ViewBag.CouleurPrimaire = ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = ecole.CouleurSecondaire;
    }
}
