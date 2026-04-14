using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;

namespace GestionEquipeSportive.Services;

public class JoueurService : IJoueurService
{
    private readonly IExcelRepository _repo;

    public JoueurService(IExcelRepository repo) => _repo = repo;

    // ── Joueurs (niveau école) ────────────────────────────────────────────────
    public List<Joueur> GetAllJoueurs() => _repo.GetAllJoueurs();
    public List<Joueur> GetJoueursByEcole(int ecoleId) => _repo.GetJoueursByEcole(ecoleId);
    public Joueur? GetJoueurById(int id) => _repo.GetJoueurById(id);

    public Joueur CreateJoueur(JoueurViewModel vm)
    {
        var joueur = new Joueur
        {
            EcoleId = vm.EcoleId,
            Nom = vm.Nom.Trim(),
            Prenom = vm.Prenom.Trim(),
            NoFiche = string.IsNullOrWhiteSpace(vm.NoFiche) ? null : vm.NoFiche.Trim(),
            CleUnique = Guid.NewGuid(),
            ConsentementPhoto = vm.ConsentementPhoto,
            Actif = vm.Actif
        };
        return _repo.AddJoueur(joueur);
    }

    public Joueur UpdateJoueur(JoueurViewModel vm)
    {
        var joueur = _repo.GetJoueurById(vm.Id) ?? new Joueur();
        var etaitActif = joueur.Actif;
        joueur.Id = vm.Id;
        joueur.EcoleId = vm.EcoleId;
        joueur.Nom = vm.Nom.Trim();
        joueur.Prenom = vm.Prenom.Trim();
        joueur.NoFiche = string.IsNullOrWhiteSpace(vm.NoFiche) ? null : vm.NoFiche.Trim();
        joueur.CleUnique ??= Guid.NewGuid();
        joueur.ConsentementPhoto = vm.ConsentementPhoto;
        joueur.Actif = vm.Actif;
        _repo.UpdateJoueur(joueur);

        // Cascade : si on désactive le joueur, désactiver toutes ses assignations
        if (etaitActif && !vm.Actif)
        {
            var assignations = _repo.GetJoueurEquipesByJoueur(vm.Id);
            foreach (var je in assignations)
            {
                je.Actif = false;
                _repo.UpdateJoueurEquipe(je);
            }
        }

        return joueur;
    }

    public bool DeleteJoueur(int id, string webRootPath)
    {
        // Supprimer photos des assignations
        var assignations = _repo.GetJoueurEquipesByJoueur(id);
        foreach (var je in assignations)
        {
            if (!string.IsNullOrEmpty(je.PhotoPath))
            {
                var p = Path.Combine(webRootPath, je.PhotoPath.TrimStart('/'));
                if (File.Exists(p)) File.Delete(p);
            }
        }
        _repo.DeleteJoueurEquipesByJoueur(id);
        return _repo.DeleteJoueur(id);
    }

    public JoueurViewModel ToViewModel(Joueur joueur) => new JoueurViewModel
    {
        Id = joueur.Id,
        EcoleId = joueur.EcoleId,
        Nom = joueur.Nom,
        Prenom = joueur.Prenom,
        NoFiche = joueur.NoFiche,
        ConsentementPhoto = joueur.ConsentementPhoto,
        Actif = joueur.Actif,
        CleUnique = joueur.CleUnique
    };

    // ── JoueurEquipes (assignations) ─────────────────────────────────────────
    public List<JoueurEquipe> GetAllJoueurEquipes() => _repo.GetAllJoueurEquipes();

    public List<JoueurEquipe> GetJoueurEquipesByEquipe(int equipeId)
    {
        var jes = _repo.GetJoueurEquipesByEquipe(equipeId);
        foreach (var je in jes)
            je.Joueur = _repo.GetJoueurById(je.JoueurId);
        return jes;
    }

    public List<JoueurEquipe> GetJoueurEquipesByJoueur(int joueurId)
    {
        var jes = _repo.GetJoueurEquipesByJoueur(joueurId);
        return jes;
    }

    public JoueurEquipe? GetJoueurEquipeById(int id)
    {
        var je = _repo.GetJoueurEquipeById(id);
        if (je != null) je.Joueur = _repo.GetJoueurById(je.JoueurId);
        return je;
    }

