using System.Security.Claims;

namespace GestionEquipeSportive.Services;

public interface IEcoleAccessService
{
    /// <summary>
    /// Retourne true si l'utilisateur peut modifier le contenu de l'école (Admin ou assigné).
    /// </summary>
    bool PeutModifier(ClaimsPrincipal user, int ecoleId);
}
