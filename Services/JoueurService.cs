using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;

namespace GestionEquipeSportive.Services;

public class JoueurService : IJoueurService
{
    private readonly IExcelRepository _repo;

    public JoueurService(IExcelRepository repo)
    {
        _repo = repo;
    }

    public List<Joueur> GetAllJoueurs() => _repo.GetAllJoueurs();

    public List<Joueur> GetHistoriqueJoueur(Joueur joueur)
    {
        // NoFiche est la clé unique permanente — sans NoFiche, pas d'historique inter-années
        if (string.IsNullOrWhiteSpace(joueur.NoFiche))
            return new List<Joueur> { joueur };

        return _repo.GetAllJoueurs()
            .Where(j => !string.IsNullOrWhiteSpace(j.NoFiche) &&
                        string.Equals(j.NoFiche.Trim(), joueur.NoFiche.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(j => j.Id)
            .ToList();
    }

    public void CopierVersEquipe(IEnumerable<int> joueurIds, int nouvelleEquipeId)
    {
        foreach (var id in joueurIds)
        {
            var source = _repo.GetJoueurById(id);
            if (source == null) continue;
            _repo.AddJoueur(new Joueur
            {
                EquipeId = nouvelleEquipeId,
                Nom = source.Nom,
                Prenom = source.Prenom,
                Numero = source.Numero,
                Position = source.Position,
                PositionSpecifique = source.PositionSpecifique,
                PhotoPath = source.PhotoPath,
                NoFiche = source.NoFiche,
                Description = source.Description
            });
        }
    }

    public List<Joueur> GetJoueursByEquipe(int equipeId) => _repo.GetJoueursByEquipe(equipeId);

    public Joueur? GetJoueurById(int id) => _repo.GetJoueurById(id);

    public Joueur CreateJoueur(JoueurViewModel vm, IFormFile? photoFile, string webRootPath)
    {
        var joueur = new Joueur
        {
            EquipeId = vm.EquipeId,
            Nom = vm.Nom,
            Prenom = vm.Prenom,
            Numero = vm.Numero,
            Position = vm.Position,
            PositionSpecifique = string.IsNullOrWhiteSpace(vm.PositionSpecifique) ? null : vm.PositionSpecifique.Trim(),
            NoFiche = string.IsNullOrWhiteSpace(vm.NoFiche) ? null : vm.NoFiche.Trim(),
            Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim()
        };

        if (photoFile != null && photoFile.Length > 0)
            joueur.PhotoPath = SaveFile(photoFile, Path.Combine(webRootPath, "uploads", "photos"));

        return _repo.AddJoueur(joueur);
    }

    public Joueur UpdateJoueur(JoueurViewModel vm, IFormFile? photoFile, string webRootPath)
    {
        var joueur = _repo.GetJoueurById(vm.Id) ?? new Joueur();
        joueur.Id = vm.Id;
        joueur.EquipeId = vm.EquipeId;
        joueur.Nom = vm.Nom;
        joueur.Prenom = vm.Prenom;
        joueur.Numero = vm.Numero;
        joueur.Position = vm.Position;
        joueur.PositionSpecifique = string.IsNullOrWhiteSpace(vm.PositionSpecifique) ? null : vm.PositionSpecifique.Trim();
        joueur.NoFiche = string.IsNullOrWhiteSpace(vm.NoFiche) ? null : vm.NoFiche.Trim();
        joueur.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();

        if (photoFile != null && photoFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(joueur.PhotoPath))
            {
                var oldPath = Path.Combine(webRootPath, joueur.PhotoPath.TrimStart('/'));
                if (File.Exists(oldPath)) File.Delete(oldPath);
            }
            joueur.PhotoPath = SaveFile(photoFile, Path.Combine(webRootPath, "uploads", "photos"));
        }

        return _repo.UpdateJoueur(joueur);
    }

    public bool DeleteJoueur(int id, string webRootPath)
    {
        var joueur = _repo.GetJoueurById(id);
        if (joueur != null && !string.IsNullOrEmpty(joueur.PhotoPath))
        {
            var photoPath = Path.Combine(webRootPath, joueur.PhotoPath.TrimStart('/'));
            if (File.Exists(photoPath)) File.Delete(photoPath);
        }
        return _repo.DeleteJoueur(id);
    }

    public JoueurViewModel ToViewModel(Joueur joueur) => new JoueurViewModel
    {
        Id = joueur.Id,
        EquipeId = joueur.EquipeId,
        Nom = joueur.Nom,
        Prenom = joueur.Prenom,
        Numero = joueur.Numero,
        Position = joueur.Position,
        PositionSpecifique = joueur.PositionSpecifique,
        NoFiche = joueur.NoFiche,
        Description = joueur.Description,
        PhotoPathActuelle = joueur.PhotoPath
    };

    private static string SaveFile(IFormFile file, string directory)
    {
        Directory.CreateDirectory(directory);
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(directory, fileName);
        using var stream = new FileStream(fullPath, FileMode.Create);
        file.CopyTo(stream);
        return $"/uploads/photos/{fileName}";
    }
}
