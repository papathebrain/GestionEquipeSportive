using GestionEquipeSportive.Models;
using Microsoft.AspNetCore.Http;

namespace GestionEquipeSportive.Services;

public interface IGalerieService
{
    List<GaleriePhoto> GetPhotosByEquipe(int equipeId);
    GaleriePhoto? GetPhotoById(int id);
    GaleriePhoto AddPhoto(int equipeId, IFormFile photoFile, string? description, string webRootPath);
    GaleriePhoto UpdatePhoto(int id, string? description);
    bool DeletePhoto(int id, string webRootPath);
}
