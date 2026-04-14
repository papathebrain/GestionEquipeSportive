using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Http;

namespace GestionEquipeSportive.Services;

public interface IEcoleService
{
    List<Ecole> GetAllEcoles();
    Ecole? GetEcoleById(int id);
    Ecole CreateEcole(EcoleViewModel vm, IFormFile? logoFile, string webRootPath);
    Ecole UpdateEcole(EcoleViewModel vm, IFormFile? logoFile, string webRootPath);
    bool DeleteEcole(int id);
    EcoleViewModel ToViewModel(Ecole ecole);

    // Équipes adverses
    List<EquipeAdverse> GetEquipesAdversesByEcole(int ecoleId);
    List<EquipeAdverse> GetEquipesAdversesByEcoleSport(int ecoleId, string typeSport);
    EquipeAdverse? GetEquipeAdverseById(int id);
    EquipeAdverse CreateEquipeAdverse(EquipeAdverseViewModel vm, IFormFile? logoFile, string webRootPath);
    EquipeAdverse UpdateEquipeAdverse(EquipeAdverseViewModel vm, IFormFile? logoFile, string webRootPath);
    bool DeleteEquipeAdverse(int id, string webRootPath);
    EquipeAdverseViewModel ToEquipeAdverseViewModel(EquipeAdverse equipe);

    // Thèmes
    List<ThemeEcole> GetThemesByEcole(int ecoleId);
    ThemeEcole? GetThemeById(int id);
    ThemeEcole CreateTheme(ThemeEcoleViewModel vm, IFormFile? logoFile, string webRootPath);
    ThemeEcole UpdateTheme(ThemeEcoleViewModel vm, IFormFile? logoFile, string webRootPath);
    bool DeleteTheme(int id, string webRootPath);
    ThemeEcoleViewModel ToThemeViewModel(ThemeEcole theme);

    // Années scolaires
    List<AnneeScolaireEcole> GetAnneesScolairesByEcole(int ecoleId);
    AnneeScolaireEcole AddAnneeScolaire(int ecoleId, string annee);
    bool DeleteAnneeScolaire(int id);
}
