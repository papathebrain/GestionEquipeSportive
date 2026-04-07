namespace GestionEquipeSportive.Models;

public class ThemeEcole
{
    public int Id { get; set; }
    public int EcoleId { get; set; }
    public string NomEquipe { get; set; } = string.Empty;
    public string CouleurPrimaire { get; set; } = "#1a3a5c";
    public string CouleurSecondaire { get; set; } = "#e8a020";
    public string? LogoPath { get; set; }
}
