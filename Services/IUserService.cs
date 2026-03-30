using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.Services;

public interface IUserService
{
    ApplicationUser? Authentifier(string nomUtilisateur, string motDePasse);
    IEnumerable<ApplicationUser> GetAllUsers();
    ApplicationUser? GetById(string id);
    ApplicationUser? GetByNomUtilisateur(string nomUtilisateur);
    void Ajouter(ApplicationUser user, string motDePasse);
    void Modifier(ApplicationUser user);
    void ChangerMotDePasse(string userId, string nouveauMotDePasse);
    void Supprimer(string id);
    bool NomUtilisateurExiste(string nomUtilisateur, string? excludeId = null);
    bool EstVerrouille(string nomUtilisateur);
    void EnregistrerEchecConnexion(string nomUtilisateur);
    void ReinitialiserEchecs(string nomUtilisateur);
}
