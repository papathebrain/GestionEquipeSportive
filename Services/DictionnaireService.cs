using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.Services;

public class DictionnaireService : IDictionnaireService
{
    private readonly IExcelRepository _repo;

    public DictionnaireService(IExcelRepository repo)
    {
        _repo = repo;
    }

    private static readonly (string Key, string Label)[] SportsParDefaut =
    [
        ("FootballAmericain", "Football américain"),
        ("FlagFootball",      "Flag Football"),
        ("Soccer",            "Soccer"),
        ("Volleyball",        "Volleyball"),
        ("Hockey",            "Hockey")
    ];

    public List<DictionnaireEntree> GetAll() => _repo.GetAllDictionnaire();

    public List<(string Key, string Label)> GetSports()
    {
        var enregistres = _repo.GetAllDictionnaire()
            .Where(e => e.Categorie == "Sport")
            .OrderBy(e => e.Ordre).ThenBy(e => e.Valeur)
            .ToList();

        if (!enregistres.Any())
            return SportsParDefaut.ToList();

        // Seulement les sports actifs
        return enregistres
            .Where(e => e.Actif)
            .Select(e => (Key: e.Valeur, Label: string.IsNullOrEmpty(e.Label) ? e.Valeur : e.Label))
            .ToList();
    }

    public List<string> GetPositions(string sportKey)
        => _repo.GetAllDictionnaire()
            .Where(e => e.Categorie == "Position" && e.Sport == sportKey)
            .OrderBy(e => e.Ordre).ThenBy(e => e.Valeur)
            .Select(e => e.Valeur).ToList();

    public List<string> GetPositionsSpecifiques(string sportKey)
        => _repo.GetAllDictionnaire()
            .Where(e => e.Categorie == "PositionSpecifique" && e.Sport == sportKey)
            .OrderBy(e => e.Ordre).ThenBy(e => e.Valeur)
            .Select(e => e.Valeur).ToList();

    public Dictionary<string, List<string>> GetPositionsSpecifiquesParGroupe(string sportKey)
    {
        var specs = _repo.GetAllDictionnaire()
            .Where(e => e.Categorie == "PositionSpecifique" && e.Sport == sportKey)
            .OrderBy(e => e.Ordre).ThenBy(e => e.Valeur)
            .ToList();

        var result = new Dictionary<string, List<string>>();
        foreach (var spec in specs)
        {
            var key = string.IsNullOrEmpty(spec.ParentValeur) ? "" : spec.ParentValeur;
            if (!result.ContainsKey(key))
                result[key] = new List<string>();
            result[key].Add(spec.Valeur);
        }
        return result;
    }

    public List<string> GetTitresStaff(string sportKey = "")
        => _repo.GetAllDictionnaire()
            .Where(e => e.Categorie == "TitreStaff" &&
                        (string.IsNullOrEmpty(sportKey) || e.Sport == sportKey || string.IsNullOrEmpty(e.Sport)))
            .OrderBy(e => e.Ordre).ThenBy(e => e.Valeur)
            .Select(e => e.Valeur).Distinct().ToList();

    private static readonly Dictionary<string, List<string>> NiveauxParDefaut = new()
    {
        ["FootballAmericain"] = new List<string> { "Benjamin", "Cadet", "Juvenil" },
        ["FlagFootball"]      = new List<string> { "Benjamin", "Cadet", "Juvenil" },
        ["Soccer"]            = new List<string> { "Benjamin", "Cadet", "Juvenil" },
        ["Volleyball"]        = new List<string> { "Benjamin", "Cadet", "Juvenil" },
        ["Hockey"]            = new List<string> { "Atome", "PeeWee", "Bantam" }
    };

    public List<string> GetRoles(string sportKey = "")
        => _repo.GetAllDictionnaire()
            .Where(e => e.Categorie == "RoleStaff" &&
                        (string.IsNullOrEmpty(sportKey) || e.Sport == sportKey || string.IsNullOrEmpty(e.Sport)))
            .OrderBy(e => e.Ordre).ThenBy(e => e.Valeur)
            .Select(e => e.Valeur).Distinct().ToList();

    public List<string> GetNiveaux(string sportKey)
    {
        var enregistres = _repo.GetAllDictionnaire()
            .Where(e => e.Categorie == "Niveau" && e.Sport == sportKey)
            .OrderBy(e => e.Ordre).ThenBy(e => e.Valeur)
            .Select(e => e.Valeur).ToList();
        if (enregistres.Any()) return enregistres;
        return NiveauxParDefaut.TryGetValue(sportKey, out var defaut) ? defaut : new List<string>();
    }

    public DictionnaireEntree Add(string categorie, string sport, string valeur, string label = "", string acronyme = "", string description = "", string parentValeur = "")
    {
        var entree = new DictionnaireEntree
        {
            Categorie   = categorie,
            Sport       = sport,
            Valeur      = valeur,
            Label       = label,
            Acronyme    = acronyme,
            Description = description,
            ParentValeur = parentValeur,
            Actif       = true
        };
        return _repo.AddDictionnaire(entree);
    }

    public DictionnaireEntree Update(int id, string valeur, string label, string acronyme, string description, string parentValeur = "")
    {
        var entree = _repo.GetAllDictionnaire().First(e => e.Id == id);
        entree.Valeur       = valeur;
        entree.Label        = label;
        entree.Acronyme     = acronyme;
        entree.Description  = description;
        entree.ParentValeur = parentValeur;
        return _repo.UpdateDictionnaire(entree);
    }

    public bool ToggleActif(int id)
    {
        var entree = _repo.GetAllDictionnaire().FirstOrDefault(e => e.Id == id);
        if (entree == null) return false;
        entree.Actif = !entree.Actif;
        _repo.UpdateDictionnaire(entree);
        return true;
    }

    public bool Delete(int id) => _repo.DeleteDictionnaire(id);
}
