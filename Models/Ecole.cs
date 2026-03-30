using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GestionEquipeSportive.Models;

public class Ecole
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string NomEquipe { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string CouleurPrimaire { get; set; } = "#1a3a5c";
    public string CouleurSecondaire { get; set; } = "#e8a020";

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
