using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

public class PublicController : Controller
{
    private readonly IEcoleService _ecoleService;
    private readonly IEquipeService _equipeService;
    private readonly IStaffService _staffService;
    private readonly IJoueurService _joueurService;
    private readonly IMatchService _matchService;
    private readonly IEvenementService _evenementService;
    private readonly IWebHostEnvironment _env;

    public PublicController(
        IEcoleService ecoleService,
        IEquipeService equipeService,
        IStaffService staffService,
        IJoueurService joueurService,
        IMatchService matchService,
        IEvenementService evenementService,
        IWebHostEnvironment env)
    {
        _ecoleService = ecoleService;
        _equipeService = equipeService;
        _staffService = staffService;
        _joueurService = joueurService;
        _matchService = matchService;
        _evenementService = evenementService;
        _env = env;
    }

    // GET /p/{ecoleSlug}/{annee}/{sport}/{nomEquipe}
    [Route("p/{ecoleSlug}/{annee}/{sport}/{nomEquipe}")]
    public IActionResult Equipe(string ecoleSlug, string annee, string sport, string nomEquipe)
    {
        var vm = BuildViewModel(ecoleSlug, annee, sport, nomEquipe, out var erreur);
        if (erreur != null) return erreur;
        return View(vm);
    }

    // GET /p/{ecoleSlug}/{annee}/{sport}/{nomEquipe}/joueurs2
    [Route("p/{ecoleSlug}/{annee}/{sport}/{nomEquipe}/joueurs2")]
    public IActionResult Joueur2(string ecoleSlug, string annee, string sport, string nomEquipe)
    {
        var vm = BuildViewModel(ecoleSlug, annee, sport, nomEquipe, out var erreur);
        if (erreur != null) return erreur;
        return View(vm);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private PublicEquipeViewModel? BuildViewModel(
        string ecoleSlug, string annee, string sport, string nomEquipe,
        out IActionResult? erreur)
    {
        erreur = null;

        // Trouver l'école par slug
        var ecoles = _ecoleService.GetAllEcoles();
        var ecole = ecoles.FirstOrDefault(e =>
            string.Equals(Ecole.ToSlug(e.Nom), ecoleSlug, StringComparison.OrdinalIgnoreCase));

        if (ecole == null) { erreur = NotFound("École introuvable."); return null; }

        // Parser le sport (pour filtrage optionnel, on garde la tolérance)
        if (!Enum.TryParse<TypeSport>(sport, ignoreCase: true, out var typeSport))
        {
            typeSport = sport.ToLowerInvariant() switch
            {
                "football" or "footballamericain" or "football-americain" => TypeSport.FootballAmericain,
                "flagfootball" or "flag-football" or "flag" => TypeSport.FlagFootball,
                "soccer" => TypeSport.Soccer,
                "volleyball" => TypeSport.Volleyball,
                "hockey" => TypeSport.Hockey,
                _ => (TypeSport)(-1)
            };
            if ((int)typeSport == -1) { erreur = NotFound("Sport introuvable."); return null; }
        }

        // Trouver l'équipe par annee + sport + slug du nom
        var equipes = _equipeService.GetEquipesByEcole(ecole.Id);
        var equipe = equipes.FirstOrDefault(e =>
            e.TypeSport == typeSport &&
            string.Equals(e.AnneeScolaire, annee, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(e.NomSlug, nomEquipe, StringComparison.OrdinalIgnoreCase));

        if (equipe == null) { erreur = NotFound("Équipe introuvable."); return null; }
        if (!equipe.AfficherPublic) { erreur = NotFound("Cette page n'est pas disponible publiquement."); return null; }

        // Charger les données
        var matchs = _matchService.GetMatchsByEquipe(equipe.Id)
            .OrderBy(m => m.DateMatch).ToList();

        var stats = _matchService.GetStatistiques(equipe.Id);

        var dernierMatch = matchs
            .Where(m => m.AResultat).OrderByDescending(m => m.DateMatch).FirstOrDefault();

        var matchesMedias = matchs
            .Where(m => m.AResultat)
            .ToDictionary(m => m.Id, m => _matchService.GetMediasByMatch(m.Id).ToList());

        List<MatchMedia> dernierMatchMedias = new();
        MatchMedia? photoBanniere = null;
        if (dernierMatch != null)
        {
            dernierMatchMedias = matchesMedias.TryGetValue(dernierMatch.Id, out var dm) ? dm : new();
            var photosSeules = dernierMatchMedias.Where(mm => mm.TypeMedia == TypeMedia.Photo).ToList();
            if (photosSeules.Any())
                photoBanniere = photosSeules[Random.Shared.Next(photosSeules.Count)];
        }

        // Fallback bannière : photo aléatoire d'une autre équipe publique du même sport
        if (photoBanniere == null)
        {
            var autresEquipesMémeSport = _equipeService.GetAllEquipes()
                .Where(e => e.Id != equipe.Id && e.AfficherPublic && e.TypeSport == typeSport)
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            foreach (var autreEquipe in autresEquipesMémeSport)
            {
                var autresPhotos = _matchService.GetMatchsByEquipe(autreEquipe.Id)
                    .Where(m => m.AResultat)
                    .SelectMany(m => _matchService.GetMediasByMatch(m.Id))
                    .Where(mm => mm.TypeMedia == TypeMedia.Photo)
                    .ToList();
                if (autresPhotos.Any())
                {
                    photoBanniere = autresPhotos[Random.Shared.Next(autresPhotos.Count)];
                    break;
                }
            }
        }

        var now = DateTime.Now;
        var prochain = matchs
            .Where(m => !m.AResultat && m.DateMatch >= now && m.DateMatch <= now.AddDays(7))
            .OrderBy(m => m.DateMatch).FirstOrDefault();

        var joueurEquipes = _joueurService.GetJoueurEquipesByEquipe(equipe.Id).Where(je => je.Actif).ToList();
        var joueurMedias = joueurEquipes.ToDictionary(je => je.JoueurId, je => _joueurService.GetMediasByJoueur(je.JoueurId));

        var matchsAvecResultat = matchs.Where(m => m.AResultat).ToList();
        var toutesAbsences = matchsAvecResultat
            .SelectMany(m => _matchService.GetAbsencesByMatch(m.Id)).ToList();
        var statsJoueurs = joueurEquipes.ToDictionary(
            je => je.JoueurId,
            je => (MatchsJoues: matchsAvecResultat.Count, Absences: toutesAbsences.Count(a => a.JoueurId == je.JoueurId)));

        var dernierMatchAvecPhotosId = matchesMedias
            .Where(kvp => kvp.Value.Any())
            .OrderByDescending(kvp => matchs.FirstOrDefault(m => m.Id == kvp.Key)?.DateMatch ?? DateTime.MinValue)
            .Select(kvp => (int?)kvp.Key).FirstOrDefault();

        ThemeEcole? theme = equipe.ThemeId.HasValue
            ? _ecoleService.GetThemeById(equipe.ThemeId.Value)
            : null;

        // Logos des équipes adverses liées
        var adversaireLogos = matchs
            .Where(m => m.AdversaireId.HasValue)
            .Select(m => m.AdversaireId!.Value)
            .Distinct()
            .Select(aid => _ecoleService.GetEquipeAdverseById(aid))
            .Where(a => a != null && !string.IsNullOrEmpty(a.LogoPath))
            .ToDictionary(a => a!.Id, a => a!.LogoPath!);

        var vm = new PublicEquipeViewModel
        {
            Equipe = equipe,
            Ecole = ecole,
            Theme = theme,
            Staff = _staffService.GetStaffByEquipe(equipe.Id),
            Joueurs = joueurEquipes,
            Matchs = matchs,
            Evenements = _evenementService.GetEvenementsByEquipe(equipe.Id),
            Stats = stats,
            DernierMatchAvecResultat = dernierMatch,
            DernierMatchMedias = dernierMatchMedias,
            MatchesMedias = matchesMedias,
            JoueurMedias = joueurMedias,
            StatsJoueurs = statsJoueurs,
            DernierMatchAvecPhotosId = dernierMatchAvecPhotosId,
            PhotoBanniere = photoBanniere,
            ProchainMatch = prochain,
            AdversaireLogos = adversaireLogos,
            SportDisplay = typeSport switch
            {
                TypeSport.FootballAmericain => "Football",
                TypeSport.FlagFootball => "Flag Football",
                TypeSport.Soccer => "Soccer",
                TypeSport.Volleyball => "Volleyball",
                TypeSport.Hockey => "Hockey",
                _ => typeSport.ToString()
            },
            NiveauDisplay = equipe.Niveau switch
            {
                NiveauEquipe.Juvenil => "Juvénile",
                NiveauEquipe.PeeWee => "Pee-Wee",
                _ => equipe.Niveau.ToString()
            }
        };

        vm.AutresEquipesEcole = equipes
            .Where(e => e.Id != equipe.Id && e.AfficherPublic)
            .Select(e => (
                Equipe: e,
                Url: $"/p/{Ecole.ToSlug(ecole.Nom)}/{e.AnneeScolaire}/{e.TypeSport.ToString().ToLower()}/{e.NomSlug}"
            )).ToList();

        return vm;
    }
}
