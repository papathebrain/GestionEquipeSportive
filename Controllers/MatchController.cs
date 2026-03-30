using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class MatchController : Controller
{
    private readonly IMatchService _matchService;
    private readonly IEquipeService _equipeService;
    private readonly IEcoleService _ecoleService;
    private readonly IEcoleAccessService _access;
    private readonly IWebHostEnvironment _env;
    private readonly IJoueurService _joueurService;

    public MatchController(IMatchService matchService, IEquipeService equipeService,
        IEcoleService ecoleService, IEcoleAccessService access, IWebHostEnvironment env,
        IJoueurService joueurService)
    {
        _matchService = matchService;
        _equipeService = equipeService;
        _ecoleService = ecoleService;
        _access = access;
        _env = env;
        _joueurService = joueurService;
    }

    // ─── Index ────────────────────────────────────────────────────────────────

    public IActionResult Index(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        var matchs = _matchService.GetMatchsByEquipe(equipeId);
        var stats = _matchService.GetStatistiques(equipeId);

        ViewBag.Equipe = equipe;
        ViewBag.Ecole = ecole;
        ViewBag.Stats = stats;
        ViewBag.PeutModifier = _access.PeutModifierEquipe(User, equipeId, equipe.EcoleId);

        return View(matchs);
    }

    // ─── Details ──────────────────────────────────────────────────────────────

    public IActionResult Details(int id)
    {
        var match = _matchService.GetMatchById(id);
        if (match == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(match.EquipeId);
        var ecole = equipe != null ? _ecoleService.GetEcoleById(equipe.EcoleId) : null;
        if (ecole != null) SetTheme(ecole);

        match.Equipe = equipe;

        var medias = _matchService.GetMediasByMatch(id);

        ViewBag.Ecole = ecole;
        ViewBag.Medias = medias;
        ViewBag.PeutModifier = equipe != null && _access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId);

        var joueurs = _joueurService.GetJoueursByEquipe(match.EquipeId);
        ViewBag.Joueurs = joueurs;
        ViewBag.Absences = _matchService.GetAbsencesByMatch(id);

        return View(match);
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

        var vm = new MatchViewModel
        {
            EquipeId = equipeId,
            EcoleId = equipe.EcoleId,
            NomEquipe = equipe.Nom,
            NomEcole = ecole?.Nom,
            DateMatch = DateTime.Today
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(MatchViewModel vm)
    {
        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, vm.EquipeId, equipe.EcoleId)) return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = _ecoleService.GetEcoleById(equipe.EcoleId);
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEquipe = equipe.Nom;
            vm.NomEcole = ecole2?.Nom;
            vm.EcoleId = equipe.EcoleId;
            return View(vm);
        }

        var match = _matchService.CreateMatch(vm);
        TempData["Success"] = $"Match contre {match.Adversaire} ajouté avec succès.";
        return RedirectToAction(nameof(Index), new { equipeId = vm.EquipeId });
    }

    // ─── Edit ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var match = _matchService.GetMatchById(id);
        if (match == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(match.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        var vm = _matchService.ToViewModel(match);
        vm.NomEquipe = equipe.Nom;
        vm.NomEcole = ecole?.Nom;
        vm.EcoleId = equipe.EcoleId;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, MatchViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = _ecoleService.GetEcoleById(equipe.EcoleId);
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEquipe = equipe.Nom;
            vm.NomEcole = ecole2?.Nom;
            vm.EcoleId = equipe.EcoleId;
            return View(vm);
        }

        _matchService.UpdateMatch(vm);
        TempData["Success"] = "Match modifié avec succès.";
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

        // Supprimer les fichiers médias
        foreach (var media in _matchService.GetMediasByMatch(id))
            _matchService.DeleteMedia(media.Id, _env.WebRootPath);

        _matchService.DeleteMatch(id);
        TempData["Success"] = "Match supprimé.";
        return RedirectToAction(nameof(Index), new { equipeId });
    }

    // ─── Médias ───────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjouterMedia(int matchId, List<IFormFile> fichiers, string typeMedia, string? description)
    {
        var match = _matchService.GetMatchById(matchId);
        if (match == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(match.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        var valides = fichiers?.Where(f => f != null && f.Length > 0).ToList() ?? new();
        if (!valides.Any())
        {
            TempData["Error"] = "Veuillez sélectionner au moins un fichier.";
            return RedirectToAction(nameof(Details), new { id = matchId });
        }

        var type = typeMedia == "Video" ? TypeMedia.Video : TypeMedia.Photo;
        var ajoutees = _matchService.AddMedias(matchId, valides, type, description, _env.WebRootPath);
        TempData["Success"] = ajoutees.Count == 1 ? "Média ajouté." : $"{ajoutees.Count} médias ajoutés.";
        return RedirectToAction(nameof(Details), new { id = matchId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SupprimerMedia(int id, int matchId)
    {
        var match = _matchService.GetMatchById(matchId);
        if (match == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(match.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        _matchService.DeleteMedia(id, _env.WebRootPath);
        TempData["Success"] = "Média supprimé.";
        return RedirectToAction(nameof(Details), new { id = matchId });
    }

    // ─── Absences ─────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleAbsence(int matchId, int joueurId)
    {
        var match = _matchService.GetMatchById(matchId);
        if (match == null) return NotFound();
        var equipe = _equipeService.GetEquipeById(match.EquipeId);
        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0))
            return Forbid();
        _matchService.ToggleAbsence(matchId, joueurId);
        return RedirectToAction(nameof(Details), new { id = matchId });
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void SetTheme(Ecole ecole)
    {
        ViewBag.CouleurPrimaire = ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = ecole.CouleurSecondaire;
    }
}
