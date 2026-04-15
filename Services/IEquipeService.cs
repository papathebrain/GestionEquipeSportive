using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;

namespace GestionEquipeSportive.Services;

public interface IEquipeService
{
    List<Equipe> GetAllEquipes();
    List<Equipe> GetEquipesByEcole(int ecoleId);
    Equipe? GetEquipeById(int id);
    Equipe CreateEquipe(EquipeViewModel vm);
    Equipe UpdateEquipe(EquipeViewModel vm);
    bool DeleteEquipe(int id);
    EquipeViewModel ToViewModel(Equipe equipe);
    bool NomDejaUtilise(int ecoleId, string nom, string annee, int excludeId = 0);
    List<string> GetNiveauxPourSport(TypeSport sport);
    List<string> GetAnnesScolaires();
}
