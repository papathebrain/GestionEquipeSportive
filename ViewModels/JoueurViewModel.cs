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

    [Required(ErrorMessage = "La position est obligatoire")]
    [Display(Name = "Position (groupe)")]
    public string Position { get; set; } = string.Empty;

    [Display(Name = "Position spécifique")]
    public string? PositionSpecifique { get; set; }

    [Display(Name = "Numéro de fiche")]
    public string? NoFiche { get; set; }

    [Display(Name = "Description / Notes")]
    public string? Description { get; set; }

    [Display(Name = "Photo actuelle")]
    public string? PhotoPathActuelle { get; set; }

    [Display(Name = "Photo")]
    public IFormFile? PhotoFile { get; set; }

    // Infos de navigation
    public string? NomEquipe { get; set; }
    public int EcoleId { get; set; }
}
