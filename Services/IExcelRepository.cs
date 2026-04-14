using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.Services;

public interface IExcelRepository
{
    // Écoles
    List<Ecole> GetAllEcoles();
    Ecole? GetEcoleById(int id);
    Ecole AddEcole(Ecole ecole);
    Ecole UpdateEcole(Ecole ecole);
    bool DeleteEcole(int id);

    // Équipes
    List<Equipe> GetAllEquipes();
    List<Equipe> GetEquipesByEcole(int ecoleId);
    Equipe? GetEquipeById(int id);
    Equipe AddEquipe(Equipe equipe);
    Equipe UpdateEquipe(Equipe equipe);
    bool DeleteEquipe(int id);

    // Joueurs (niveau école)
    List<Joueur> GetAllJoueurs();
    List<Joueur> GetJoueursByEcole(int ecoleId);
    Joueur? GetJoueurById(int id);
    Joueur AddJoueur(Joueur joueur);
    Joueur UpdateJoueur(Joueur joueur);
    bool DeleteJoueur(int id);

    // JoueurEquipes (assignations équipe)
    List<JoueurEquipe> GetAllJoueurEquipes();
    List<JoueurEquipe> GetJoueurEquipesByEquipe(int equipeId);
    List<JoueurEquipe> GetJoueurEquipesByJoueur(int joueurId);
    JoueurEquipe? GetJoueurEquipeById(int id);
    JoueurEquipe? GetJoueurEquipeByJoueurAndEquipe(int joueurId, int equipeId);
    JoueurEquipe AddJoueurEquipe(JoueurEquipe je);
    JoueurEquipe UpdateJoueurEquipe(JoueurEquipe je);
    bool DeleteJoueurEquipe(int id);
    void DeleteJoueurEquipesByJoueur(int joueurId);

    // Galerie
    List<GaleriePhoto> GetAllPhotos();
    List<GaleriePhoto> GetPhotosByEquipe(int equipeId);
    GaleriePhoto? GetPhotoById(int id);
    GaleriePhoto AddPhoto(GaleriePhoto photo);
    GaleriePhoto UpdatePhoto(GaleriePhoto photo);
    bool DeletePhoto(int id);

    // Staff
    List<Staff> GetAllStaff();
    List<Staff> GetStaffByEquipe(int equipeId);
    Staff? GetStaffById(int id);
    List<Staff> GetStaffByNoFiche(string noFiche);
    Staff AddStaff(Staff staff);
    Staff UpdateStaff(Staff staff);
    bool DeleteStaff(int id);

    // Matchs
    List<Match> GetAllMatchs();
    List<Match> GetMatchsByEquipe(int equipeId);
    Match? GetMatchById(int id);
    Match AddMatch(Match match);
    Match UpdateMatch(Match match);
    bool DeleteMatch(int id);

    // Médias de match
    List<MatchMedia> GetAllMatchMedias();
    List<MatchMedia> GetMediasByMatch(int matchId);
    MatchMedia? GetMatchMediaById(int id);
    MatchMedia AddMatchMedia(MatchMedia media);
    bool DeleteMatchMedia(int id);

    // Médias joueur
    List<JoueurMedia> GetAllJoueurMedias();
    List<JoueurMedia> GetMediasByJoueur(int joueurId);
    JoueurMedia AddJoueurMedia(JoueurMedia media);
    bool DeleteJoueurMedia(int id);

    // Absences match
    List<AbsenceMatch> GetAllAbsences();
    List<AbsenceMatch> GetAbsencesByMatch(int matchId);
    AbsenceMatch AddAbsence(AbsenceMatch absence);
    bool DeleteAbsence(int id);
    bool DeleteAbsenceByMatchJoueur(int matchId, int joueurId);

    // Événements
    List<Evenement> GetAllEvenements();
    List<Evenement> GetEvenementsByEquipe(int equipeId);
    Evenement? GetEvenementById(int id);
    Evenement AddEvenement(Evenement ev);
    Evenement UpdateEvenement(Evenement ev);
    bool DeleteEvenement(int id);

    // Années scolaires par école
    List<AnneeScolaireEcole> GetAllAnneesScolaires();
    List<AnneeScolaireEcole> GetAnneesScolairesByEcole(int ecoleId);
    AnneeScolaireEcole AddAnneeScolaire(AnneeScolaireEcole annee);
    bool DeleteAnneeScolaire(int id);

    // Équipes adverses
    List<EquipeAdverse> GetAllEquipesAdverses();
    List<EquipeAdverse> GetEquipesAdversesByEcole(int ecoleId);
    List<EquipeAdverse> GetEquipesAdversesByEcoleSport(int ecoleId, string typeSport);
    EquipeAdverse? GetEquipeAdverseById(int id);
    EquipeAdverse AddEquipeAdverse(EquipeAdverse equipe);
    EquipeAdverse UpdateEquipeAdverse(EquipeAdverse equipe);
    bool DeleteEquipeAdverse(int id);

    // Thèmes d'école
    List<ThemeEcole> GetAllThemes();
    List<ThemeEcole> GetThemesByEcole(int ecoleId);
    ThemeEcole? GetThemeById(int id);
    ThemeEcole AddTheme(ThemeEcole theme);
    ThemeEcole UpdateTheme(ThemeEcole theme);
    bool DeleteTheme(int id);

    // Dictionnaires
    List<DictionnaireEntree> GetAllDictionnaire();
    DictionnaireEntree AddDictionnaire(DictionnaireEntree entree);
    DictionnaireEntree UpdateDictionnaire(DictionnaireEntree entree);
    bool DeleteDictionnaire(int id);
}
