using GestionEquipeSportive.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IEcoleService _ecoleService;

    public HomeController(IEcoleService ecoleService)
    {
        _ecoleService = ecoleService;
    }

    public IActionResult Index()
    {
        var ecoles = _ecoleService.GetAllEcoles();
        return View(ecoles);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
