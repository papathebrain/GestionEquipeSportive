using GestionEquipeSportive.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IEcoleService _ecoleService;
    private readonly IEcoleAccessService _access;

    public HomeController(IEcoleService ecoleService, IEcoleAccessService access)
    {
        _ecoleService = ecoleService;
        _access = access;
    }

    public IActionResult Index()
    {
        var toutes = _ecoleService.GetAllEcoles();
        var visibles = _access.GetEcolesVisibles(User, toutes.Select(e => e.Id))
                              .ToHashSet();
        var ecoles = toutes.Where(e => visibles.Contains(e.Id)).ToList();

        // Redirection automatique si 1 seule école accessible
        if (ecoles.Count == 1)
            return RedirectToAction("Details", "Ecole", new { id = ecoles[0].Id });

        return View(ecoles);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
