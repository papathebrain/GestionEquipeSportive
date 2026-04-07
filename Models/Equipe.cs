namespace GestionEquipeSportive.Models;

public enum TypeSport
{
    FootballAmericain,
    FlagFootball,
    Soccer,
    Volleyball,
    Hockey
}

public enum NiveauEquipe
{
    // Football américain & Soccer
    Benjamin,
    Cadet,
    Juvenil,
    // Hockey
    Atome,
    PeeWee,
    Bantam
}

public class Equipe
{
    public int Id { get; set; }
    public int EcoleId { get; set; }
    public string AnneeScolaire { get; set; } = string.Empty;
    public TypeSport TypeSport { get; set; }
    public NiveauEquipe Niveau { get; set; }
    public string Nom { get; set; } = string.Empty;
    public bool AfficherPublic { get; set; } = false;
    public int? ThemeId { get; set; }

    // Navigation
    public Ecole? Ecole { get; set; }
    public ThemeEcole? Theme { get; set; }
}
