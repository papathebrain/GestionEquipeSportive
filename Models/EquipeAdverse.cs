namespace GestionEquipeSportive.Models;

public class EquipeAdverse
{
    public int Id { get; set; }
    public int EcoleId { get; set; }
    public string TypeSport { get; set; } = string.Empty;  // ex: "FootballAmericain"
    public string Nom { get; set; } = string.Empty;
    public string? Lieu { get; set; }
    public string? LogoPath { get; set; }
}
