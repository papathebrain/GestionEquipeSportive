using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionEquipeSportive.Controllers;

[Authorize(Roles = Roles.AdminOuAdminEcole)]
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

    // ─── Index ────────────────────────────────────────────────────────────────

    public IActionResult Index()
    {
        var ecolesGerables = GetEcolesGerables();
        IEnumerable<ApplicationUser> users;

        if (EstAdminGlobal())
        {
            users = _userService.GetAllUsers();
        }
        else
        {
            // AdminEcole : voir seulement les utilisateurs de ses écoles
            // + les utilisateurs qui ont des équipes dans ses écoles
            users = _userService.GetAllUsers()
                .Where(u => u.Role != Roles.Admin // jamais les admins globaux
                    && (u.EcolesIds.Any(id => ecolesGerables.Contains(id))
                        || u.EquipesIds.Any(equipeId =>
                        {
                            var eq = _equipeService.GetEquipeById(equipeId);
                            return eq != null && ecolesGerables.Contains(eq.EcoleId);
                        })));
        }

        ViewBag.EstAdminGlobal = EstAdminGlobal();
        return View(users.OrderBy(u => u.NomUtilisateur));
    }

    // ─── Create ───────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Create()
    {
        var vm = new UtilisateurCreateViewModel
        {
            Ecoles = BuildEcoleAcces([], []),
            RolesDisponibles = GetRolesDisponibles()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UtilisateurCreateViewModel model)
    {
        if (_userService.NomUtilisateurExiste(model.NomUtilisateur))
            ModelState.AddModelError(nameof(model.NomUtilisateur), "Ce nom d'utilisateur est déjà utilisé.");

        // AdminEcole ne peut pas créer un Admin global ou AdminEcole
        if (!EstAdminGlobal() && model.Role == Roles.Admin)
            ModelState.AddModelError(nameof(model.Role), "Vous ne pouvez pas créer un administrateur global.");
        if (!EstAdminGlobal() && model.Role == Roles.AdminEcole)
            ModelState.AddModelError(nameof(model.Role), "Vous ne pouvez pas créer un administrateur d'école.");

        if (!ModelState.IsValid)
        {
            model.Ecoles = RebuildEcoles(model.Ecoles);
            model.RolesDisponibles = GetRolesDisponibles();
            return View(model);
        }

        var (ecolesIds, equipesIds) = ExtraireAcces(model.Ecoles, model.Role);

        // AdminEcole : forcer les écoles assignées à ses propres écoles
        if (!EstAdminGlobal())
        {
            var gerables = GetEcolesGerables();
            ecolesIds = ecolesIds.Where(gerables.Contains).ToList();
            equipesIds = equipesIds.Where(equipeId =>
            {
                var eq = _equipeService.GetEquipeById(equipeId);
                return eq != null && gerables.Contains(eq.EcoleId);
            }).ToList();
        }

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

    // ─── Edit ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Edit(string id)
    {
        var user = _userService.GetById(id);
        if (user == null) return NotFound();

        // AdminEcole ne peut pas modifier un Admin global ou AdminEcole
        if (!EstAdminGlobal() && user.Role is Roles.Admin or Roles.AdminEcole)
            return Forbid();

        // AdminEcole ne peut modifier que les utilisateurs de ses écoles
        if (!EstAdminGlobal() && !UtilisateurDansMesEcoles(user))
            return Forbid();

        var vm = new UtilisateurEditViewModel
        {
            Id = user.Id,
            NomUtilisateur = user.NomUtilisateur,
            NomComplet = user.NomComplet,
            Role = user.Role,
            EstActif = user.EstActif,
            ChangerMotDePasse = user.ChangerMotDePasse,
            Ecoles = BuildEcoleAcces(user.EcolesIds, user.EquipesIds),
            RolesDisponibles = GetRolesDisponibles()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UtilisateurEditViewModel model)
    {
        var user = _userService.GetById(model.Id);
        if (user == null) return NotFound();

        if (!EstAdminGlobal() && user.Role is Roles.Admin or Roles.AdminEcole)
            return Forbid();
        if (!EstAdminGlobal() && !UtilisateurDansMesEcoles(user))
            return Forbid();

        // AdminEcole ne peut pas promouvoir à Admin/AdminEcole
        if (!EstAdminGlobal() && model.Role is Roles.Admin or Roles.AdminEcole)
            ModelState.AddModelError(nameof(model.Role), "Vous ne pouvez pas attribuer ce rôle.");

        if (_userService.NomUtilisateurExiste(model.NomUtilisateur, model.Id))
            ModelState.AddModelError(nameof(model.NomUtilisateur), "Ce nom d'utilisateur est déjà utilisé.");

        if (string.IsNullOrEmpty(model.NouveauMotDePasse))
        {
            ModelState.Remove(nameof(model.NouveauMotDePasse));
            ModelState.Remove(nameof(model.ConfirmerMotDePasse));
        }

        if (!ModelState.IsValid)
        {
            model.Ecoles = RebuildEcoles(model.Ecoles);
            model.RolesDisponibles = GetRolesDisponibles();
            return View(model);
        }

        var (ecolesIds, equipesIds) = ExtraireAcces(model.Ecoles, model.Role);

        if (!EstAdminGlobal())
        {
            var gerables = GetEcolesGerables();
            // Garder les accès existants hors de ses écoles + ajouter les nouveaux dans ses écoles
            var ecolesHorsScopeExistants = user.EcolesIds.Where(id => !gerables.Contains(id)).ToList();
            var equipesHorsScopeExistants = user.EquipesIds.Where(equipeId =>
            {
                var eq = _equipeService.GetEquipeById(equipeId);
                return eq != null && !gerables.Contains(eq.EcoleId);
            }).ToList();

            ecolesIds = ecolesHorsScopeExistants
                .Concat(ecolesIds.Where(gerables.Contains))
                .Distinct().ToList();
            equipesIds = equipesHorsScopeExistants
                .Concat(equipesIds.Where(equipeId =>
                {
                    var eq = _equipeService.GetEquipeById(equipeId);
                    return eq != null && gerables.Contains(eq.EcoleId);
                }))
                .Distinct().ToList();
        }

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

    // ─── Supprimer ────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Supprimer(string id)
    {
        var user = _userService.GetById(id);
        if (user == null) return NotFound();

        if (!EstAdminGlobal() && user.Role is Roles.Admin or Roles.AdminEcole)
            return Forbid();
        if (!EstAdminGlobal() && !UtilisateurDansMesEcoles(user))
            return Forbid();

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

    private bool EstAdminGlobal() => User.IsInRole(Roles.Admin);

    private HashSet<int> GetEcolesGerables()
    {
        if (EstAdminGlobal())
            return _ecoleService.GetAllEcoles().Select(e => e.Id).ToHashSet();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var appUser = userId != null ? _userService.GetById(userId) : null;
        return appUser?.EcolesIds.ToHashSet() ?? [];
    }

    private bool UtilisateurDansMesEcoles(ApplicationUser user)
    {
        var gerables = GetEcolesGerables();
        return user.EcolesIds.Any(gerables.Contains)
            || user.EquipesIds.Any(equipeId =>
            {
                var eq = _equipeService.GetEquipeById(equipeId);
                return eq != null && gerables.Contains(eq.EcoleId);
            });
    }

    private List<(string Value, string Text)> GetRolesDisponibles()
    {
        var roles = new List<(string, string)>
        {
            (Roles.Utilisateur, "Utilisateur")
        };
        if (EstAdminGlobal())
        {
            roles.Add((Roles.AdminEcole, "Administrateur d'école"));
            roles.Add((Roles.Admin, "Administrateur global"));
        }
        return roles;
    }

    private List<EcoleAccesViewModel> BuildEcoleAcces(List<int> ecolesIds, List<int> equipesIds)
    {
        var ecoles = EstAdminGlobal()
            ? _ecoleService.GetAllEcoles()
            : _ecoleService.GetAllEcoles()
                .Where(e => GetEcolesGerables().Contains(e.Id))
                .ToList();

        return ecoles.Select(ecole => new EcoleAccesViewModel
        {
            Id = ecole.Id,
            Nom = ecole.Nom,
            AccesComplet = ecolesIds.Contains(ecole.Id),
            Equipes = _equipeService.GetEquipesByEcole(ecole.Id)
                .OrderBy(eq => eq.AnneeScolaire)
                .ThenBy(eq => eq.TypeSport.ToString())
                .ThenBy(eq => eq.Niveau.ToString())
                .Select(eq => new EquipeAccesItem
                {
                    Id = eq.Id,
                    Nom = eq.Nom,
                    AnneeScolaire = eq.AnneeScolaire,
                    Sport = GetSportDisplay(eq.TypeSport),
                    Niveau = GetNiveauDisplay(eq.Niveau),
                    Selectionne = equipesIds.Contains(eq.Id)
                }).ToList()
        }).ToList();
    }

    private List<EcoleAccesViewModel> RebuildEcoles(List<EcoleAccesViewModel> soumis)
        => BuildEcoleAcces(
            soumis.Where(e => e.AccesComplet).Select(e => e.Id).ToList(),
            soumis.SelectMany(e => e.Equipes.Where(eq => eq.Selectionne).Select(eq => eq.Id)).ToList());

    private static string GetSportDisplay(TypeSport sport) => sport switch
    {
        TypeSport.FootballAmericain => "Football",
        TypeSport.FlagFootball => "Flag Football",
        TypeSport.Soccer => "Soccer",
        TypeSport.Volleyball => "Volleyball",
        TypeSport.Hockey => "Hockey",
        _ => sport.ToString()
    };

    private static string GetNiveauDisplay(NiveauEquipe niveau) => niveau switch
    {
        NiveauEquipe.Juvenil => "Juvénile",
        NiveauEquipe.PeeWee => "Pee-Wee",
        _ => niveau.ToString()
    };

    private static (List<int> ecolesIds, List<int> equipesIds) ExtraireAcces(
        List<EcoleAccesViewModel> ecoles, string role)
    {
        if (role == Roles.Admin)
            return ([], []);

        // AdminEcole : sauvegarder les écoles sélectionnées, pas d'équipes spécifiques
        if (role == Roles.AdminEcole)
        {
            var ecolesAdminIds = ecoles.Where(e => e.AccesComplet).Select(e => e.Id).ToList();
            return (ecolesAdminIds, []);
        }

        var ecolesIds = ecoles.Where(e => e.AccesComplet).Select(e => e.Id).ToList();
        var equipesIds = ecoles
            .Where(e => !e.AccesComplet)
            .SelectMany(e => e.Equipes.Where(eq => eq.Selectionne).Select(eq => eq.Id))
            .ToList();

        return (ecolesIds, equipesIds);
    }
}
