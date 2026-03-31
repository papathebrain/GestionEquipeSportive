using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.ViewModels;

public class PublicEquipeViewModel
{
    public Equipe Equipe { get; set; } = null!;
    public Ecole Ecole { get; set; } = null!;

    public List<Staff> Staff { get; set; } = new();
    public List<Joueur> Joueurs { get; set; } = new();
    public List<Match> Matchs { get; set; } = new();
    public List<Evenement> Evenements { get; set; } = new();
    public StatistiquesMatchViewModel Stats { get; set; } = new();

    // Bannière : dernier match avec résultat OU prochain match dans les 7 jours
    public Match? DernierMatchAvecResultat { get; set; }
    public List<MatchMedia> DernierMatchMedias { get; set; } = new();
    public MatchMedia? PhotoBanniere { get; set; }  // Photo aléatoire pour la bannière
    public Match? ProchainMatch { get; set; }  // dans les 7 jours

    // Photos par match (matchId → liste de photos)
    public Dictionary<int, List<MatchMedia>> MatchesMedias { get; set; } = new();

    // Photos par joueur (joueurId → liste de photos)
    public Dictionary<int, List<JoueurMedia>> JoueurMedias { get; set; } = new();

    // Stats par joueur (joueurId → (matchsJoués, absences))
    public Dictionary<int, (int MatchsJoues, int Absences)> StatsJoueurs { get; set; } = new();

    // Id du dernier match avec photos (pour sélection auto galerie)
    public int? DernierMatchAvecPhotosId { get; set; }

    // Sport/Niveau display
    public string SportDisplay { get; set; } = "";
    public string NiveauDisplay { get; set; } = "";

    // Navigation inter-équipes / inter-écoles
    public List<(Equipe Equipe, string Url)> AutresEquipesEcole { get; set; } = new();
    public List<(Ecole Ecole, Equipe PremiereEquipe, string Url)> AutresEcoles { get; set; } = new();
}
