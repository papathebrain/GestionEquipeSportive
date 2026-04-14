namespace GestionEquipeSportive.Models;

public class Match
{
    public int Id { get; set; }
    public int EquipeId { get; set; }
    public DateTime DateMatch { get; set; }
    public string? HeureArriveeVestiaire { get; set; }  // "HH:mm"
    public string? HeureDepartAutobus { get; set; }     // null si domicile
    public string? HeureDebutMatch { get; set; }        // "HH:mm"
    public bool EstDomicile { get; set; }
    public int? AdversaireId { get; set; }   // FK vers EquipeAdverse (optionnel)
    public string Adversaire { get; set; } = "";
    public string? Lieu { get; set; }
    public int? ScoreEquipe { get; set; }
    public int? ScoreAdversaire { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Equipe? Equipe { get; set; }

    public bool AResultat => ScoreEquipe.HasValue && ScoreAdversaire.HasValue;

    public string Resultat => AResultat
        ? ScoreEquipe > ScoreAdversaire ? "V"
        : ScoreEquipe < ScoreAdversaire ? "D"
        : "N"
        : "";
}

public enum TypeMedia { Photo, Video }

public class MatchMedia
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public string CheminFichier { get; set; } = "";
    public TypeMedia TypeMedia { get; set; }
    public string? Description { get; set; }
    public DateTime DateAjout { get; set; } = DateTime.UtcNow;
}

public class AbsenceMatch
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int JoueurId { get; set; }
    public string? Raison { get; set; }
}
