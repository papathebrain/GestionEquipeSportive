namespace GestionEquipeSportive.Models;

public class Joueur
{
    public int Id { get; set; }
    public int EquipeId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }

    // Navigation
    public Equipe? Equipe { get; set; }
}
