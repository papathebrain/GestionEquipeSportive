using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class JoueurController : Controller
{
    private readonly IJoueurService _joueurService;
    private readonly IEquipeService _equipeService;
    private readonly IEcoleService _ecoleService;
    private readonly IEcoleAccessService _access;
    private readonly IWebHostEnvironment _env;

    public JoueurController(IJoueurService joueurService, IEquipeService equipeService,
        IEcoleService ecoleService, IEcoleAccessService access, IWebHostEnvironment env)
    {
        _joueurService = joueurService;
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
        var joueurs = _joueurService.GetJoueursByEquipe(equipeId);
        return View(joueurs);
    }

    public IActionResult Details(int id)
    {
        var joueur = _joueurService.GetJoueurById(id);
        if (joueur == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(joueur.EquipeId);
        var ecole = equipe != null ? _ecoleService.GetEcoleById(equipe.EcoleId) : null;
        if (ecole != null) SetTheme(ecole);

        joueur.Equipe = equipe;
        if (equipe != null) equipe.Ecole = ecole;
        ViewBag.PeutModifier = _access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0);

        // Historique : toutes les participations du joueur (par NoFiche ou nom/prénom)
        var historique = _joueurService.GetHistoriqueJoueur(joueur);
        foreach (var j in historique)
        {
            j.Equipe = _equipeService.GetEquipeById(j.EquipeId);
            if (j.Equipe != null)
                j.Equipe.Ecole = _ecoleService.GetEcoleById(j.Equipe.EcoleId);
        }
        ViewBag.Historique = historique;

        return View(joueur);
    }

    public IActionResult Create(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();

        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId))
            return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        var vm = new JoueurViewModel
        {
            EquipeId = equipeId,
            NomEquipe = equipe.Nom,
            EcoleId = equipe.EcoleId
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(JoueurViewModel vm)
    {
        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (equipe == null) return NotFound();

        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId))
            return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = _ecoleService.GetEcoleById(equipe.EcoleId);
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEquipe = equipe.Nom;
            return View(vm);
        }

        _joueurService.CreateJoueur(vm, vm.PhotoFile, _env.WebRootPath);
        TempData["Success"] = "Joueur ajouté avec succès.";
        return RedirectToAction(nameof(Index), new { equipeId = vm.EquipeId });
    }

    public IActionResult Edit(int id)
    {
        var joueur = _joueurService.GetJoueurById(id);
        if (joueur == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(joueur.EquipeId);
        var ecole = equipe != null ? _ecoleService.GetEcoleById(equipe.EcoleId) : null;

        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0))
            return Forbid();

        if (ecole != null) SetTheme(ecole);

        var vm = _joueurService.ToViewModel(joueur);
        vm.NomEquipe = equipe?.Nom;
        vm.EcoleId = equipe?.EcoleId ?? 0;
        ViewBag.JoueurMedias = _joueurService.GetMediasByJoueur(id);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, JoueurViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? vm.EcoleId))
            return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = equipe != null ? _ecoleService.GetEcoleById(equipe.EcoleId) : null;
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEquipe = equipe?.Nom;
            return View(vm);
        }

        _joueurService.UpdateJoueur(vm, vm.PhotoFile, _env.WebRootPath);
        TempData["Success"] = "Joueur modifié avec succès.";
        return RedirectToAction(nameof(Index), new { equipeId = vm.EquipeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id, int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipeId, equipe?.EcoleId ?? 0))
            return Forbid();

        _joueurService.DeleteJoueur(id, _env.WebRootPath);
        TempData["Success"] = "Joueur supprimé avec succès.";
        return RedirectToAction(nameof(Index), new { equipeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjouterMedia(int id, IFormFile fichier)
    {
        var joueur = _joueurService.GetJoueurById(id);
        if (joueur == null) return NotFound();
        var equipe = _equipeService.GetEquipeById(joueur.EquipeId);
        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0))
            return Forbid();
        if (fichier != null && fichier.Length > 0)
            _joueurService.AddJoueurMedia(id, fichier, _env.WebRootPath);
        TempData["Success"] = "Photo ajoutée.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SupprimerMedia(int mediaId, int joueurId)
    {
        var joueur = _joueurService.GetJoueurById(joueurId);
        var equipe = joueur != null ? _equipeService.GetEquipeById(joueur.EquipeId) : null;
        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0))
            return Forbid();
        _joueurService.DeleteJoueurMedia(mediaId, _env.WebRootPath);
        TempData["Success"] = "Photo supprimée.";
        return RedirectToAction(nameof(Edit), new { id = joueurId });
    }

    private void SetTheme(Ecole ecole)
    {
        ViewBag.CouleurPrimaire = ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = ecole.CouleurSecondaire;
    }
}
