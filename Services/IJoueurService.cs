using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Http;

namespace GestionEquipeSportive.Services;

public interface IJoueurService
{
    // Joueurs (niveau école)
    List<Joueur> GetAllJoueurs();
    List<Joueur> GetJoueursByEcole(int ecoleId);
    Joueur? GetJoueurById(int id);
    Joueur CreateJoueur(JoueurViewModel vm);
    Joueur UpdateJoueur(JoueurViewModel vm);
    bool DeleteJoueur(int id, string webRootPath);
    JoueurViewModel ToViewModel(Joueur joueur);

    // JoueurEquipes (assignations)
    List<JoueurEquipe> GetAllJoueurEquipes();
    List<JoueurEquipe> GetJoueurEquipesByEquipe(int equipeId);
    List<JoueurEquipe> GetJoueurEquipesByJoueur(int joueurId);
    JoueurEquipe? GetJoueurEquipeById(int id);
    JoueurEquipe AssignerAEquipe(JoueurEquipeViewModel vm, IFormFile? photoFile, string webRootPath);
    JoueurEquipe UpdateAssignation(JoueurEquipeViewModel vm, IFormFile? photoFile, string webRootPath);
    bool SupprimerAssignation(int joueurEquipeId, string webRootPath);
    JoueurEquipeViewModel ToAssignationViewModel(JoueurEquipe je);

    // Médias
    List<JoueurMedia> GetMediasByJoueur(int joueurId);
    JoueurMedia AddJoueurMedia(int joueurId, IFormFile file, string webRootPath);
    bool DeleteJoueurMedia(int id, string webRootPath);
}
