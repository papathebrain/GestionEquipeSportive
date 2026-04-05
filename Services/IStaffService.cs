using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;

namespace GestionEquipeSportive.Services;

public interface IStaffService
{
    List<Staff> GetStaffByEquipe(int equipeId);
    Staff? GetStaffById(int id);
    List<Staff> GetStaffByNoFiche(string noFiche);
    void CopierVersEquipe(IEnumerable<int> staffIds, int nouvelleEquipeId);
    Staff CreateStaff(StaffViewModel vm, IFormFile? photoFile, string webRootPath);
    Staff UpdateStaff(StaffViewModel vm, IFormFile? photoFile, string webRootPath);
    bool DeleteStaff(int id, string webRootPath);
    StaffViewModel ToViewModel(Staff staff);
}
