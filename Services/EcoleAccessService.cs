using GestionEquipeSportive.Models;
using System.Security.Claims;

namespace GestionEquipeSportive.Services;

public class EcoleAccessService : IEcoleAccessService
{
    private readonly IUserService _userService;
    private readonly IEquipeService _equipeService;

    public EcoleAccessService(IUserService userService, IEquipeService equipeService)
    {
        _userService = userService;
        _equipeService = equipeService;
    }

    /// <summary>Accès complet à l'école : Admin ou utilisateur avec l'école dans EcolesIds.</summary>
    public bool PeutModifier(ClaimsPrincipal user, int ecoleId)
    {
        if (user.IsInRole(Roles.Admin)) return true;
        var appUser = GetAppUser(user);
        return appUser != null && appUser.EcolesIds.Contains(ecoleId);
    }

    /// <summary>
    /// Peut modifier une équipe spécifique :
    /// - Admin → toujours
    /// - Accès école complet → toutes les équipes de l'école
    /// - Accès équipe spécifique → seulement cette équipe
    /// </summary>
    public bool PeutModifierEquipe(ClaimsPrincipal user, int equipeId, int ecoleId)
    {
        if (user.IsInRole(Roles.Admin)) return true;
        var appUser = GetAppUser(user);
        if (appUser == null) return false;
        return appUser.EcolesIds.Contains(ecoleId)
            || appUser.EquipesIds.Contains(equipeId);
    }

    /// <summary>
    /// Filtre la liste des équipes selon les droits :
    /// - Admin ou accès école complet → toutes les équipes
    /// - Accès équipes spécifiques → seulement celles assignées
    /// </summary>
    public IEnumerable<Equipe> FiltrerEquipes(ClaimsPrincipal user, IEnumerable<Equipe> equipes, int ecoleId)
    {
        if (user.IsInRole(Roles.Admin)) return equipes;
        var appUser = GetAppUser(user);
        if (appUser == null) return [];

        // Accès complet à l'école → toutes les équipes
        if (appUser.EcolesIds.Contains(ecoleId)) return equipes;

        // Accès spécifique → filtrer
        return equipes.Where(e => appUser.EquipesIds.Contains(e.Id));
    }

    /// <summary>Retourne les IDs des écoles que l'utilisateur peut voir (école directe ou via équipe).</summary>
    public IEnumerable<int> GetEcolesVisibles(ClaimsPrincipal user, IEnumerable<int> tousLesEcolesIds)
    {
        if (user.IsInRole(Roles.Admin)) return tousLesEcolesIds;
        var appUser = GetAppUser(user);
        if (appUser == null) return [];

        var ecoles = new HashSet<int>(appUser.EcolesIds);

        // Ajouter les écoles des équipes spécifiques
        foreach (var equipeId in appUser.EquipesIds)
        {
            var equipe = _equipeService.GetEquipeById(equipeId);
            if (equipe != null) ecoles.Add(equipe.EcoleId);
        }

        return tousLesEcolesIds.Where(ecoles.Contains);
    }

    private ApplicationUser? GetAppUser(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return null;
        return _userService.GetById(userId);
    }
}
