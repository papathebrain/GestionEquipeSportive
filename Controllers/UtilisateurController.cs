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

    public UtilisateurController(IUserService userService)
    {
        _userService = userService;
    }

    public IActionResult Index()
    {
        var users = _userService.GetAllUsers();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create()
        => View(new UtilisateurCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UtilisateurCreateViewModel model)
    {
        if (_userService.NomUtilisateurExiste(model.NomUtilisateur))
            ModelState.AddModelError(nameof(model.NomUtilisateur), "Ce nom d'utilisateur est déjà utilisé.");

        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            NomUtilisateur = model.NomUtilisateur.Trim(),
            NomComplet = model.NomComplet.Trim(),
            Role = model.Role,
            EstActif = true,
            ChangerMotDePasse = model.ChangerMotDePasse
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
            ChangerMotDePasse = user.ChangerMotDePasse
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UtilisateurEditViewModel model)
    {
        if (_userService.NomUtilisateurExiste(model.NomUtilisateur, model.Id))
            ModelState.AddModelError(nameof(model.NomUtilisateur), "Ce nom d'utilisateur est déjà utilisé.");

        // Valider le mot de passe seulement s'il est fourni
        if (string.IsNullOrEmpty(model.NouveauMotDePasse))
        {
            ModelState.Remove(nameof(model.NouveauMotDePasse));
            ModelState.Remove(nameof(model.ConfirmerMotDePasse));
        }

        if (!ModelState.IsValid)
            return View(model);

        var user = _userService.GetById(model.Id);
        if (user == null) return NotFound();

        user.NomUtilisateur = model.NomUtilisateur.Trim();
        user.NomComplet = model.NomComplet.Trim();
        user.Role = model.Role;
        user.EstActif = model.EstActif;
        user.ChangerMotDePasse = model.ChangerMotDePasse;

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

        // Empêcher la suppression du dernier admin
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
}
