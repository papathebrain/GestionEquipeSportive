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
    /// <summary>Écoles avec accès complet (toutes les équipes).</summary>
    public List<int> EcolesIds { get; set; } = [];
    /// <summary>Équipes spécifiques (si vide pour une école, accès à toutes les équipes de cette école).</summary>
    public List<int> EquipesIds { get; set; } = [];
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string AdminEcole = "AdminEcole";
    public const string Utilisateur = "Utilisateur";

    public const string AdminOuAdminEcole = Admin + "," + AdminEcole;
}
