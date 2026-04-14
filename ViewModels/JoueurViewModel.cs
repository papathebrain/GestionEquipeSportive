using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GestionEquipeSportive.ViewModels;

// ViewModel pour créer/éditer un joueur au niveau école
public class JoueurViewModel
{
    public int Id { get; set; }

    [Required]
    public int EcoleId { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [Display(Name = "Nom")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prénom est obligatoire")]
    [Display(Name = "Prénom")]
    public string Prenom { get; set; } = string.Empty;

    [Display(Name = "Numéro de fiche GPI")]
    public string? NoFiche { get; set; }

    [Display(Name = "Consentement photo")]
    public bool ConsentementPhoto { get; set; } = true;

    [Display(Name = "Actif")]
    public bool Actif { get; set; } = true;

    // Infos de navigation (lecture seulement)
    public string? NomEcole { get; set; }
    public Guid? CleUnique { get; set; }
}

// ViewModel pour l'assignation d'un joueur à une équipe
public class JoueurEquipeViewModel
{
    public int Id { get; set; }

    [Required]
    public int JoueurId { get; set; }

    [Required]
    public int EquipeId { get; set; }

    [Display(Name = "Position principale")]
    public string PositionPrincipale { get; set; } = string.Empty;

    [Display(Name = "Positions")]
    public string PositionPairsRaw { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le numéro est obligatoire")]
    [Display(Name = "Numéro")]
    public string Numero { get; set; } = string.Empty;

    [Display(Name = "Description / Notes")]
    public string? Description { get; set; }

    [Display(Name = "Photo actuelle")]
    public string? PhotoPathActuelle { get; set; }

    [Display(Name = "Photo")]
    public IFormFile? PhotoFile { get; set; }

    [Display(Name = "Joueur actif")]
    public bool Actif { get; set; } = true;

    // Navigation
    public string? NomJoueur { get; set; }
    public string? NomEquipe { get; set; }
    public int EcoleId { get; set; }

    // Listes pour combobox
    public List<string> PositionsDisponibles { get; set; } = new();
    public List<string> PositionsSpecifiquesDisponibles { get; set; } = new();
    public Dictionary<string, List<string>> PositionsSpecifiquesParGroupe { get; set; } = new();

    public List<(string Pos, string Spec)> PositionPairs
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PositionPairsRaw)) return new();
            return PositionPairsRaw
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(p => { var parts = p.Split('|'); return (parts[0].Trim(), parts.Length > 1 ? parts[1].Trim() : ""); })
                .ToList();
        }
    }
}
