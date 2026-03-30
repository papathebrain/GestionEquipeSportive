using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GestionEquipeSportive.ViewModels;

public class StaffViewModel
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

    [Required(ErrorMessage = "Le titre est obligatoire")]
    [Display(Name = "Titre / Rôle")]
    public string Titre { get; set; } = string.Empty;

    [Display(Name = "Responsable de")]
    public string? ResponsableDe { get; set; }

    [Display(Name = "Photo actuelle")]
    public string? PhotoPathActuelle { get; set; }

    [Display(Name = "Photo")]
    public IFormFile? PhotoFile { get; set; }

    // Navigation
    public string? NomEquipe { get; set; }
    public int EcoleId { get; set; }
}
