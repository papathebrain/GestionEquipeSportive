using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.Services;

public interface IDictionnaireService
{
    List<DictionnaireEntree> GetAll();
    List<(string Key, string Label)> GetSports();
    List<string> GetPositions(string sportKey);
    List<string> GetPositionsSpecifiques(string sportKey);
    Dictionary<string, List<string>> GetPositionsSpecifiquesParGroupe(string sportKey);
    List<string> GetTitresStaff(string sportKey = "");
    List<string> GetRoles(string sportKey = "");
    List<string> GetNiveaux(string sportKey);
    DictionnaireEntree Add(string categorie, string sport, string valeur, string label = "", string acronyme = "", string description = "", string parentValeur = "");
    DictionnaireEntree Update(int id, string valeur, string label, string acronyme, string description, string parentValeur = "");
    bool ToggleActif(int id);
    bool Delete(int id);
}
