using GestionEquipeSportive.Models;
using System.Security.Claims;

namespace GestionEquipeSportive.Services;

public interface IEcoleAccessService
{
    /// <summary>Accès complet à l'école (créer des équipes, modifier l'école). Admin ou accès école complet.</summary>
    bool PeutModifier(ClaimsPrincipal user, int ecoleId);

    /// <summary>Peut voir/modifier une équipe spécifique.</summary>
    bool PeutModifierEquipe(ClaimsPrincipal user, int equipeId, int ecoleId);

    /// <summary>Filtre la liste des équipes selon les droits de l'utilisateur.</summary>
    IEnumerable<Equipe> FiltrerEquipes(ClaimsPrincipal user, IEnumerable<Equipe> equipes, int ecoleId);

    /// <summary>Retourne les IDs des écoles visibles par l'utilisateur (via école ou équipe assignée).</summary>
    IEnumerable<int> GetEcolesVisibles(ClaimsPrincipal user, IEnumerable<int> tousLesEcolesIds);
}
