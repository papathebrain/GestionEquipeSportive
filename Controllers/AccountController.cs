using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionEquipeSportive.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    // ─── Connexion ────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (_userService.EstVerrouille(model.NomUtilisateur))
        {
            ModelState.AddModelError("", "Compte temporairement verrouillé suite à trop de tentatives. Réessayez dans 15 minutes.");
            return View(model);
        }

        var user = _userService.Authentifier(model.NomUtilisateur, model.MotDePasse);
        if (user == null)
        {
            ModelState.AddModelError("", "Nom d'utilisateur ou mot de passe incorrect.");
            return View(model);
        }

        await SignInAsync(user, model.SeRappeler);

        if (user.ChangerMotDePasse)
        {
            TempData["Info"] = "Vous devez changer votre mot de passe avant de continuer.";
            return RedirectToAction(nameof(ChangerMotDePasse));
        }

        return RedirectToLocal(model.ReturnUrl);
    }

    // ─── Déconnexion ──────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // ─── Changement de mot de passe ───────────────────────────────────────────

    [HttpGet]
    [Authorize]
    public IActionResult ChangerMotDePasse()
        => View(new ChangerMotDePasseViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangerMotDePasse(ChangerMotDePasseViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = _userService.GetById(userId);
        if (user == null) return RedirectToAction(nameof(Login));

        // Vérifier le mot de passe actuel
        var verification = _userService.Authentifier(user.NomUtilisateur, model.MotDePasseActuel);
        if (verification == null)
        {
            ModelState.AddModelError(nameof(model.MotDePasseActuel), "Mot de passe actuel incorrect.");
            return View(model);
        }

        _userService.ChangerMotDePasse(userId, model.NouveauMotDePasse);

        // Re-signer avec les claims mis à jour
        var userMisAJour = _userService.GetById(userId)!;
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await SignInAsync(userMisAJour, false);

        TempData["Success"] = "Mot de passe modifié avec succès.";
        return RedirectToAction("Index", "Home");
    }

    // ─── Accès refusé ─────────────────────────────────────────────────────────

    public IActionResult AccesDenie()
        => View();

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task SignInAsync(ApplicationUser user, bool persistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.NomUtilisateur),
            new(ClaimTypes.GivenName, user.NomComplet),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var props = new AuthenticationProperties
        {
            IsPersistent = persistent,
            ExpiresUtc = persistent
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8),
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}
