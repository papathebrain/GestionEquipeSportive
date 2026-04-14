namespace GestionEquipeSportive.Models;

public class JoueurEquipe
{
    public int Id { get; set; }
    public int JoueurId { get; set; }
    public int EquipeId { get; set; }
    public string Position { get; set; } = string.Empty;
    public string? PositionSpecifique { get; set; }    // Paires "Attaque|QB,Défense|CB"
    public string Numero { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public string? Description { get; set; }
    public bool Actif { get; set; } = true;

    // Navigation
    public Joueur? Joueur { get; set; }
    public Equipe? Equipe { get; set; }

    // Helpers
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
}
