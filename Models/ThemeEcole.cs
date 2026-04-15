namespace GestionEquipeSportive.Models;

public class ThemeEcole
{
    public int Id { get; set; }
    public int EcoleId { get; set; }
    public string NomEquipe { get; set; } = string.Empty;
    public string CouleurPrimaire { get; set; } = "#1a3a5c";
    public string CouleurSecondaire { get; set; } = "#e8a020";
    public string? LogoPath { get; set; }
    public string? MusiqueProchainMatchPath { get; set; }
    public int? MusiqueProchainMatchDebut { get; set; }
    public int? MusiqueProchainMatchDuree { get; set; }

    public string? MusiqueVictoirePath { get; set; }
    public int? MusiqueVictoireDebut { get; set; }
    public int? MusiqueVictoireDuree { get; set; }

    public string? MusiqueDefaitePath { get; set; }
    public int? MusiqueDefaiteDebut { get; set; }
    public int? MusiqueDefaiteDuree { get; set; }
}
