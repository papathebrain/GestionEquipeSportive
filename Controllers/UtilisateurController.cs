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

    public UtilisateurController(IUserService userService, IEcoleService ecoleService)
    {
        _userService = userService;
        _ecoleService = ecoleService;
    }

    public IActionResult Index()
    {
        var users = _userService.GetAllUsers();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var vm = new UtilisateurCreateViewModel
        {
            Ecoles = BuildEcoleCheckboxes([])
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UtilisateurCreateViewModel model)
    {
        if (_userService.NomUtilisateurExiste(model.NomUtilisateur))
            ModelState.AddModelError(nameof(model.NomUtilisateur), "Ce nom d'utilisateur est déjà utilisé.");

        if (!ModelState.IsValid)
        {
            model.Ecoles = BuildEcoleCheckboxes(model.Ecoles
                .Where(e => e.Selectionne).Select(e => e.Id).ToList());
            return View(model);
        }

        var ecolesSelectionnees = model.Ecoles
            .Where(e => e.Selectionne).Select(e => e.Id).ToList();

        var user = new ApplicationUser
        {
            NomUtilisateur = model.NomUtilisateur.Trim(),
            NomComplet = model.NomComplet.Trim(),
            Role = model.Role,
            EstActif = true,
            ChangerMotDePasse = model.ChangerMotDePasse,
            EcolesIds = model.Role == Roles.Admin ? [] : ecolesSelectionnees
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
            Ecoles = BuildEcoleCheckboxes(user.EcolesIds)
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
            model.Ecoles = BuildEcoleCheckboxes(model.Ecoles
                .Where(e => e.Selectionne).Select(e => e.Id).ToList());
            return View(model);
        }

        var user = _userService.GetById(model.Id);
        if (user == null) return NotFound();

        var ecolesSelectionnees = model.Ecoles
            .Where(e => e.Selectionne).Select(e => e.Id).ToList();

        user.NomUtilisateur = model.NomUtilisateur.Trim();
        user.NomComplet = model.NomComplet.Trim();
        user.Role = model.Role;
        user.EstActif = model.EstActif;
        user.ChangerMotDePasse = model.ChangerMotDePasse;
        // Les admins n'ont pas besoin d'association d'écoles
        user.EcolesIds = model.Role == Roles.Admin ? [] : ecolesSelectionnees;

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

    private List<EcoleCheckboxItem> BuildEcoleCheckboxes(List<int> selectedIds)
        => _ecoleService.GetAllEcoles().Select(e => new EcoleCheckboxItem
        {
            Id = e.Id,
            Nom = e.Nom,
            Selectionne = selectedIds.Contains(e.Id)
        }).ToList();
}
