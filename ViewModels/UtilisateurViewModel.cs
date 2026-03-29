using GestionEquipeSportive.Models;
using System.ComponentModel.DataAnnotations;

namespace GestionEquipeSportive.ViewModels;

public class EcoleCheckboxItem
{
    public int Id { get; set; }
    public string Nom { get; set; } = "";
    public bool Selectionne { get; set; }
}

public class UtilisateurCreateViewModel
{
    [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Entre 3 et 50 caractères.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Lettres, chiffres, points, tirets et underscores seulement.")]
    [Display(Name = "Nom d'utilisateur")]
    public string NomUtilisateur { get; set; } = "";

    [Required(ErrorMessage = "Le nom complet est requis.")]
    [StringLength(100)]
    [Display(Name = "Nom complet")]
    public string NomComplet { get; set; } = "";

    [Required(ErrorMessage = "Le rôle est requis.")]
    [Display(Name = "Rôle")]
    public string Role { get; set; } = Roles.Utilisateur;

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@#$%!&*]).{8,}$",
        ErrorMessage = "Le mot de passe doit contenir une majuscule, une minuscule, un chiffre et un caractère spécial (@#$%!&*).")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string MotDePasse { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(MotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    [Display(Name = "Confirmer le mot de passe")]
    public string ConfirmerMotDePasse { get; set; } = "";

    [Display(Name = "Forcer changement au prochain login")]
    public bool ChangerMotDePasse { get; set; } = true;

    [Display(Name = "Écoles accessibles")]
    public List<EcoleCheckboxItem> Ecoles { get; set; } = [];
}

public class UtilisateurEditViewModel
{
    public string Id { get; set; } = "";

    [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Lettres, chiffres, points, tirets et underscores seulement.")]
    [Display(Name = "Nom d'utilisateur")]
    public string NomUtilisateur { get; set; } = "";

    [Required(ErrorMessage = "Le nom complet est requis.")]
    [StringLength(100)]
    [Display(Name = "Nom complet")]
    public string NomComplet { get; set; } = "";

    [Required]
    [Display(Name = "Rôle")]
    public string Role { get; set; } = Roles.Utilisateur;

    [Display(Name = "Compte actif")]
    public bool EstActif { get; set; } = true;

    [Display(Name = "Forcer changement de mot de passe")]
    public bool ChangerMotDePasse { get; set; }

    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@#$%!&*]).{8,}$",
        ErrorMessage = "Le mot de passe doit contenir une majuscule, une minuscule, un chiffre et un caractère spécial.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nouveau mot de passe (laisser vide pour conserver)")]
    public string? NouveauMotDePasse { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NouveauMotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    [Display(Name = "Confirmer le mot de passe")]
    public string? ConfirmerMotDePasse { get; set; }

    [Display(Name = "Écoles accessibles")]
    public List<EcoleCheckboxItem> Ecoles { get; set; } = [];
}
