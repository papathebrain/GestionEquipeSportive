namespace GestionEquipeSportive.Models;

public class ApplicationUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string NomUtilisateur { get; set; } = "";
    public string NomComplet { get; set; } = "";
    public string MotDePasseHash { get; set; } = "";
    public string Role { get; set; } = Roles.Utilisateur;
    public bool EstActif { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DerniereConnexion { get; set; }
    public bool ChangerMotDePasse { get; set; } = false;
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Utilisateur = "Utilisateur";
}
