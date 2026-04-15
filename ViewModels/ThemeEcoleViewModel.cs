using GestionEquipeSportive.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GestionEquipeSportive.ViewModels;

public class ThemeEcoleViewModel
{
    public int Id { get; set; }

    [Required]
    public int EcoleId { get; set; }

    [Required(ErrorMessage = "Le nom d'équipe est obligatoire")]
    [Display(Name = "Nom d'équipe")]
    public string NomEquipe { get; set; } = string.Empty;

    [Required(ErrorMessage = "La couleur primaire est obligatoire")]
    [Display(Name = "Couleur primaire")]
    public string CouleurPrimaire { get; set; } = "#1a3a5c";

    [Required(ErrorMessage = "La couleur secondaire est obligatoire")]
    [Display(Name = "Couleur secondaire")]
    public string CouleurSecondaire { get; set; } = "#e8a020";

    [Display(Name = "Logo actuel")]
    public string? LogoPathActuel { get; set; }

    [Display(Name = "Logo")]
    public IFormFile? LogoFile { get; set; }

    [Display(Name = "Musique match à venir (actuelle)")]
    public string? MusiqueProchainMatchPathActuel { get; set; }
    [Display(Name = "Musique match à venir")]
    public IFormFile? MusiqueProchainMatchFile { get; set; }
    public int? MusiqueProchainMatchDebut { get; set; }
    public int? MusiqueProchainMatchDuree { get; set; }

    [Display(Name = "Musique victoire (actuelle)")]
    public string? MusiqueVictoirePathActuel { get; set; }
    [Display(Name = "Musique victoire")]
    public IFormFile? MusiqueVictoireFile { get; set; }
    public int? MusiqueVictoireDebut { get; set; }
    public int? MusiqueVictoireDuree { get; set; }

    [Display(Name = "Musique défaite (actuelle)")]
    public string? MusiqueDefaitePathActuel { get; set; }
    [Display(Name = "Musique défaite")]
    public IFormFile? MusiqueDefaiteFile { get; set; }
    public int? MusiqueDefaiteDebut { get; set; }
    public int? MusiqueDefaiteDuree { get; set; }
}
