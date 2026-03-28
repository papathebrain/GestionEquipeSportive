using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Http;

namespace GestionEquipeSportive.Services;

public interface IJoueurService
{
    List<Joueur> GetAllJoueurs();
    List<Joueur> GetJoueursByEquipe(int equipeId);
    Joueur? GetJoueurById(int id);
    Joueur CreateJoueur(JoueurViewModel vm, IFormFile? photoFile, string webRootPath);
    Joueur UpdateJoueur(JoueurViewModel vm, IFormFile? photoFile, string webRootPath);
    bool DeleteJoueur(int id, string webRootPath);
    JoueurViewModel ToViewModel(Joueur joueur);
}
