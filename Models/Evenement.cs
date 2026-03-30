namespace GestionEquipeSportive.Models;

public enum TypeEvenement
{
    Pratique,
    ReunionParents,
    Tournoi,
    Jamboree,
    Evaluation,
    ActiviteEquipe,
    Autre
}

public class Evenement
{
    public int Id { get; set; }
    public int EquipeId { get; set; }
    public string Titre { get; set; } = string.Empty;
    public TypeEvenement Type { get; set; } = TypeEvenement.Pratique;
    public DateTime DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public string? Lieu { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Equipe? Equipe { get; set; }
}
