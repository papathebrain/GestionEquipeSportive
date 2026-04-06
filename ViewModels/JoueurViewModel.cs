using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GestionEquipeSportive.ViewModels;

public class JoueurViewModel
{
    public int Id { get; set; }

    [Required]
    public int EquipeId { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [Display(Name = "Nom")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prénom est obligatoire")]
    [Display(Name = "Prénom")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le numéro est obligatoire")]
    [Display(Name = "Numéro")]
    public string Numero { get; set; } = string.Empty;

    // Position principale (pour le groupement dans les listes)
    [Display(Name = "Position principale")]
    public string PositionPrincipale { get; set; } = string.Empty;

    // Toutes les paires encodées "Attaque|QB,Défense|CB" — champ hidden soumis
    [Display(Name = "Positions")]
    public string PositionPairsRaw { get; set; } = string.Empty;

    [Display(Name = "Numéro de fiche")]
    public string? NoFiche { get; set; }

    [Display(Name = "Description / Notes")]
    public string? Description { get; set; }

    [Display(Name = "Photo actuelle")]
    public string? PhotoPathActuelle { get; set; }

    [Display(Name = "Photo")]
    public IFormFile? PhotoFile { get; set; }

    [Display(Name = "Consentement photo")]
    public bool ConsentementPhoto { get; set; } = true;

    [Display(Name = "Joueur actif")]
    public bool Actif { get; set; } = true;

    // Infos de navigation
    public string? NomEquipe { get; set; }
    public int EcoleId { get; set; }

    // Listes pour les combobox
    public List<string> PositionsDisponibles { get; set; } = new();
    public List<string> PositionsSpecifiquesDisponibles { get; set; } = new();
    public Dictionary<string, List<string>> PositionsSpecifiquesParGroupe { get; set; } = new();

    // Helper : paires parsées depuis PositionPairsRaw
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
