using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;

namespace GestionEquipeSportive.Services;

public class EquipeService : IEquipeService
{
    private readonly IExcelRepository _repo;
    private readonly IEcoleService _ecoleService;

    public EquipeService(IExcelRepository repo, IEcoleService ecoleService)
    {
        _repo = repo;
        _ecoleService = ecoleService;
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
            Nom = !string.IsNullOrWhiteSpace(vm.Nom) ? vm.Nom.Trim() : GenererNom(vm.EcoleId, vm.TypeSport, vm.Niveau, vm.ThemeId),
            AfficherPublic = vm.AfficherPublic,
            ThemeId = vm.ThemeId,
            CleUnique = Guid.NewGuid()
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
            Nom = !string.IsNullOrWhiteSpace(vm.Nom) ? vm.Nom.Trim() : GenererNom(vm.EcoleId, vm.TypeSport, vm.Niveau, vm.ThemeId),
            AfficherPublic = vm.AfficherPublic,
            ThemeId = vm.ThemeId,
            CleUnique = _repo.GetEquipeById(vm.Id)?.CleUnique ?? Guid.NewGuid()
        };
        return _repo.UpdateEquipe(equipe);
    }

    public bool DeleteEquipe(int id) => _repo.DeleteEquipe(id);

    public bool NomDejaUtilise(int ecoleId, string nom, string annee, int excludeId = 0)
        => _repo.GetEquipesByEcole(ecoleId).Any(e =>
            e.Id != excludeId &&
            string.Equals(e.AnneeScolaire, annee, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(e.Nom, nom, StringComparison.OrdinalIgnoreCase));

    public EquipeViewModel ToViewModel(Equipe equipe) => new EquipeViewModel
    {
        Id = equipe.Id,
        EcoleId = equipe.EcoleId,
        Nom = equipe.Nom,
        AnneeScolaire = equipe.AnneeScolaire,
        TypeSport = equipe.TypeSport,
        Niveau = equipe.Niveau,
        AfficherPublic = equipe.AfficherPublic,
        ThemeId = equipe.ThemeId,
        CleUnique = equipe.CleUnique
    };

    private string GenererNom(int ecoleId, TypeSport sport, NiveauEquipe niveau, int? themeId)
    {
        var ecole = _ecoleService.GetEcoleById(ecoleId);
        string nomEquipe;
        if (themeId.HasValue)
        {
            var theme = ecole?.Themes.FirstOrDefault(t => t.Id == themeId.Value);
            nomEquipe = theme?.NomEquipe ?? ecole?.NomEquipe ?? "";
        }
        else
        {
            nomEquipe = ecole?.NomEquipe ?? "";
        }
        var sportDisplay = sport switch
        {
            TypeSport.FootballAmericain => "Football",
            TypeSport.FlagFootball => "Flag Football",
            TypeSport.Soccer => "Soccer",
            TypeSport.Volleyball => "Volleyball",
            TypeSport.Hockey => "Hockey",
            _ => sport.ToString()
        };
        var niveauDisplay = niveau switch
        {
            NiveauEquipe.Juvenil => "Juvénile",
            NiveauEquipe.PeeWee => "Pee-Wee",
            _ => niveau.ToString()
        };
        return $"{nomEquipe} — {sportDisplay} — {niveauDisplay}";
    }

    public List<string> GetNiveauxPourSport(TypeSport sport)
    {
        return sport switch
        {
            TypeSport.FootballAmericain => new List<string> { "Benjamin", "Cadet", "Juvenil" },
            TypeSport.FlagFootball => new List<string> { "Benjamin", "Cadet", "Juvenil" },
            TypeSport.Soccer => new List<string> { "Benjamin", "Cadet", "Juvenil" },
            TypeSport.Volleyball => new List<string> { "Benjamin", "Cadet", "Juvenil" },
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
