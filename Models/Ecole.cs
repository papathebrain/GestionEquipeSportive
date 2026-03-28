namespace GestionEquipeSportive.Models;

public class Ecole
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string CodeEcole { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string CouleurPrimaire { get; set; } = "#1a3a5c";
    public string CouleurSecondaire { get; set; } = "#e8a020";
}
