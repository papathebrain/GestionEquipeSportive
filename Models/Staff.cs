namespace GestionEquipeSportive.Models;

public class Staff
{
    public int Id { get; set; }
    public int EquipeId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;       // Ex: Entraîneur chef, Physio, ...
    public string? ResponsableDe { get; set; }              // Ex: Attaque, Défense, ...
    public string? PhotoPath { get; set; }

    // Navigation
    public Equipe? Equipe { get; set; }
}
