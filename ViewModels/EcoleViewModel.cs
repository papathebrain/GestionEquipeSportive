using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GestionEquipeSportive.ViewModels;

public class EcoleViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [Display(Name = "Nom de l'école")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le code est obligatoire")]
    [Display(Name = "Code de l'école")]
    public string CodeEcole { get; set; } = string.Empty;

    [Display(Name = "Logo actuel")]
    public string? LogoPathActuel { get; set; }

    [Display(Name = "Logo")]
    public IFormFile? LogoFile { get; set; }

    [Required(ErrorMessage = "La couleur primaire est obligatoire")]
    [Display(Name = "Couleur primaire")]
    public string CouleurPrimaire { get; set; } = "#1a3a5c";

    [Required(ErrorMessage = "La couleur secondaire est obligatoire")]
    [Display(Name = "Couleur secondaire")]
    public string CouleurSecondaire { get; set; } = "#e8a020";
}
