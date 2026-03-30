using GestionEquipeSportive.Models;
using GestionEquipeSportive.ViewModels;

namespace GestionEquipeSportive.Services;

public class MatchService : IMatchService
{
    private readonly IExcelRepository _repo;

    public MatchService(IExcelRepository repo)
    {
        _repo = repo;
    }

    public List<Match> GetMatchsByEquipe(int equipeId)
        => _repo.GetMatchsByEquipe(equipeId).OrderBy(m => m.DateMatch).ToList();

    public Match? GetMatchById(int id) => _repo.GetMatchById(id);

    public Match CreateMatch(MatchViewModel vm)
    {
        var match = new Match
        {
            EquipeId = vm.EquipeId,
            DateMatch = vm.DateMatch,
            HeureArriveeVestiaire = NormaliserHeure(vm.HeureArriveeVestiaire),
            HeureDepartAutobus = vm.EstDomicile ? null : NormaliserHeure(vm.HeureDepartAutobus),
            HeureDebutMatch = NormaliserHeure(vm.HeureDebutMatch),
            EstDomicile = vm.EstDomicile,
            Adversaire = vm.Adversaire.Trim(),
            Lieu = string.IsNullOrWhiteSpace(vm.Lieu) ? null : vm.Lieu.Trim(),
            ScoreEquipe = vm.ScoreEquipe,
            ScoreAdversaire = vm.ScoreAdversaire,
            Notes = string.IsNullOrWhiteSpace(vm.Notes) ? null : vm.Notes.Trim()
        };
        return _repo.AddMatch(match);
    }

    public Match UpdateMatch(MatchViewModel vm)
    {
        var match = new Match
        {
            Id = vm.Id,
            EquipeId = vm.EquipeId,
            DateMatch = vm.DateMatch,
            HeureArriveeVestiaire = NormaliserHeure(vm.HeureArriveeVestiaire),
            HeureDepartAutobus = vm.EstDomicile ? null : NormaliserHeure(vm.HeureDepartAutobus),
            HeureDebutMatch = NormaliserHeure(vm.HeureDebutMatch),
            EstDomicile = vm.EstDomicile,
            Adversaire = vm.Adversaire.Trim(),
            Lieu = string.IsNullOrWhiteSpace(vm.Lieu) ? null : vm.Lieu.Trim(),
            ScoreEquipe = vm.ScoreEquipe,
            ScoreAdversaire = vm.ScoreAdversaire,
            Notes = string.IsNullOrWhiteSpace(vm.Notes) ? null : vm.Notes.Trim()
        };
        return _repo.UpdateMatch(match);
    }

    public bool DeleteMatch(int id)
    {
        // Supprimer les médias du match d'abord (fichiers Excel seulement)
        foreach (var media in _repo.GetMediasByMatch(id))
            _repo.DeleteMatchMedia(media.Id);
        return _repo.DeleteMatch(id);
    }

    public MatchViewModel ToViewModel(Match match) => new MatchViewModel
    {
        Id = match.Id,
        EquipeId = match.EquipeId,
        DateMatch = match.DateMatch,
        HeureArriveeVestiaire = match.HeureArriveeVestiaire,
        HeureDepartAutobus = match.HeureDepartAutobus,
        HeureDebutMatch = match.HeureDebutMatch,
        EstDomicile = match.EstDomicile,
        Adversaire = match.Adversaire,
        Lieu = match.Lieu,
        ScoreEquipe = match.ScoreEquipe,
        ScoreAdversaire = match.ScoreAdversaire,
        Notes = match.Notes
    };

    public StatistiquesMatchViewModel GetStatistiques(int equipeId)
    {
        var matchs = _repo.GetMatchsByEquipe(equipeId);
        var joues = matchs.Where(m => m.AResultat).ToList();

        return new StatistiquesMatchViewModel
        {
            MatchsJoues = joues.Count,
            Victoires = joues.Count(m => m.ScoreEquipe > m.ScoreAdversaire),
            Defaites = joues.Count(m => m.ScoreEquipe < m.ScoreAdversaire),
            Nuls = joues.Count(m => m.ScoreEquipe == m.ScoreAdversaire),
            ButsPour = joues.Sum(m => m.ScoreEquipe ?? 0),
            ButsContre = joues.Sum(m => m.ScoreAdversaire ?? 0),
            MatchsAVenir = matchs.Count(m => !m.AResultat && m.DateMatch >= DateTime.Today)
        };
    }

    public List<MatchMedia> GetMediasByMatch(int matchId)
        => _repo.GetMediasByMatch(matchId);

    public List<MatchMedia> AddMedias(int matchId, List<IFormFile> fichiers, TypeMedia type, string? description, string webRootPath)
    {
        var match = _repo.GetMatchById(matchId);
        var equipe = match != null ? _repo.GetEquipeById(match.EquipeId) : null;

        // Structure : uploads/matchs/{sport}/{annee}/{niveau}/{yyyy-MM-dd}/{photos|videos}/
        var sportSlug = equipe != null ? equipe.TypeSport.ToString().ToLower() : "autre";
        var anneeSlug = equipe?.AnneeScolaire.Replace("/", "-") ?? "inconnu";
        var niveauSlug = equipe != null ? equipe.Niveau.ToString().ToLower() : "inconnu";
        var dateSlug = match?.DateMatch.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
        var typeSlug = type == TypeMedia.Video ? "videos" : "photos";

        var dossierRelatif = Path.Combine("uploads", "matchs", sportSlug, anneeSlug, niveauSlug, dateSlug, typeSlug);
        var dossierComplet = Path.Combine(webRootPath, dossierRelatif);
        Directory.CreateDirectory(dossierComplet);

        var result = new List<MatchMedia>();
        foreach (var fichier in fichiers)
        {
            if (fichier == null || fichier.Length == 0) continue;

            var ext = Path.GetExtension(fichier.FileName);
            var nomFichier = $"{Guid.NewGuid()}{ext}";
            var cheminComplet = Path.Combine(dossierComplet, nomFichier);

            using var stream = new FileStream(cheminComplet, FileMode.Create);
            fichier.CopyTo(stream);

            var chemin = "/" + dossierRelatif.Replace(Path.DirectorySeparatorChar, '/') + "/" + nomFichier;

            var media = new MatchMedia
            {
                MatchId = matchId,
                CheminFichier = chemin,
                TypeMedia = type,
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                DateAjout = DateTime.UtcNow
            };
            result.Add(_repo.AddMatchMedia(media));
        }
        return result;
    }

    public bool DeleteMedia(int id, string webRootPath)
    {
        var media = _repo.GetMatchMediaById(id);
        if (media == null) return false;

        var cheminFichier = Path.Combine(webRootPath, media.CheminFichier.TrimStart('/'));
        if (File.Exists(cheminFichier)) File.Delete(cheminFichier);

        return _repo.DeleteMatchMedia(id);
    }

    private static string? NormaliserHeure(string? heure)
    {
        if (string.IsNullOrWhiteSpace(heure)) return null;
        // Accepter "HH:mm" ou "H:mm"
        return heure.Trim();
    }
}
