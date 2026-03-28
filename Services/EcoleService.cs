using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;

namespace GestionEquipeSportive.Services;

public class EcoleService : IEcoleService
{
    private readonly IExcelRepository _repo;

    public EcoleService(IExcelRepository repo)
    {
        _repo = repo;
    }

    public List<Ecole> GetAllEcoles() => _repo.GetAllEcoles();

    public Ecole? GetEcoleById(int id) => _repo.GetEcoleById(id);

    public Ecole CreateEcole(EcoleViewModel vm, IFormFile? logoFile, string webRootPath)
    {
        var ecole = new Ecole
        {
            Nom = vm.Nom,
            CodeEcole = vm.CodeEcole,
            CouleurPrimaire = vm.CouleurPrimaire,
            CouleurSecondaire = vm.CouleurSecondaire
        };

        if (logoFile != null && logoFile.Length > 0)
        {
            ecole.LogoPath = SaveFile(logoFile, Path.Combine(webRootPath, "uploads", "logos"));
        }

        return _repo.AddEcole(ecole);
    }

    public Ecole UpdateEcole(EcoleViewModel vm, IFormFile? logoFile, string webRootPath)
    {
        var ecole = _repo.GetEcoleById(vm.Id) ?? new Ecole();
        ecole.Id = vm.Id;
        ecole.Nom = vm.Nom;
        ecole.CodeEcole = vm.CodeEcole;
        ecole.CouleurPrimaire = vm.CouleurPrimaire;
        ecole.CouleurSecondaire = vm.CouleurSecondaire;

        if (logoFile != null && logoFile.Length > 0)
        {
            // Supprimer l'ancien logo
            if (!string.IsNullOrEmpty(ecole.LogoPath))
            {
                var oldPath = Path.Combine(webRootPath, ecole.LogoPath.TrimStart('/'));
                if (File.Exists(oldPath)) File.Delete(oldPath);
            }
            ecole.LogoPath = SaveFile(logoFile, Path.Combine(webRootPath, "uploads", "logos"));
        }

        return _repo.UpdateEcole(ecole);
    }

    public bool DeleteEcole(int id) => _repo.DeleteEcole(id);

    public EcoleViewModel ToViewModel(Ecole ecole) => new EcoleViewModel
    {
        Id = ecole.Id,
        Nom = ecole.Nom,
        CodeEcole = ecole.CodeEcole,
        LogoPathActuel = ecole.LogoPath,
        CouleurPrimaire = ecole.CouleurPrimaire,
        CouleurSecondaire = ecole.CouleurSecondaire
    };

    private static string SaveFile(IFormFile file, string directory)
    {
        Directory.CreateDirectory(directory);
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(directory, fileName);
        using var stream = new FileStream(fullPath, FileMode.Create);
        file.CopyTo(stream);
        return $"/uploads/logos/{fileName}";
    }
}
