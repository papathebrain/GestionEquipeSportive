using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;

namespace GestionEquipeSportive.Services;

public class StaffService : IStaffService
{
    private readonly IExcelRepository _repo;

    public StaffService(IExcelRepository repo)
    {
        _repo = repo;
    }

    public List<Staff> GetStaffByEquipe(int equipeId) => _repo.GetStaffByEquipe(equipeId);
    public Staff? GetStaffById(int id) => _repo.GetStaffById(id);
    public List<Staff> GetStaffByNoFiche(string noFiche) => _repo.GetStaffByNoFiche(noFiche);

    public void CopierVersEquipe(IEnumerable<int> staffIds, int nouvelleEquipeId)
    {
        var destStaff = _repo.GetStaffByEquipe(nouvelleEquipeId);
        foreach (var id in staffIds)
        {
            var source = _repo.GetStaffById(id);
            if (source == null) continue;
            // Ne pas dupliquer si même NoFiche déjà présent dans l'équipe destination
            if (!string.IsNullOrEmpty(source.NoFiche) &&
                destStaff.Any(s => s.NoFiche == source.NoFiche))
                continue;
            // Assurer que la source a un NoFiche (migration des anciens enregistrements)
            if (string.IsNullOrEmpty(source.NoFiche))
            {
                source.NoFiche = Guid.NewGuid().ToString("N");
                _repo.UpdateStaff(source);
            }
            _repo.AddStaff(new Staff
            {
                EquipeId = nouvelleEquipeId,
                Nom = source.Nom,
                Prenom = source.Prenom,
                Titre = source.Titre,
                Description = source.Description,
                PhotoPath = source.PhotoPath,
                NoFiche = source.NoFiche
            });
        }
    }

    public Staff CreateStaff(StaffViewModel vm, IFormFile? photoFile, string webRootPath)
    {
        var staff = new Staff
        {
            EquipeId = vm.EquipeId,
            Nom = vm.Nom.Trim(),
            Prenom = vm.Prenom.Trim(),
            Titre = vm.Titre.Trim(),
            Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim(),
            NoFiche = Guid.NewGuid().ToString("N")
        };
        if (photoFile != null && photoFile.Length > 0)
            staff.PhotoPath = SavePhoto(photoFile, webRootPath);
        return _repo.AddStaff(staff);
    }

    public Staff UpdateStaff(StaffViewModel vm, IFormFile? photoFile, string webRootPath)
    {
        var staff = _repo.GetStaffById(vm.Id) ?? new Staff();
        staff.Id = vm.Id;
        staff.EquipeId = vm.EquipeId;
        staff.Nom = vm.Nom.Trim();
        staff.Prenom = vm.Prenom.Trim();
        staff.Titre = vm.Titre.Trim();
        staff.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();

        if (photoFile != null && photoFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(staff.PhotoPath))
            {
                var old = Path.Combine(webRootPath, staff.PhotoPath.TrimStart('/'));
                if (File.Exists(old)) File.Delete(old);
            }
            staff.PhotoPath = SavePhoto(photoFile, webRootPath);
        }
        return _repo.UpdateStaff(staff);
    }

    public bool DeleteStaff(int id, string webRootPath)
    {
        var staff = _repo.GetStaffById(id);
        if (staff != null && !string.IsNullOrEmpty(staff.PhotoPath))
        {
            var path = Path.Combine(webRootPath, staff.PhotoPath.TrimStart('/'));
            if (File.Exists(path)) File.Delete(path);
        }
        return _repo.DeleteStaff(id);
    }

    public StaffViewModel ToViewModel(Staff staff) => new StaffViewModel
    {
        Id = staff.Id,
        EquipeId = staff.EquipeId,
        Nom = staff.Nom,
        Prenom = staff.Prenom,
        Titre = staff.Titre,
        Description = staff.Description,
        PhotoPathActuelle = staff.PhotoPath,
        NoFiche = staff.NoFiche
    };

    private static string SavePhoto(IFormFile file, string webRootPath)
    {
        var dir = Path.Combine(webRootPath, "uploads", "staff");
        Directory.CreateDirectory(dir);
        var ext = Path.GetExtension(file.FileName);
        var nom = $"{Guid.NewGuid()}{ext}";
        using var stream = new FileStream(Path.Combine(dir, nom), FileMode.Create);
        file.CopyTo(stream);
        return $"/uploads/staff/{nom}";
    }
}
