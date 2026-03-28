using System.ComponentModel.DataAnnotations;

namespace GestionEquipeSportive.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
    [Display(Name = "Nom d'utilisateur")]
    public string NomUtilisateur { get; set; } = "";

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string MotDePasse { get; set; } = "";

    [Display(Name = "Se souvenir de moi")]
    public bool SeRappeler { get; set; }

    public string? ReturnUrl { get; set; }
}

public class ChangerMotDePasseViewModel
{
    [Required(ErrorMessage = "Le mot de passe actuel est requis.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe actuel")]
    public string MotDePasseActuel { get; set; } = "";

    [Required(ErrorMessage = "Le nouveau mot de passe est requis.")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@#$%!&*]).{8,}$",
        ErrorMessage = "Le mot de passe doit contenir une majuscule, une minuscule, un chiffre et un caractère spécial (@#$%!&*).")]
    [DataType(DataType.Password)]
    [Display(Name = "Nouveau mot de passe")]
    public string NouveauMotDePasse { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NouveauMotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    [Display(Name = "Confirmer le mot de passe")]
    public string ConfirmerMotDePasse { get; set; } = "";
}
