namespace GestionEquipeSportive.Models;

public class AnneeScolaireEcole
{
    public int Id { get; set; }
    public int EcoleId { get; set; }
    public string AnneeScolaire { get; set; } = string.Empty;
}
