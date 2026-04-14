namespace GestionEquipeSportive.Models;

public class Joueur
{
    public int Id { get; set; }
    public int EcoleId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? NoFiche { get; set; }       // Clé externe GPI
    public Guid? CleUnique { get; set; }       // Clé unique permanente GUID
    public bool ConsentementPhoto { get; set; } = true;
    public bool Actif { get; set; } = true;

    // Navigation
    public Ecole? Ecole { get; set; }
    public List<JoueurEquipe> JoueurEquipes { get; set; } = new();
}

public class JoueurMedia
{
    public int Id { get; set; }
    public int JoueurId { get; set; }
    public string CheminFichier { get; set; } = "";
    public string? Description { get; set; }
    public DateTime DateAjout { get; set; } = DateTime.UtcNow;
}
