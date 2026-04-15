namespace GestionEquipeSportive.Models;

public class Staff
{
    public int Id { get; set; }
    public int EquipeId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;       // Ex: Entraîneur chef, Physio, ...
    public string? Description { get; set; }               // Bio / description (peut changer par année)
    public string? PhotoPath { get; set; }
    public string? NoFiche { get; set; }  // Clé externe GPI — lien avec les systèmes scolaires externes

    // Navigation
    public Equipe? Equipe { get; set; }
}
