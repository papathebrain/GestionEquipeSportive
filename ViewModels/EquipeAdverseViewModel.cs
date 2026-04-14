using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GestionEquipeSportive.ViewModels;

public class EquipeAdverseViewModel
{
    public int Id { get; set; }

    [Required]
    public int EcoleId { get; set; }

    [Required(ErrorMessage = "Le type de sport est obligatoire")]
    [Display(Name = "Sport")]
    public string TypeSport { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [Display(Name = "Nom de l'équipe")]
    public string Nom { get; set; } = string.Empty;

    [Display(Name = "Lieu habituel")]
    public string? Lieu { get; set; }

    [Display(Name = "Logo actuel")]
    public string? LogoPathActuel { get; set; }

    [Display(Name = "Logo")]
    public IFormFile? LogoFile { get; set; }
}
