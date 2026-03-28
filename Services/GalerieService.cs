using GestionEquipeSportive.Models;
using Microsoft.AspNetCore.Http;

namespace GestionEquipeSportive.Services;

public class GalerieService : IGalerieService
{
    private readonly IExcelRepository _repo;

    public GalerieService(IExcelRepository repo)
    {
        _repo = repo;
    }

    public List<GaleriePhoto> GetPhotosByEquipe(int equipeId) => _repo.GetPhotosByEquipe(equipeId);

    public GaleriePhoto? GetPhotoById(int id) => _repo.GetPhotoById(id);

    public GaleriePhoto AddPhoto(int equipeId, IFormFile photoFile, string? description, string webRootPath)
    {
        var directory = Path.Combine(webRootPath, "uploads", "galerie");
        Directory.CreateDirectory(directory);
        var ext = Path.GetExtension(photoFile.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(directory, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        photoFile.CopyTo(stream);

        var photo = new GaleriePhoto
        {
            EquipeId = equipeId,
            CheminPhoto = $"/uploads/galerie/{fileName}",
            Description = description,
            DateAjout = DateTime.Now
        };

        return _repo.AddPhoto(photo);
    }

    public GaleriePhoto UpdatePhoto(int id, string? description)
    {
        var photo = _repo.GetPhotoById(id) ?? throw new Exception("Photo introuvable");
        photo.Description = description;
        return _repo.UpdatePhoto(photo);
    }

    public bool DeletePhoto(int id, string webRootPath)
    {
        var photo = _repo.GetPhotoById(id);
        if (photo != null && !string.IsNullOrEmpty(photo.CheminPhoto))
        {
            var filePath = Path.Combine(webRootPath, photo.CheminPhoto.TrimStart('/'));
            if (File.Exists(filePath)) File.Delete(filePath);
        }
        return _repo.DeletePhoto(id);
    }
}
