using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

[Authorize(Roles = Roles.Admin)]
public class UtilisateurController : Controller
{
    private readonly IUserService _userService;
    private readonly IEcoleService _ecoleService;
    private readonly IEquipeService _equipeService;

    public UtilisateurController(IUserService userService, IEcoleService ecoleService, IEquipeService equipeService)
    {
        _userService = userService;
        _ecoleService = ecoleService;
        _equipeService = equipeService;
    }

    public IActionResult Index()
        => View(_userService.GetAllUsers());

    [HttpGet]
    public IActionResult Create()
        => View(new UtilisateurCreateViewModel { Ecoles = BuildEcoleAcces([], []) });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UtilisateurCreateViewModel model)
    {
        if (_userService.NomUtilisateurExiste(model.NomUtilisateur))
            ModelState.AddModelError(nameof(model.NomUtilisateur), "Ce nom d'utilisateur est déjà utilisé.");

        if (!ModelState.IsValid)
        {
            model.Ecoles = BuildEcoleAcces(
                model.Ecoles.Where(e => e.AccesComplet).Select(e => e.Id).ToList(),
                model.Ecoles.SelectMany(e => e.Equipes.Where(eq => eq.Selectionne).Select(eq => eq.Id)).ToList());
            return View(model);
        }

        var (ecolesIds, equipesIds) = ExtraireAcces(model.Ecoles, model.Role);

        var user = new ApplicationUser
        {
            NomUtilisateur = model.NomUtilisateur.Trim(),
            NomComplet = model.NomComplet.Trim(),
            Role = model.Role,
            EstActif = true,
            ChangerMotDePasse = model.ChangerMotDePasse,
            EcolesIds = ecolesIds,
            EquipesIds = equipesIds
        };

        _userService.Ajouter(user, model.MotDePasse);
        TempData["Success"] = $"Utilisateur « {user.NomUtilisateur} » créé avec succès.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(string id)
    {
        var user = _userService.GetById(id);
        if (user == null) return NotFound();

        var vm = new UtilisateurEditViewModel
        {
            Id = user.Id,
            NomUtilisateur = user.NomUtilisateur,
            NomComplet = user.NomComplet,
            Role = user.Role,
            EstActif = user.EstActif,
            ChangerMotDePasse = user.ChangerMotDePasse,
            Ecoles = BuildEcoleAcces(user.EcolesIds, user.EquipesIds)
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UtilisateurEditViewModel model)
    {
        if (_userService.NomUtilisateurExiste(model.NomUtilisateur, model.Id))
            ModelState.AddModelError(nameof(model.NomUtilisateur), "Ce nom d'utilisateur est déjà utilisé.");

        if (string.IsNullOrEmpty(model.NouveauMotDePasse))
        {
            ModelState.Remove(nameof(model.NouveauMotDePasse));
            ModelState.Remove(nameof(model.ConfirmerMotDePasse));
        }

        if (!ModelState.IsValid)
        {
            model.Ecoles = BuildEcoleAcces(
                model.Ecoles.Where(e => e.AccesComplet).Select(e => e.Id).ToList(),
                model.Ecoles.SelectMany(e => e.Equipes.Where(eq => eq.Selectionne).Select(eq => eq.Id)).ToList());
            return View(model);
        }

        var user = _userService.GetById(model.Id);
        if (user == null) return NotFound();

        var (ecolesIds, equipesIds) = ExtraireAcces(model.Ecoles, model.Role);

        user.NomUtilisateur = model.NomUtilisateur.Trim();
        user.NomComplet = model.NomComplet.Trim();
        user.Role = model.Role;
        user.EstActif = model.EstActif;
        user.ChangerMotDePasse = model.ChangerMotDePasse;
        user.EcolesIds = ecolesIds;
        user.EquipesIds = equipesIds;

        _userService.Modifier(user);

        if (!string.IsNullOrEmpty(model.NouveauMotDePasse))
            _userService.ChangerMotDePasse(user.Id, model.NouveauMotDePasse);

        TempData["Success"] = $"Utilisateur « {user.NomUtilisateur} » modifié avec succès.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Supprimer(string id)
    {
        var user = _userService.GetById(id);
        if (user == null) return NotFound();

        var admins = _userService.GetAllUsers().Where(u => u.Role == Roles.Admin && u.EstActif).ToList();
        if (user.Role == Roles.Admin && admins.Count == 1)
        {
            TempData["Error"] = "Impossible de supprimer le dernier administrateur.";
            return RedirectToAction(nameof(Index));
        }

        _userService.Supprimer(id);
        TempData["Success"] = $"Utilisateur « {user.NomUtilisateur} » supprimé.";
        return RedirectToAction(nameof(Index));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private List<EcoleAccesViewModel> BuildEcoleAcces(List<int> ecolesIds, List<int> equipesIds)
    {
        return _ecoleService.GetAllEcoles().Select(ecole => new EcoleAccesViewModel
        {
            Id = ecole.Id,
            Nom = ecole.Nom,
            AccesComplet = ecolesIds.Contains(ecole.Id),
            Equipes = _equipeService.GetEquipesByEcole(ecole.Id).Select(eq => new EquipeAccesItem
            {
                Id = eq.Id,
                Nom = eq.Nom,
                Selectionne = equipesIds.Contains(eq.Id)
            }).ToList()
        }).ToList();
    }

    private static (List<int> ecolesIds, List<int> equipesIds) ExtraireAcces(
        List<EcoleAccesViewModel> ecoles, string role)
    {
        if (role == Roles.Admin)
            return ([], []);

        var ecolesIds = ecoles.Where(e => e.AccesComplet).Select(e => e.Id).ToList();

        // N'inclure les équipes spécifiques que pour les écoles sans accès complet
        var equipesIds = ecoles
            .Where(e => !e.AccesComplet)
            .SelectMany(e => e.Equipes.Where(eq => eq.Selectionne).Select(eq => eq.Id))
            .ToList();

        return (ecolesIds, equipesIds);
    }
}
