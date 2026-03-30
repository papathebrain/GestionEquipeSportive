using System.ComponentModel.DataAnnotations;
using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.ViewModels;

public class EvenementViewModel
{
    public int Id { get; set; }

    [Required]
    public int EquipeId { get; set; }

    [Required(ErrorMessage = "Le titre est obligatoire")]
    [Display(Name = "Titre")]
    public string Titre { get; set; } = string.Empty;

    [Display(Name = "Type")]
    public TypeEvenement Type { get; set; } = TypeEvenement.Pratique;

    [Required(ErrorMessage = "La date est obligatoire")]
    [Display(Name = "Date")]
    public string DateStr { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");

    [Display(Name = "Heure de début")]
    public string HeureDebut { get; set; } = "09:00";

    [Display(Name = "Heure de fin")]
    public string? HeureFin { get; set; }

    [Display(Name = "Lieu")]
    public string? Lieu { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    // Récurrence
    [Display(Name = "Répéter l'événement")]
    public bool Recurrence { get; set; } = false;

    /// <summary>Jours de la semaine sélectionnés : "1"=Lundi … "7"=Dimanche</summary>
    public List<int> JoursSemaine { get; set; } = new();

    [Display(Name = "Répéter jusqu'au")]
    public string? RecurrenceDateFin { get; set; }

    // Navigation
    public string? NomEquipe { get; set; }
}
