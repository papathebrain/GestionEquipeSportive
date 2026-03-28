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
}