    public JoueurEquipe AssignerAEquipe(JoueurEquipeViewModel vm, IFormFile? photoFile, string webRootPath)
    {
        var je = new JoueurEquipe
        {
            JoueurId = vm.JoueurId,
            EquipeId = vm.EquipeId,
            Position = vm.PositionPrincipale.Trim(),
            PositionSpecifique = string.IsNullOrWhiteSpace(vm.PositionPairsRaw) ? null : vm.PositionPairsRaw.Trim(),
            Numero = vm.Numero,
            Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim(),
            Actif = vm.Actif
        };
        if (photoFile != null && photoFile.Length > 0)
            je.PhotoPath = SaveFile(photoFile, Path.Combine(webRootPath, "uploads", "photos"));
        return _repo.AddJoueurEquipe(je);
    }

    public JoueurEquipe UpdateAssignation(JoueurEquipeViewModel vm, IFormFile? photoFile, string webRootPath)
    {
        var je = _repo.GetJoueurEquipeById(vm.Id) ?? new JoueurEquipe();
        je.Id = vm.Id;
        je.JoueurId = vm.JoueurId;
        je.EquipeId = vm.EquipeId;
        je.Position = vm.PositionPrincipale.Trim();
        je.PositionSpecifique = string.IsNullOrWhiteSpace(vm.PositionPairsRaw) ? null : vm.PositionPairsRaw.Trim();
        je.Numero = vm.Numero;
        je.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
        je.Actif = vm.Actif;
        if (photoFile != null && photoFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(je.PhotoPath))
            {
                var old = Path.Combine(webRootPath, je.PhotoPath.TrimStart('/'));
                if (File.Exists(old)) File.Delete(old);
            }
            je.PhotoPath = SaveFile(photoFile, Path.Combine(webRootPath, "uploads", "photos"));
        }
        return _repo.UpdateJoueurEquipe(je);
    }

    public bool SupprimerAssignation(int joueurEquipeId, string webRootPath)
    {
        var je = _repo.GetJoueurEquipeById(joueurEquipeId);
        if (je != null && !string.IsNullOrEmpty(je.PhotoPath))
        {
            var p = Path.Combine(webRootPath, je.PhotoPath.TrimStart('/'));
            if (File.Exists(p)) File.Delete(p);
        }
        return _repo.DeleteJoueurEquipe(joueurEquipeId);
    }

    public JoueurEquipeViewModel ToAssignationViewModel(JoueurEquipe je) => new JoueurEquipeViewModel
    {
        Id = je.Id,
        JoueurId = je.JoueurId,
        EquipeId = je.EquipeId,
        PositionPrincipale = je.Position,
        PositionPairsRaw = je.PositionSpecifique ?? "",
        Numero = je.Numero,
        Description = je.Description,
        PhotoPathActuelle = je.PhotoPath,
        Actif = je.Actif,
        NomJoueur = je.Joueur != null ? $"{je.Joueur.Prenom} {je.Joueur.Nom}" : null
    };

    // ── Médias ────────────────────────────────────────────────────────────────
    public List<JoueurMedia> GetMediasByJoueur(int joueurId) => _repo.GetMediasByJoueur(joueurId);

    public JoueurMedia AddJoueurMedia(int joueurId, IFormFile file, string webRootPath)
    {
        var dir = Path.Combine(webRootPath, "uploads", "joueurs");
        Directory.CreateDirectory(dir);
        var ext = Path.GetExtension(file.FileName);
        var nom = $"{Guid.NewGuid()}{ext}";
        using var stream = new FileStream(Path.Combine(dir, nom), FileMode.Create);
        file.CopyTo(stream);
        return _repo.AddJoueurMedia(new JoueurMedia
        {
            JoueurId = joueurId,
            CheminFichier = $"/uploads/joueurs/{nom}",
            DateAjout = DateTime.UtcNow
        });
    }

    public bool DeleteJoueurMedia(int id, string webRootPath)
    {
        var medias = _repo.GetAllJoueurMedias();
        var media = medias.FirstOrDefault(m => m.Id == id);
        if (media != null && !string.IsNullOrEmpty(media.CheminFichier))
        {
            var p = Path.Combine(webRootPath, media.CheminFichier.TrimStart('/'));
            if (File.Exists(p)) File.Delete(p);
        }
        return _repo.DeleteJoueurMedia(id);
    }

    private static string SaveFile(IFormFile file, string dir)
    {
        Directory.CreateDirectory(dir);
        var ext = Path.GetExtension(file.FileName);
        var nom = $"{Guid.NewGuid()}{ext}";
        using var stream = new FileStream(Path.Combine(dir, nom), FileMode.Create);
        file.CopyTo(stream);
        return $"/uploads/photos/{nom}";
    }
}
