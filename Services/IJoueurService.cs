using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Http;

namespace GestionEquipeSportive.Services;

public interface IJoueurService
{
    List<Joueur> GetAllJoueurs();
    List<Joueur> GetJoueursByEquipe(int equipeId);
    Joueur? GetJoueurById(int id);
    List<Joueur> GetHistoriqueJoueur(Joueur joueur);
    void CopierVersEquipe(IEnumerable<int> joueurIds, int nouvelleEquipeId);
    Joueur CreateJoueur(JoueurViewModel vm, IFormFile? photoFile, string webRootPath);
    Joueur UpdateJoueur(JoueurViewModel vm, IFormFile? photoFile, string webRootPath);
    bool DeleteJoueur(int id, string webRootPath);
    JoueurViewModel ToViewModel(Joueur joueur);
    List<JoueurMedia> GetMediasByJoueur(int joueurId);
    JoueurMedia AddJoueurMedia(int joueurId, IFormFile file, string webRootPath);
    bool DeleteJoueurMedia(int id, string webRootPath);
}
