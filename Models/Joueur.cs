namespace GestionEquipeSportive.Models;

public class Joueur
{
    public int Id { get; set; }
    public int EquipeId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;      // Position principale (pour groupement)
    public string? PositionSpecifique { get; set; }           // Paires encodées : "Attaque|QB,Défense|CB"

    // Helpers calculés
    // Retourne toutes les paires (Position, PositionSpecifique)
    public List<(string Pos, string Spec)> PositionPairs
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PositionSpecifique)) return new();
            return PositionSpecifique
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(p => { var parts = p.Split('|'); return (parts[0].Trim(), parts.Length > 1 ? parts[1].Trim() : ""); })
                .ToList();
        }
    }

    public List<string> Positions => PositionPairs.Select(p => p.Pos).Distinct().ToList();
    public List<string> PositionsSpecifiques => PositionPairs.Select(p => p.Spec).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
    public string? PhotoPath { get; set; }
    public string? NoFiche { get; set; }  // Identifiant permanent du joueur entre les années
    public string? Description { get; set; }
    public bool ConsentementPhoto { get; set; } = true;  // Consentement parental pour diffusion des photos
    public bool Actif { get; set; } = true;

    // Navigation
    public Equipe? Equipe { get; set; }
}

public class JoueurMedia
{
    public int Id { get; set; }
    public int JoueurId { get; set; }
    public string CheminFichier { get; set; } = "";
    public string? Description { get; set; }
    public DateTime DateAjout { get; set; } = DateTime.UtcNow;
}
