using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.Services;

public interface IEvenementService
{
    List<Evenement> GetEvenementsByEquipe(int equipeId);
    Evenement? GetEvenementById(int id);
    Evenement CreateEvenement(Evenement ev);
    Evenement UpdateEvenement(Evenement ev);
    bool DeleteEvenement(int id);
}
