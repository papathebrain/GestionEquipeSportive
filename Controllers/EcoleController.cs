using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class EcoleController : Controller
{
    private readonly IEcoleService _ecoleService;
    private readonly IEquipeService _equipeService;
    private readonly IEcoleAccessService _access;
    private readonly IWebHostEnvironment _env;
    private readonly IJoueurService _joueurService;
    private readonly IStaffService _staffService;
    private readonly IDictionnaireService _dictionnaireService;

    public EcoleController(IEcoleService ecoleService, IEquipeService equipeService,
        IEcoleAccessService access, IWebHostEnvironment env, IJoueurService joueurService,
        IStaffService staffService, IDictionnaireService dictionnaireService)
    {
        _ecoleService = ecoleService;
        _equipeService = equipeService;
        _access = access;
        _env = env;
        _joueurService = joueurService;
        _staffService = staffService;
        _dictionnaireService = dictionnaireService;
    }

    public IActionResult Index()
    {
        var toutes = _ecoleService.GetAllEcoles();
        var visibles = _access.GetEcolesVisibles(User, toutes.Select(e => e.Id)).ToHashSet();
        var ecoles = toutes.Where(e => visibles.Contains(e.Id)).ToList();

        // Redirection automatique si 1 seule école accessible → liste des équipes directement
        if (ecoles.Count == 1)
            return RedirectToAction("Index", "Equipe", new { ecoleId = ecoles[0].Id });

        return View(ecoles);
    }

    public IActionResult Details(int id)
    {
        var ecole = _ecoleService.GetEcoleById(id);
        if (ecole == null) return NotFound();

        // Vérifier que l'utilisateur a accès à cette école
        var visibles = _access.GetEcolesVisibles(User, [id]).ToList();
        if (!visibles.Contains(id)) return Forbid();

        SetTheme(ecole);

        var toutesEquipes = _equipeService.GetEquipesByEcole(id);
        var equipes = _access.FiltrerEquipes(User, toutesEquipes, id).ToList();

        var annees = equipes.Select(e => e.AnneeScolaire)
            .Where(a => !string.IsNullOrEmpty(a))
            .Distinct().OrderByDescending(a => a).ToList();

        var nbJoueurs = equipes.ToDictionary(
            e => e.Id,
            e => _joueurService.GetJoueurEquipesByEquipe(e.Id).Count);

        // Année scolaire courante (1 juillet → 30 juin)
        var aujourd = DateTime.Today;
        var anneeCourante = aujourd.Month >= 7
            ? $"{aujourd.Year}-{aujourd.Year + 1}"
            : $"{aujourd.Year - 1}-{aujourd.Year}";

        ViewBag.Equipes = equipes;
        ViewBag.Annees = annees;
        ViewBag.NbJoueurs = nbJoueurs;
        ViewBag.AnneeCourante = anneeCourante;
        ViewBag.PeutModifier = _access.PeutModifier(User, id);
        return View(ecole);
    }

    // ─── Actions réservées aux admins ─────────────────────────────────────────

    [Authorize(Roles = Roles.Admin)]
    public IActionResult Create()
        => View("Edit", new EcoleViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Create(EcoleViewModel vm)
    {
        if (!ModelState.IsValid) return View("Edit", vm);
        var ecole = _ecoleService.CreateEcole(vm, vm.LogoFile, _env.WebRootPath);
        TempData["Success"] = "École créée avec succès.";
        return RedirectToAction(nameof(Edit), new { id = ecole.Id });
    }

    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult Edit(int id)
    {
        var ecole = _ecoleService.GetEcoleById(id);
        if (ecole == null) return NotFound();
        if (!_access.PeutModifier(User, id)) return Forbid();
        ViewBag.Themes = _ecoleService.GetThemesByEcole(id);
        ViewBag.EquipesAdverses = _ecoleService.GetEquipesAdversesByEcole(id);
        ViewBag.AnneesScolaires = _ecoleService.GetAnneesScolairesByEcole(id);
        ViewBag.SportsDict = _dictionnaireService.GetSports();

        var equipes = _equipeService.GetEquipesByEcole(id);
        var tousJoueurs = _joueurService.GetJoueursByEcole(id)
            .OrderBy(j => j.Nom).ThenBy(j => j.Prenom)
            .ToList();
        var tousJE = equipes
            .SelectMany(e => {
                var jes = _joueurService.GetJoueurEquipesByEquipe(e.Id);
                foreach (var je in jes) je.Equipe = e;
                return jes;
            }).ToList();
        var jeParJoueur = tousJE
            .GroupBy(je => je.JoueurId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var premierePhotoParJoueur = tousJoueurs
            .ToDictionary(
                j => j.Id,
                j => _joueurService.GetMediasByJoueur(j.Id).FirstOrDefault()?.CheminFichier);
        ViewBag.Joueurs = tousJoueurs;
        ViewBag.JoueurEquipesParJoueur = jeParJoueur;
        ViewBag.PremierePhotoParJoueur = premierePhotoParJoueur;
        ViewBag.EquipesEcole = equipes.OrderByDescending(e => e.AnneeScolaire).ThenBy(e => e.Nom).ToList();

        // Employés : tous les staff des équipes de l'école (chaque record est unique par Id)
        var tousStaff = equipes
            .SelectMany(e => _staffService.GetStaffByEquipe(e.Id)
                .Select(s => { s.Equipe = e; return s; }))
            .OrderBy(s => s.Nom).ThenBy(s => s.Prenom)
            .ToList();
        var equipeParId = tousStaff
            .ToDictionary(s => s.Id.ToString(), s => new List<Equipe?> { s.Equipe });
        ViewBag.Employes = tousStaff;
        ViewBag.EquipesParEmploye = equipeParId;

        return View(_ecoleService.ToViewModel(ecole));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult Edit(int id, EcoleViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!_access.PeutModifier(User, id)) return Forbid();
        if (!ModelState.IsValid)
        {
            ViewBag.Themes = _ecoleService.GetThemesByEcole(id);
            ViewBag.EquipesAdverses = _ecoleService.GetEquipesAdversesByEcole(id);
            ViewBag.AnneesScolaires = _ecoleService.GetAnneesScolairesByEcole(id);
            ViewBag.SportsDict = _dictionnaireService.GetSports();
            return View(vm);
        }
        _ecoleService.UpdateEcole(vm, vm.LogoFile, _env.WebRootPath);
        TempData["Success"] = "École modifiée avec succès.";
        if (!User.IsInRole(Roles.Admin))
            return RedirectToAction(nameof(Edit), new { id = vm.Id });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Delete(int id)
    {
        _ecoleService.DeleteEcole(id);
        TempData["Success"] = "École supprimée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    // ── Actions Années scolaires ─────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult AjouterAnnee(int ecoleId, string annee)
    {
        if (!_access.PeutModifier(User, ecoleId)) return Forbid();
        if (!string.IsNullOrWhiteSpace(annee))
        {
            var existantes = _ecoleService.GetAnneesScolairesByEcole(ecoleId);
            if (!existantes.Any(a => a.AnneeScolaire == annee.Trim()))
            {
                _ecoleService.AddAnneeScolaire(ecoleId, annee);
                TempData["Success"] = "Année scolaire ajoutée.";
            }
            else
            {
                TempData["Error"] = "Cette année scolaire existe déjà.";
            }
        }
        return RedirectToAction(nameof(Edit), new { id = ecoleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult SupprimerAnnee(int id, int ecoleId)
    {
        if (!_access.PeutModifier(User, ecoleId)) return Forbid();
        _ecoleService.DeleteAnneeScolaire(id);
        TempData["Success"] = "Année scolaire supprimée.";
        return RedirectToAction(nameof(Edit), new { id = ecoleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult CreerAnneeAvecCopie(int ecoleId, string annee, string? equipeSourceIds)
    {
        if (!_access.PeutModifier(User, ecoleId)) return Forbid();
        if (string.IsNullOrWhiteSpace(annee))
        {
            TempData["Error"] = "L'année scolaire est requise.";
            return RedirectToAction(nameof(Edit), new { id = ecoleId, tab = "annees" });
        }
        annee = annee.Trim();
        var existantes = _ecoleService.GetAnneesScolairesByEcole(ecoleId);
        if (!existantes.Any(a => a.AnneeScolaire == annee))
            _ecoleService.AddAnneeScolaire(ecoleId, annee);

        // Copier les équipes sélectionnées vers la nouvelle année
        var ids = (equipeSourceIds ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var i) ? i : 0)
            .Where(i => i > 0).ToList();

        int nbEquipes = 0;
        int nbJoueurs = 0;
        foreach (var sourceId in ids)
        {
            var sourceEquipe = _equipeService.GetEquipeById(sourceId);
            if (sourceEquipe == null || sourceEquipe.EcoleId != ecoleId) continue;

            var vm = _equipeService.ToViewModel(sourceEquipe);
            vm.Id = 0;
            vm.AnneeScolaire = annee;
            var nouvelleEquipe = _equipeService.CreateEquipe(vm);
            nbEquipes++;

            // Copier les joueurs
            var joueurEquipes = _joueurService.GetJoueurEquipesByEquipe(sourceId);
            foreach (var je in joueurEquipes)
            {
                var dejaPresent = _joueurService.GetJoueurEquipesByEquipe(nouvelleEquipe.Id).Any(x => x.JoueurId == je.JoueurId);
                if (dejaPresent) continue;
                _joueurService.AssignerAEquipe(new ViewModels.JoueurEquipeViewModel
                {
                    JoueurId = je.JoueurId,
                    EquipeId = nouvelleEquipe.Id,
                    Actif = true
                }, null, _env.WebRootPath);
                nbJoueurs++;
            }
        }

        TempData["Success"] = nbEquipes == 0
            ? $"Année scolaire {annee} créée."
            : $"Année {annee} créée avec {nbEquipes} équipe(s) et {nbJoueurs} joueur(s) copié(s).";
        return RedirectToAction(nameof(Edit), new { id = ecoleId, tab = "annees" });
    }

    // ── Actions Équipes adverses ─────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult AjouterAdverse(EquipeAdverseViewModel vm)
    {
        if (!_access.PeutModifier(User, vm.EcoleId)) return Forbid();
        if (ModelState.IsValid)
        {
            _ecoleService.CreateEquipeAdverse(vm, vm.LogoFile, _env.WebRootPath);
            TempData["Success"] = "Équipe adverse ajoutée.";
        }
        else
        {
            TempData["Error"] = "Données invalides.";
        }
        return RedirectToAction(nameof(Edit), new { id = vm.EcoleId, tab = "adverses" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult ModifierAdverse(EquipeAdverseViewModel vm)
    {
        if (!_access.PeutModifier(User, vm.EcoleId)) return Forbid();
        if (ModelState.IsValid)
        {
            _ecoleService.UpdateEquipeAdverse(vm, vm.LogoFile, _env.WebRootPath);
            TempData["Success"] = "Équipe adverse modifiée.";
        }
        else
        {
            TempData["Error"] = "Données invalides.";
        }
        return RedirectToAction(nameof(Edit), new { id = vm.EcoleId, tab = "adverses" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult SupprimerAdverse(int id, int ecoleId)
    {
        if (!_access.PeutModifier(User, ecoleId)) return Forbid();
        _ecoleService.DeleteEquipeAdverse(id, _env.WebRootPath);
        TempData["Success"] = "Équipe adverse supprimée.";
        return RedirectToAction(nameof(Edit), new { id = ecoleId });
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult GetEquipesPourAnnee(int ecoleId, string annee)
    {
        if (!_access.PeutModifier(User, ecoleId)) return Forbid();
        var equipes = _equipeService.GetEquipesByEcole(ecoleId)
            .Where(e => e.AnneeScolaire == annee)
            .OrderBy(e => e.TypeSport.ToString()).ThenBy(e => e.Nom)
            .Select(e => new { e.Id, e.Nom, sport = e.TypeSport.ToString() });
        return Json(equipes);
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult GetAdverse(int id)
    {
        var a = _ecoleService.GetEquipeAdverseById(id);
        if (a == null) return NotFound();
        if (!_access.PeutModifier(User, a.EcoleId)) return Forbid();
        return Json(new { a.Id, a.EcoleId, a.TypeSport, a.Nom, a.Lieu, a.LogoPath });
    }

    [HttpGet]
    public IActionResult GetAdversesParSport(int ecoleId, string sport)
    {
        var adverses = _ecoleService.GetEquipesAdversesByEcoleSport(ecoleId, sport)
            .OrderBy(a => a.Nom)
            .Select(a => new { a.Id, a.Nom, a.Lieu, a.LogoPath });
        return Json(adverses);
    }

    // ── Actions Thèmes ────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult AjouterTheme(ThemeEcoleViewModel vm)
    {
        if (!_access.PeutModifier(User, vm.EcoleId)) return Forbid();
        if (ModelState.IsValid)
        {
            _ecoleService.CreateTheme(vm, vm.LogoFile, _env.WebRootPath);
            TempData["Success"] = "Thème ajouté avec succès.";
        }
        else
        {
            TempData["Error"] = "Données invalides.";
        }
        return RedirectToAction(nameof(Edit), new { id = vm.EcoleId, tab = "themes" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult ModifierTheme(ThemeEcoleViewModel vm)
    {
        if (!_access.PeutModifier(User, vm.EcoleId)) return Forbid();
        if (ModelState.IsValid)
        {
            _ecoleService.UpdateTheme(vm, vm.LogoFile, _env.WebRootPath);
            TempData["Success"] = "Thème modifié avec succès.";
        }
        else
        {
            TempData["Error"] = "Données invalides.";
        }
        return RedirectToAction(nameof(Edit), new { id = vm.EcoleId, tab = "themes" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult SupprimerTheme(int id, int ecoleId)
    {
        if (!_access.PeutModifier(User, ecoleId)) return Forbid();
        _ecoleService.DeleteTheme(id, _env.WebRootPath);
        TempData["Success"] = "Thème supprimé avec succès.";
        return RedirectToAction(nameof(Edit), new { id = ecoleId, tab = "themes" });
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOuAdminEcole)]
    public IActionResult GetTheme(int id)
    {
        var theme = _ecoleService.GetThemeById(id);
        if (theme == null) return NotFound();
        if (!_access.PeutModifier(User, theme.EcoleId)) return Forbid();
        return Json(new {
            theme.Id, theme.EcoleId, theme.NomEquipe,
            theme.CouleurPrimaire, theme.CouleurSecondaire, theme.LogoPath,
            theme.MusiqueProchainMatchPath, theme.MusiqueProchainMatchDebut, theme.MusiqueProchainMatchDuree,
            theme.MusiqueVictoirePath, theme.MusiqueVictoireDebut, theme.MusiqueVictoireDuree,
            theme.MusiqueDefaitePath, theme.MusiqueDefaiteDebut, theme.MusiqueDefaiteDuree
        });
    }

    private void SetTheme(Ecole ecole)
    {
        ViewBag.CouleurPrimaire = ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = ecole.CouleurSecondaire;
    }
}
