namespace GestionEquipeSportive.Models;

public class Joueur
{
    public int Id { get; set; }
    public int EquipeId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;      // Groupe : Défenseur, Attaquant, Gardien...
    public string? PositionSpecifique { get; set; }           // Position précise : QB, WR, CB...
    public string? PhotoPath { get; set; }
    public string? NoFiche { get; set; }  // Identifiant permanent du joueur entre les années
    public string? Description { get; set; }
    public bool ConsentementPhoto { get; set; } = true;  // Consentement parental pour diffusion des photos
    public bool Actif { get; set; } = true;

    // Navigation
    public Equipe? Equipe { get; set; }
}

public class JoueurMedia
{
    public int Id { get; set; }
    public int JoueurId { get; set; }
    public string CheminFichier { get; set; } = "";
    public string? Description { get; set; }
    public DateTime DateAjout { get; set; } = DateTime.UtcNow;
}
