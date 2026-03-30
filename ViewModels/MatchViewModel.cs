using System.ComponentModel.DataAnnotations;

namespace GestionEquipeSportive.ViewModels;

public class MatchViewModel
{
    public int Id { get; set; }

    [Required]
    public int EquipeId { get; set; }

    [Required(ErrorMessage = "La date est obligatoire")]
    [Display(Name = "Date du match")]
    [DataType(DataType.Date)]
    public DateTime DateMatch { get; set; } = DateTime.Today;

    [Display(Name = "Arrivée au vestiaire")]
    public string? HeureArriveeVestiaire { get; set; }

    [Display(Name = "Départ de l'autobus")]
    public string? HeureDepartAutobus { get; set; }

    [Display(Name = "Début du match")]
    public string? HeureDebutMatch { get; set; }

    [Display(Name = "Match à domicile")]
    public bool EstDomicile { get; set; }

    [Required(ErrorMessage = "L'adversaire est obligatoire")]
    [Display(Name = "Adversaire")]
    public string Adversaire { get; set; } = "";

    [Display(Name = "Lieu")]
    public string? Lieu { get; set; }

    [Display(Name = "Notre score")]
    [Range(0, 999)]
    public int? ScoreEquipe { get; set; }

    [Display(Name = "Score adversaire")]
    [Range(0, 999)]
    public int? ScoreAdversaire { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    // Navigation (affichage)
    public string? NomEquipe { get; set; }
    public string? NomEcole { get; set; }
    public int EcoleId { get; set; }
}

public class StatistiquesMatchViewModel
{
    public int MatchsJoues { get; set; }
    public int Victoires { get; set; }
    public int Defaites { get; set; }
    public int Nuls { get; set; }
    public int ButsPour { get; set; }
    public int ButsContre { get; set; }
    public int MatchsAVenir { get; set; }
}
