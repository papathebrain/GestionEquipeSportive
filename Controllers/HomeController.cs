using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IEcoleService _ecoleService;
    private readonly IEquipeService _equipeService;
    private readonly IEcoleAccessService _access;

    public HomeController(IEcoleService ecoleService, IEquipeService equipeService, IEcoleAccessService access)
    {
        _ecoleService = ecoleService;
        _equipeService = equipeService;
        _access = access;
    }

    public IActionResult Index()
    {
        var toutes = _ecoleService.GetAllEcoles();
        var visibles = _access.GetEcolesVisibles(User, toutes.Select(e => e.Id))
                              .ToHashSet();
        var ecoles = toutes.Where(e => visibles.Contains(e.Id)).ToList();

        // Redirection automatique si 1 seule école accessible (pas pour le super admin)
        if (ecoles.Count == 1 && !User.IsInRole(Roles.Admin))
        {
            if (User.IsInRole(Roles.AdminEcole))
                return RedirectToAction("Index", "Equipe", new { ecoleId = ecoles[0].Id });
            return RedirectToAction("Details", "Ecole", new { id = ecoles[0].Id });
        }

        // Année scolaire courante : commence le 1er juillet, se termine le 30 juin
        var aujourd = DateTime.Today;
        var anneeScolaire = aujourd.Month >= 7
            ? $"{aujourd.Year}-{aujourd.Year + 1}"
            : $"{aujourd.Year - 1}-{aujourd.Year}";

        // Nb d'équipes par école pour l'année courante
        var nbEquipes = ecoles.ToDictionary(
            e => e.Id,
            e => _equipeService.GetEquipesByEcole(e.Id)
                               .Count(eq => eq.AnneeScolaire == anneeScolaire));

        ViewBag.AnneeScolaire = anneeScolaire;
        ViewBag.NbEquipes = nbEquipes;

        return View(ecoles);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
