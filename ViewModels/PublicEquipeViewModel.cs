using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.ViewModels;

public class PublicEquipeViewModel
{
    public Equipe Equipe { get; set; } = null!;
    public Ecole Ecole { get; set; } = null!;

    public List<Staff> Staff { get; set; } = new();
    public List<Joueur> Joueurs { get; set; } = new();
    public List<Match> Matchs { get; set; } = new();
    public StatistiquesMatchViewModel Stats { get; set; } = new();

    // Bannière : dernier match avec résultat OU prochain match dans les 7 jours
    public Match? DernierMatchAvecResultat { get; set; }
    public List<MatchMedia> DernierMatchMedias { get; set; } = new();
    public MatchMedia? PhotoBanniere { get; set; }  // Photo aléatoire pour la bannière
    public Match? ProchainMatch { get; set; }  // dans les 7 jours

    // Sport/Niveau display
    public string SportDisplay { get; set; } = "";
    public string NiveauDisplay { get; set; } = "";
}
