namespace GestionEquipeSportive.Models;

public class DictionnaireEntree
{
    public int Id { get; set; }
    public string Categorie { get; set; } = string.Empty; // "Position", "PositionSpecifique", "TitreStaff", "RoleStaff", "Niveau", "Sport"
    public string Sport { get; set; } = string.Empty;     // TypeSport.ToString() or "" for staff
    public string Valeur { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;  // Libellé affiché (utilisé pour les sports)
    public string Acronyme { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Ordre { get; set; }
    public string ParentValeur { get; set; } = string.Empty; // Pour PositionSpecifique : Valeur de la Position parente
    public bool Actif { get; set; } = true;                  // Pour Sport : actif dans l'application
}
