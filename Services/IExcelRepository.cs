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

    // Joueurs
    List<Joueur> GetAllJoueurs();
    List<Joueur> GetJoueursByEquipe(int equipeId);
    Joueur? GetJoueurById(int id);
    Joueur AddJoueur(Joueur joueur);
    Joueur UpdateJoueur(Joueur joueur);
    bool DeleteJoueur(int id);

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
}
