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
    private readonly IWebHostEnvironment _env;

    public PublicController(
        IEcoleService ecoleService,
        IEquipeService equipeService,
        IStaffService staffService,
        IJoueurService joueurService,
        IMatchService matchService,
        IWebHostEnvironment env)
    {
        _ecoleService = ecoleService;
        _equipeService = equipeService;
        _staffService = staffService;
        _joueurService = joueurService;
        _matchService = matchService;
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
                "soccer" => TypeSport.Soccer,
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

        List<MatchMedia> dernierMatchMedias = new();
        MatchMedia? photoBanniere = null;
        if (dernierMatch != null)
        {
            dernierMatchMedias = _matchService.GetMediasByMatch(dernierMatch.Id);
            var photos = dernierMatchMedias.Where(m => m.TypeMedia == TypeMedia.Photo).ToList();
            if (photos.Any())
                photoBanniere = photos[Random.Shared.Next(photos.Count)];
        }

        // Prochain match dans les 7 jours
        var now = DateTime.Now;
        var prochain = matchs
            .Where(m => !m.AResultat && m.DateMatch >= now && m.DateMatch <= now.AddDays(7))
            .OrderBy(m => m.DateMatch)
            .FirstOrDefault();

        var vm = new PublicEquipeViewModel
        {
            Equipe = equipe,
            Ecole = ecole,
            Staff = _staffService.GetStaffByEquipe(equipe.Id),
            Joueurs = _joueurService.GetJoueursByEquipe(equipe.Id),
            Matchs = matchs,
            Stats = stats,
            DernierMatchAvecResultat = dernierMatch,
            DernierMatchMedias = dernierMatchMedias,
            PhotoBanniere = photoBanniere,
            ProchainMatch = prochain,
            SportDisplay = typeSport switch
            {
                TypeSport.FootballAmericain => "Football",
                TypeSport.Soccer => "Soccer",
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

        return View(vm);
    }
}
