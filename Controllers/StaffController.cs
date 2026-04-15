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
    private readonly IDictionnaireService _dictionnaireService;

    public StaffController(IStaffService staffService, IEquipeService equipeService,
        IEcoleService ecoleService, IEcoleAccessService access, IWebHostEnvironment env,
        IDictionnaireService dictionnaireService)
    {
        _staffService = staffService;
        _equipeService = equipeService;
        _ecoleService = ecoleService;
        _access = access;
        _env = env;
        _dictionnaireService = dictionnaireService;
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

        var sport = equipe.TypeSport.ToString();
        return View(new StaffViewModel
        {
            EquipeId = equipeId,
            EcoleId = equipe.EcoleId,
            NomEquipe = equipe.Nom,
            TitresDisponibles = _dictionnaireService.GetTitresStaff(sport)
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
            var sport0 = equipe.TypeSport.ToString();
            vm.TitresDisponibles = _dictionnaireService.GetTitresStaff(sport0);
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
        var sportEdit = equipe.TypeSport.ToString();
        vm.TitresDisponibles = _dictionnaireService.GetTitresStaff(sportEdit);

        // Historique : toutes les présences de ce membre (même NoFiche)
        if (!string.IsNullOrEmpty(staff.NoFiche))
        {
            var historique = _staffService.GetStaffByNoFiche(staff.NoFiche)
                .Select(s => new StaffHistoriqueEntry
                {
                    StaffEntry = s,
                    Equipe = _equipeService.GetEquipeById(s.EquipeId)!
                })
                .Where(x => x.Equipe != null)
                .OrderByDescending(x => x.Equipe.AnneeScolaire)
                .ToList();
            ViewBag.Historique = historique;
        }

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
            var sportPost = equipe.TypeSport.ToString();
            vm.TitresDisponibles = _dictionnaireService.GetTitresStaff(sportPost);
            return View(vm);
        }

        var staff = _staffService.UpdateStaff(vm, vm.PhotoFile, _env.WebRootPath);
        TempData["Success"] = $"{staff.Prenom} {staff.Nom} modifié(e).";
        return RedirectToAction(nameof(Index), new { equipeId = vm.EquipeId });
    }

    // ─── Importer ─────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Importer(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipeId, equipe.EcoleId)) return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        // Toutes les autres équipes de la même école (autres années ou sports)
        var autresEquipes = _equipeService.GetEquipesByEcole(equipe.EcoleId)
            .Where(e => e.Id != equipeId)
            .OrderByDescending(e => e.AnneeScolaire)
            .ThenBy(e => e.Nom)
            .ToList();

        ViewBag.Equipe = equipe;
        ViewBag.Ecole = ecole;
        ViewBag.AutresEquipes = autresEquipes;
        return View();
    }

    [HttpGet]
    public IActionResult StaffSourceEquipe(int sourceEquipeId)
    {
        var staff = _staffService.GetStaffByEquipe(sourceEquipeId)
            .OrderBy(s => s.Titre).ThenBy(s => s.Nom).ToList();
        return Json(staff.Select(s => new {
            s.Id, s.Nom, s.Prenom, s.Titre, s.PhotoPath, s.NoFiche
        }));
    }

    [HttpGet]
    public IActionResult RechercherStaff(string q, int equipeId)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Json(new { staff = Array.Empty<object>(), message = "Saisissez au moins 2 caractères." });

        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return Json(new { staff = Array.Empty<object>() });

        var equipesDeLEcole = _equipeService.GetEquipesByEcole(equipe.EcoleId);
        var equipeIds = equipesDeLEcole.Select(e => e.Id).ToHashSet();

        // Ids déjà présents dans cette équipe (pour marquer les doublons)
        var idsDejaDansEquipe = _staffService.GetStaffByEquipe(equipeId)
            .Select(s => s.Id)
            .ToHashSet();

        var terme = q.Trim();

        // Recherche dans tout le staff de l'école
        var tous = _staffService.GetAllStaff()
            .Where(s => equipeIds.Contains(s.EquipeId))
            .Where(s =>
                s.Nom.Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                s.Prenom.Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                (s.Prenom + " " + s.Nom).Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                (s.Nom + " " + s.Prenom).Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                s.Titre.Contains(terme, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.Nom).ThenBy(s => s.Prenom)
            .Take(40)
            .ToList();

        var equipeNoms = equipesDeLEcole.ToDictionary(e => e.Id, e => e.Nom);

        var result = tous.Select(s => new
        {
            id = s.Id,
            nom = s.Nom,
            prenom = s.Prenom,
            titre = s.Titre,
            photoPath = s.PhotoPath ?? "",
            equipeActuelle = equipeNoms.TryGetValue(s.EquipeId, out var en) ? en : "",
            dejaPresent = idsDejaDansEquipe.Contains(s.Id)
        }).ToList();

        return Json(new { staff = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjouterExistantStaff(string staffSourceIds, int equipeId, string? returnUrl = null)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipeId, equipe?.EcoleId ?? 0)) return Forbid();

        var ids = (staffSourceIds ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var i) ? i : 0)
            .Where(i => i > 0)
            .ToList();

        if (ids.Count == 0)
        {
            TempData["Error"] = "Aucun employé sélectionné.";
        }
        else
        {
            _staffService.CopierVersEquipe(ids, equipeId);
            TempData["Success"] = ids.Count == 1
                ? "1 employé importé dans l'équipe."
                : $"{ids.Count} employés importés dans l'équipe.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "staff" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Importer(int equipeId, List<int> staffIds)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipeId, equipe.EcoleId)) return Forbid();

        if (staffIds != null && staffIds.Count > 0)
        {
            _staffService.CopierVersEquipe(staffIds, equipeId);
            TempData["Success"] = $"{staffIds.Count} membre(s) du staff importé(s).";
        }
        return RedirectToAction(nameof(Index), new { equipeId });
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
