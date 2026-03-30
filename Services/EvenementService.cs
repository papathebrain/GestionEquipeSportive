using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.Services;

public class EvenementService : IEvenementService
{
    private readonly IExcelRepository _repo;
    public EvenementService(IExcelRepository repo) => _repo = repo;

    public List<Evenement> GetEvenementsByEquipe(int equipeId) => _repo.GetEvenementsByEquipe(equipeId);
    public Evenement? GetEvenementById(int id) => _repo.GetEvenementById(id);
    public Evenement CreateEvenement(Evenement ev) => _repo.AddEvenement(ev);
    public Evenement UpdateEvenement(Evenement ev) => _repo.UpdateEvenement(ev);
    public bool DeleteEvenement(int id) => _repo.DeleteEvenement(id);
}
