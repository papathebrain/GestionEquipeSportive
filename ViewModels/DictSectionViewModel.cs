using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.ViewModels;

public class DictSectionViewModel
{
    public string Titre { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-list-ul";
    public List<DictionnaireEntree> Entrees { get; set; } = new();
    public string Categorie { get; set; } = string.Empty;
    public string Sport { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public List<string> ParentPositions { get; set; } = new(); // Positions parentes pour PositionSpecifique
}
