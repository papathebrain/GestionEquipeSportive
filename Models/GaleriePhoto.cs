namespace GestionEquipeSportive.Models;

public class GaleriePhoto
{
    public int Id { get; set; }
    public int EquipeId { get; set; }
    public string CheminPhoto { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DateAjout { get; set; } = DateTime.Now;

    // Navigation
    public Equipe? Equipe { get; set; }
}
