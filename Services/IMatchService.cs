using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;

namespace GestionEquipeSportive.Services;

public interface IMatchService
{
    List<Match> GetMatchsByEquipe(int equipeId);
    Match? GetMatchById(int id);
    Match CreateMatch(MatchViewModel vm);
    Match UpdateMatch(MatchViewModel vm);
    bool DeleteMatch(int id);
    MatchViewModel ToViewModel(Match match);
    StatistiquesMatchViewModel GetStatistiques(int equipeId);

    // Médias
    List<MatchMedia> GetMediasByMatch(int matchId);
    List<MatchMedia> AddMedias(int matchId, List<IFormFile> fichiers, TypeMedia type, string? description, string webRootPath);
    bool DeleteMedia(int id, string webRootPath);

    // Absences
    List<AbsenceMatch> GetAbsencesByMatch(int matchId);
    void ToggleAbsence(int matchId, int joueurId);
}
