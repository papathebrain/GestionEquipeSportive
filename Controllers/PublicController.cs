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

    // GET /p/{ecoleSlug}/{annee}/{sport}/{niveau}
    [Route("p/{ecoleSlug}/{annee}/{sport}/{niveau}")]
    public IActionResult Equipe(string ecoleSlug, string annee, string sport, string niveau)
    {
        // Trouver l'école par slug
        var ecoles = _ecoleService.GetAllEcoles();
        var ecole = ecoles.FirstOrDefault(e =>
            string.Equals(Ecole.ToSlug(e.Nom), ecoleSlug, StringComparison.OrdinalIgnoreCase));

        if (ecole == null)
            return NotFound("École introuvable.");

        // Parser le sport
        if (!Enum.TryParse<TypeSport>(sport, ignoreCase: true, out var typeSport))
        {
            // Essayer avec les noms d'affichage
            typeSport = sport.ToLowerInvariant() switch
            {
                "football" or "footballamericain" or "football-americain" => TypeSport.FootballAmericain,
                "flagfootball" or "flag-football" or "flag" => TypeSport.FlagFootball,
                "soccer" => TypeSport.Soccer,
                "volleyball" => TypeSport.Volleyball,
                "hockey" => TypeSport.Hockey,
                _ => (TypeSport)(-1)
            };
            if ((int)typeSport == -1)
                return NotFound("Sport introuvable.");
        }

        // Parser le niveau
        if (!Enum.TryParse<NiveauEquipe>(niveau, ignoreCase: true, out var niveauEquipe))
        {
            niveauEquipe = niveau.ToLowerInvariant() switch
            {
                "juvenil" or "juvenile" or "juvénile" => NiveauEquipe.Juvenil,
                "peewee" or "pee-wee" => NiveauEquipe.PeeWee,
                "benjamin" => NiveauEquipe.Benjamin,
                "cadet" => NiveauEquipe.Cadet,
                "atome" => NiveauEquipe.Atome,
                "bantam" => NiveauEquipe.Bantam,
                _ => (NiveauEquipe)(-1)
            };
            if ((int)niveauEquipe == -1)
                return NotFound("Niveau introuvable.");
        }

        // Trouver l'équipe
        var equipes = _equipeService.GetEquipesByEcole(ecole.Id);
        var equipe = equipes.FirstOrDefault(e =>
            e.TypeSport == typeSport &&
            e.Niveau == niveauEquipe &&
            string.Equals(e.AnneeScolaire, annee, StringComparison.OrdinalIgnoreCase));

        if (equipe == null)
            return NotFound("Équipe introuvable.");

        if (!equipe.AfficherPublic)
            return NotFound("Cette page n'est pas disponible publiquement.");

        // Charger les données
        var matchs = _matchService.GetMatchsByEquipe(equipe.Id)
            .OrderBy(m => m.DateMatch)
            .ToList();

        var stats = _matchService.GetStatistiques(equipe.Id);

        // Dernier match avec résultat
        var dernierMatch = matchs
            .Where(m => m.AResultat)
            .OrderByDescending(m => m.DateMatch)
            .FirstOrDefault();

        // Charger toutes les photos des matchs passés avec résultat
        // Charger toutes les médias (photos + vidéos) des matchs passés
        var matchesMedias = matchs
            .Where(m => m.AResultat)
            .ToDictionary(
                m => m.Id,
                m => _matchService.GetMediasByMatch(m.Id).ToList());

        List<MatchMedia> dernierMatchMedias = new();
        MatchMedia? photoBanniere = null;
        if (dernierMatch != null)
        {
            dernierMatchMedias = matchesMedias.TryGetValue(dernierMatch.Id, out var dm) ? dm : new();
            // La bannière utilise une photo (pas une vidéo)
            var photosSeules = dernierMatchMedias.Where(mm => mm.TypeMedia == TypeMedia.Photo).ToList();
            if (photosSeules.Any())
                photoBanniere = photosSeules[Random.Shared.Next(photosSeules.Count)];
        }

        // Prochain match dans les 7 jours
        var now = DateTime.Now;
        var prochain = matchs
            .Where(m => !m.AResultat && m.DateMatch >= now && m.DateMatch <= now.AddDays(7))
            .OrderBy(m => m.DateMatch)
            .FirstOrDefault();

        var joueurs = _joueurService.GetJoueursByEquipe(equipe.Id, actifSeulement: true);
        var joueurMedias = joueurs.ToDictionary(
            j => j.Id,
            j => _joueurService.GetMediasByJoueur(j.Id));

        // Stats par joueur : matchs joués = total matchs avec résultat, absences = entrées dans AbsenceMatch
        var matchsAvecResultat = matchs.Where(m => m.AResultat).ToList();
        var toutesAbsences = matchsAvecResultat
            .SelectMany(m => _matchService.GetAbsencesByMatch(m.Id))
            .ToList();
        var statsJoueurs = joueurs.ToDictionary(
            j => j.Id,
            j => (
                MatchsJoues: matchsAvecResultat.Count,
                Absences: toutesAbsences.Count(a => a.JoueurId == j.Id)
            ));

        // Dernier match avec photos pour sélection auto galerie
        var dernierMatchAvecPhotosId = matchesMedias
            .Where(kvp => kvp.Value.Any())
            .OrderByDescending(kvp => matchs.FirstOrDefault(m => m.Id == kvp.Key)?.DateMatch ?? DateTime.MinValue)
            .Select(kvp => (int?)kvp.Key)
            .FirstOrDefault();

        var vm = new PublicEquipeViewModel
        {
            Equipe = equipe,
            Ecole = ecole,
            Staff = _staffService.GetStaffByEquipe(equipe.Id),
            Joueurs = joueurs,
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
            SportDisplay = typeSport switch
            {
                TypeSport.FootballAmericain => "Football",
                TypeSport.FlagFootball => "Flag Football",
                TypeSport.Soccer => "Soccer",
                TypeSport.Volleyball => "Volleyball",
                TypeSport.Hockey => "Hockey",
                _ => typeSport.ToString()
            },
            NiveauDisplay = niveauEquipe switch
            {
                NiveauEquipe.Juvenil => "Juvénile",
                NiveauEquipe.PeeWee => "Pee-Wee",
                _ => niveauEquipe.ToString()
            }
        };

        // Autres équipes publiques de la même école
        vm.AutresEquipesEcole = equipes
            .Where(e => e.Id != equipe.Id && e.AfficherPublic)
            .Select(e => (
                Equipe: e,
                Url: $"/p/{Ecole.ToSlug(ecole.Nom)}/{e.AnneeScolaire}/{e.TypeSport.ToString().ToLower()}/{e.Niveau.ToString().ToLower()}"
            ))
            .ToList();

        return View(vm);
    }
}
