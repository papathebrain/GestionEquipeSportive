using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class GalerieController : Controller
{
    private readonly IGalerieService _galerieService;
    private readonly IEquipeService _equipeService;
    private readonly IEcoleService _ecoleService;
    private readonly IEcoleAccessService _access;
    private readonly IWebHostEnvironment _env;

    public GalerieController(IGalerieService galerieService, IEquipeService equipeService,
        IEcoleService ecoleService, IEcoleAccessService access, IWebHostEnvironment env)
    {
        _galerieService = galerieService;
        _equipeService = equipeService;
        _ecoleService = ecoleService;
        _access = access;
        _env = env;
    }

    public IActionResult Index(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        equipe.Ecole = ecole;
        ViewBag.Equipe = equipe;
        ViewBag.PeutModifier = _access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId);
        var photos = _galerieService.GetPhotosByEquipe(equipeId);
        return View(photos);
    }

    public IActionResult Upload(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();

        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId))
            return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        equipe.Ecole = ecole;
        ViewBag.Equipe = equipe;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upload(int equipeId, IFormFile photoFile, string? description)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();

        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId))
            return Forbid();

        if (photoFile == null || photoFile.Length == 0)
        {
            ModelState.AddModelError("", "Veuillez sélectionner une photo.");
            var ecole2 = _ecoleService.GetEcoleById(equipe.EcoleId);
            if (ecole2 != null) SetTheme(ecole2);
            equipe.Ecole = ecole2;
            ViewBag.Equipe = equipe;
            return View();
        }

        _galerieService.AddPhoto(equipeId, photoFile, description, _env.WebRootPath);
        TempData["Success"] = "Photo ajoutée à la galerie.";
        return RedirectToAction(nameof(Index), new { equipeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id, int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipeId, equipe?.EcoleId ?? 0))
            return Forbid();

        _galerieService.DeletePhoto(id, _env.WebRootPath);
        TempData["Success"] = "Photo supprimée.";
        return RedirectToAction(nameof(Index), new { equipeId });
    }

    private void SetTheme(Ecole ecole)
    {
        ViewBag.CouleurPrimaire = ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = ecole.CouleurSecondaire;
    }
}
