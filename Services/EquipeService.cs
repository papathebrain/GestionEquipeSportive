using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;

namespace GestionEquipeSportive.Services;

public class EquipeService : IEquipeService
{
    private readonly IExcelRepository _repo;

    public EquipeService(IExcelRepository repo)
    {
        _repo = repo;
    }

    public List<Equipe> GetAllEquipes() => _repo.GetAllEquipes();

    public List<Equipe> GetEquipesByEcole(int ecoleId) => _repo.GetEquipesByEcole(ecoleId);

    public Equipe? GetEquipeById(int id) => _repo.GetEquipeById(id);

    public Equipe CreateEquipe(EquipeViewModel vm)
    {
        var equipe = new Equipe
        {
            EcoleId = vm.EcoleId,
            AnneeScolaire = vm.AnneeScolaire,
            TypeSport = vm.TypeSport,
            Niveau = vm.Niveau,
            Nom = vm.Nom
        };
        return _repo.AddEquipe(equipe);
    }

    public Equipe UpdateEquipe(EquipeViewModel vm)
    {
        var equipe = new Equipe
        {
            Id = vm.Id,
            EcoleId = vm.EcoleId,
            AnneeScolaire = vm.AnneeScolaire,
            TypeSport = vm.TypeSport,
            Niveau = vm.Niveau,
            Nom = vm.Nom
        };
        return _repo.UpdateEquipe(equipe);
    }

    public bool DeleteEquipe(int id) => _repo.DeleteEquipe(id);

    public EquipeViewModel ToViewModel(Equipe equipe) => new EquipeViewModel
    {
        Id = equipe.Id,
        EcoleId = equipe.EcoleId,
        AnneeScolaire = equipe.AnneeScolaire,
        TypeSport = equipe.TypeSport,
        Niveau = equipe.Niveau,
        Nom = equipe.Nom
    };

    public List<string> GetNiveauxPourSport(TypeSport sport)
    {
        return sport switch
        {
            TypeSport.FootballAmericain => new List<string> { "Benjamin", "Cadet", "Juvenil" },
            TypeSport.Soccer => new List<string> { "Benjamin", "Cadet", "Juvenil" },
            TypeSport.Hockey => new List<string> { "Atome", "PeeWee", "Bantam" },
            _ => new List<string>()
        };
    }

    public List<string> GetAnnesScolaires()
    {
        var annees = new List<string>();
        int anneeActuelle = DateTime.Now.Year;
        for (int i = anneeActuelle - 1; i <= anneeActuelle + 2; i++)
        {
            annees.Add($"{i}-{i + 1}");
        }
        return annees;
    }
}
