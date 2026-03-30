using GestionEquipeSportive.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GestionEquipeSportive.ViewModels;

public class EquipeViewModel
{
    public int Id { get; set; }

    [Required]
    public int EcoleId { get; set; }

    [Required(ErrorMessage = "L'année scolaire est obligatoire")]
    [Display(Name = "Année scolaire")]
    public string AnneeScolaire { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le type de sport est obligatoire")]
    [Display(Name = "Sport")]
    public TypeSport TypeSport { get; set; }

    [Required(ErrorMessage = "Le niveau est obligatoire")]
    [Display(Name = "Niveau")]
    public NiveauEquipe Niveau { get; set; }

    [Display(Name = "Afficher sur le site public")]
    public bool AfficherPublic { get; set; }

    // Listes de sélection
    public List<SelectListItem> SportsList { get; set; } = new();
    public List<SelectListItem> NiveauxList { get; set; } = new();
    public List<SelectListItem> AnneesList { get; set; } = new();

    // Nom de l'école (affichage)
    public string? NomEcole { get; set; }
}
