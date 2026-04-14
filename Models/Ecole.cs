using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GestionEquipeSportive.Models;

public class LienSocial
{
    public string Type { get; set; } = "Site web";  // Facebook, Instagram, X, YouTube, TikTok, Site web, Autre
    public string Url { get; set; } = "";
    public string? Label { get; set; }
}

public class Ecole
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string NomEquipe { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string CouleurPrimaire { get; set; } = "#1a3a5c";
    public string CouleurSecondaire { get; set; } = "#e8a020";
    public List<LienSocial> LiensSociaux { get; set; } = new();
    public List<ThemeEcole> Themes { get; set; } = new();
    public List<EquipeAdverse> EquipesAdverses { get; set; } = new();
    public List<AnneeScolaireEcole> AnneesScolaires { get; set; } = new();

    public string Slug => ToSlug(Nom);

    public static string ToSlug(string texte)
    {
        if (string.IsNullOrWhiteSpace(texte)) return "";
        var normalized = texte.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        var s = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, @"-+", "-").Trim('-');
        return s;
    }
}
