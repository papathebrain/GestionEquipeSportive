using GestionEquipeSportive.Models;
using System.Security.Claims;

namespace GestionEquipeSportive.Services;

public class EcoleAccessService : IEcoleAccessService
{
    private readonly IUserService _userService;

    public EcoleAccessService(IUserService userService)
    {
        _userService = userService;
    }

    public bool PeutModifier(ClaimsPrincipal user, int ecoleId)
    {
        if (user.IsInRole(Roles.Admin)) return true;

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return false;

        var appUser = _userService.GetById(userId);
        return appUser != null && appUser.EcolesIds.Contains(ecoleId);
    }
}
