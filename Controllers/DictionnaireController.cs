using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

[Authorize(Roles = Roles.Admin)]
public class DictionnaireController : Controller
{
    private readonly IDictionnaireService _dictionnaireService;
    private readonly IJoueurService _joueurService;
    private readonly IStaffService _staffService;
    private readonly IEquipeService _equipeService;

    public DictionnaireController(IDictionnaireService dictionnaireService,
        IJoueurService joueurService, IStaffService staffService, IEquipeService equipeService)
    {
        _dictionnaireService = dictionnaireService;
        _joueurService = joueurService;
        _staffService = staffService;
        _equipeService = equipeService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.Entrees = _dictionnaireService.GetAll();
        ViewBag.Sports  = _dictionnaireService.GetAll()
            .Where(e => e.Categorie == "Sport")
            .OrderBy(e => e.Ordre).ThenBy(e => e.Valeur)
            .ToList();
        // Sports actifs pour les onglets (utilise aussi les defaults si vide)
        ViewBag.SportsActifs = _dictionnaireService.GetSports();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Ajouter(string categorie, string sport, string valeur,
        string label = "", string acronyme = "", string description = "", string parentValeur = "")
    {
        if (!string.IsNullOrWhiteSpace(valeur))
        {
            var cle = valeur.Trim();

            // Vérifier doublon de clé interne pour les sports
            if (categorie == "Sport" && _dictionnaireService.GetAll()
                    .Any(e => e.Categorie == "Sport" &&
                              string.Equals(e.Valeur, cle, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["Error"] = $"Un sport avec la clé « {cle} » existe déjà.";
                return RedirectToAction(nameof(Index));
            }

            _dictionnaireService.Add(categorie, sport ?? string.Empty,
                cle, label.Trim(), acronyme.Trim(), description.Trim(), parentValeur.Trim());
            TempData["Success"] = "Entrée ajoutée.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Supprimer(int id)
    {
        var entree = _dictionnaireService.GetAll().FirstOrDefault(e => e.Id == id);
        if (entree != null)
        {
            string? erreur = null;
            var valeur = entree.Valeur;

            if (entree.Categorie == "Position" &&
                _joueurService.GetAllJoueurEquipes().Any(je => je.Position == valeur || je.PositionPairs.Any(p => p.Pos == valeur)))
                erreur = $"Impossible de supprimer « {valeur} » : utilisé par au moins un joueur (Position).";

            else if (entree.Categorie == "PositionSpecifique" &&
                _joueurService.GetAllJoueurEquipes().Any(je => je.PositionPairs.Any(p => p.Spec == valeur)))
                erreur = $"Impossible de supprimer « {valeur} » : utilisé par au moins un joueur (Position spécifique).";

            else if (entree.Categorie == "TitreStaff" &&
                _equipeService.GetAllEquipes().SelectMany(e => _staffService.GetStaffByEquipe(e.Id)).Any(s => s.Titre == valeur))
                erreur = $"Impossible de supprimer « {valeur} » : utilisé par au moins un membre du staff (Titre).";

            else if (entree.Categorie == "Niveau" &&
                _equipeService.GetAllEquipes().Any(e => e.Niveau.ToString() == valeur && e.TypeSport.ToString() == entree.Sport))
                erreur = $"Impossible de supprimer « {valeur} » : utilisé par au moins une équipe (Niveau).";

            else if (entree.Categorie == "Sport" &&
                _equipeService.GetAllEquipes().Any(e => e.TypeSport.ToString() == valeur))
                erreur = $"Impossible de supprimer « {valeur} » : utilisé par au moins une équipe (Sport).";

            // Vérifier que la Position parente n'a pas des PositionSpecifique enfants
            else if (entree.Categorie == "Position" &&
                _dictionnaireService.GetAll().Any(e => e.Categorie == "PositionSpecifique" && e.Sport == entree.Sport && e.ParentValeur == valeur))
                erreur = $"Impossible de supprimer « {valeur} » : des positions spécifiques lui sont associées.";

            if (erreur != null)
            {
                TempData["Error"] = erreur;
                return RedirectToAction(nameof(Index));
            }
        }

        _dictionnaireService.Delete(id);
        TempData["Success"] = "Entrée supprimée.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Modifier(int id)
    {
        var entree = _dictionnaireService.GetAll().FirstOrDefault(e => e.Id == id);
        if (entree == null) return NotFound();
        return Json(new {
            entree.Id, entree.Valeur, entree.Label, entree.Acronyme,
            entree.Description, entree.ParentValeur, entree.Actif, entree.Categorie
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modifier(int id, string valeur, string label = "", string acronyme = "",
        string description = "", string parentValeur = "")
    {
        if (!string.IsNullOrWhiteSpace(valeur))
        {
            _dictionnaireService.Update(id, valeur.Trim(), label.Trim(), acronyme.Trim(), description.Trim(), parentValeur.Trim());
            TempData["Success"] = "Entrée modifiée.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActif(int id)
    {
        _dictionnaireService.ToggleActif(id);
        TempData["Success"] = "Statut du sport modifié.";
        return RedirectToAction(nameof(Index));
    }
}
