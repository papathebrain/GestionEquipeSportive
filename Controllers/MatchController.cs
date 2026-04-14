using ClosedXML.Excel;
using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class MatchController : Controller
{
    private readonly IMatchService _matchService;
    private readonly IEquipeService _equipeService;
    private readonly IEcoleService _ecoleService;
    private readonly IEcoleAccessService _access;
    private readonly IWebHostEnvironment _env;
    private readonly IJoueurService _joueurService;

    public MatchController(IMatchService matchService, IEquipeService equipeService,
        IEcoleService ecoleService, IEcoleAccessService access, IWebHostEnvironment env,
        IJoueurService joueurService)
    {
        _matchService = matchService;
        _equipeService = equipeService;
        _ecoleService = ecoleService;
        _access = access;
        _env = env;
        _joueurService = joueurService;
    }

    // ─── Index ────────────────────────────────────────────────────────────────

    public IActionResult Index(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        var matchs = _matchService.GetMatchsByEquipe(equipeId);
        var stats = _matchService.GetStatistiques(equipeId);

        ViewBag.Equipe = equipe;
        ViewBag.Ecole = ecole;
        ViewBag.Stats = stats;
        ViewBag.PeutModifier = _access.PeutModifierEquipe(User, equipeId, equipe.EcoleId);

        return View(matchs);
    }



    // ─── Create ───────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Create(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();

        if (!_access.PeutModifierEquipe(User, equipeId, equipe.EcoleId)) return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        var vm = new MatchViewModel
        {
            EquipeId = equipeId,
            EcoleId = equipe.EcoleId,
            NomEquipe = equipe.Nom,
            NomEcole = ecole?.Nom,
            DateMatch = DateTime.Today
        };
        vm.EquipesAdversesDisponibles = _ecoleService.GetEquipesAdversesByEcoleSport(equipe.EcoleId, equipe.TypeSport.ToString());
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(MatchViewModel vm)
    {
        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, vm.EquipeId, equipe.EcoleId)) return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = _ecoleService.GetEcoleById(equipe.EcoleId);
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEquipe = equipe.Nom;
            vm.NomEcole = ecole2?.Nom;
            vm.EcoleId = equipe.EcoleId;
            vm.EquipesAdversesDisponibles = _ecoleService.GetEquipesAdversesByEcoleSport(equipe.EcoleId, equipe.TypeSport.ToString());
            return View(vm);
        }

        // Pré-remplir adversaire/lieu depuis le dictionnaire si sélectionné
        if (vm.AdversaireId.HasValue)
        {
            var adv = _ecoleService.GetEquipeAdverseById(vm.AdversaireId.Value);
            if (adv != null)
            {
                vm.Adversaire = adv.Nom;
                if (string.IsNullOrWhiteSpace(vm.Lieu) && !string.IsNullOrEmpty(adv.Lieu))
                    vm.Lieu = adv.Lieu;
            }
        }

        var match = _matchService.CreateMatch(vm);
        TempData["Success"] = $"Match contre {match.Adversaire} ajouté avec succès.";
        return RedirectToAction("Details", "Equipe", new { id = vm.EquipeId, tab = "matchs" });
    }

    // ─── Edit ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Edit(int id, string? tab = null)
    {
        var match = _matchService.GetMatchById(id);
        if (match == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(match.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        var vm = _matchService.ToViewModel(match);
        vm.NomEquipe = equipe.Nom;
        vm.NomEcole = ecole?.Nom;
        vm.EcoleId = equipe.EcoleId;
        vm.EquipesAdversesDisponibles = _ecoleService.GetEquipesAdversesByEcoleSport(equipe.EcoleId, equipe.TypeSport.ToString());

        ViewBag.Medias = _matchService.GetMediasByMatch(id);
        ViewBag.Joueurs = _joueurService.GetJoueurEquipesByEquipe(match.EquipeId);
        ViewBag.Absences = _matchService.GetAbsencesByMatch(id);
        ViewBag.ActiveTab = tab ?? "info";

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, MatchViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = _ecoleService.GetEcoleById(equipe.EcoleId);
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEquipe = equipe.Nom;
            vm.NomEcole = ecole2?.Nom;
            vm.EcoleId = equipe.EcoleId;
            vm.EquipesAdversesDisponibles = _ecoleService.GetEquipesAdversesByEcoleSport(equipe.EcoleId, equipe.TypeSport.ToString());
            ViewBag.Medias = _matchService.GetMediasByMatch(id);
            ViewBag.Joueurs = _joueurService.GetJoueurEquipesByEquipe(vm.EquipeId);
            ViewBag.Absences = _matchService.GetAbsencesByMatch(id);
            return View(vm);
        }

        // Pré-remplir adversaire/lieu depuis le dictionnaire si sélectionné
        if (vm.AdversaireId.HasValue)
        {
            var adv = _ecoleService.GetEquipeAdverseById(vm.AdversaireId.Value);
            if (adv != null)
                vm.Adversaire = adv.Nom;
        }

        _matchService.UpdateMatch(vm);
        TempData["Success"] = "Match modifié avec succès.";
        return RedirectToAction("Details", "Equipe", new { id = vm.EquipeId, tab = "matchs" });
    }

    // ─── Delete ───────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id, int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipeId, equipe.EcoleId)) return Forbid();

        // Supprimer les fichiers médias
        foreach (var media in _matchService.GetMediasByMatch(id))
            _matchService.DeleteMedia(media.Id, _env.WebRootPath);

        _matchService.DeleteMatch(id);
        TempData["Success"] = "Match supprimé.";
        return RedirectToAction(nameof(Index), new { equipeId });
    }

    // ─── Médias ───────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjouterMedia(int matchId, List<IFormFile> fichiers, string typeMedia, string? description)
    {
        var match = _matchService.GetMatchById(matchId);
        if (match == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(match.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        var valides = fichiers?.Where(f => f != null && f.Length > 0).ToList() ?? new();
        if (!valides.Any())
        {
            TempData["Error"] = "Veuillez sélectionner au moins un fichier.";
            return RedirectToAction(nameof(Edit), new { id = matchId, tab = "galerie" });
        }

        var type = typeMedia == "Video" ? TypeMedia.Video : TypeMedia.Photo;
        var ajoutees = _matchService.AddMedias(matchId, valides, type, description, _env.WebRootPath);
        TempData["Success"] = ajoutees.Count == 1 ? "Média ajouté." : $"{ajoutees.Count} médias ajoutés.";
        return RedirectToAction(nameof(Edit), new { id = matchId, tab = "galerie" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SupprimerMedia(int id, int matchId)
    {
        var match = _matchService.GetMatchById(matchId);
        if (match == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(match.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        _matchService.DeleteMedia(id, _env.WebRootPath);
        TempData["Success"] = "Média supprimé.";
        return RedirectToAction(nameof(Edit), new { id = matchId, tab = "galerie" });
    }

    // ─── Absences ─────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleAbsence(int matchId, int joueurId)
    {
        var match = _matchService.GetMatchById(matchId);
        if (match == null) return NotFound();
        var equipe = _equipeService.GetEquipeById(match.EquipeId);
        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0))
            return Forbid();
        _matchService.ToggleAbsence(matchId, joueurId);
        return RedirectToAction(nameof(Edit), new { id = matchId, tab = "presences" });
    }

    // ─── Import calendrier Excel ──────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ImporterCalendrier(int equipeId, IFormFile fichierExcel)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipeId, equipe.EcoleId)) return Forbid();

        if (fichierExcel == null || fichierExcel.Length == 0)
        {
            TempData["Error"] = "Veuillez sélectionner un fichier Excel.";
            return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "calendrier" });
        }

        int importes = 0, erreurs = 0;

        try
        {
            using var stream = fichierExcel.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            if (lastRow < 2)
            {
                TempData["Error"] = "Le fichier est vide ou ne contient pas de données.";
                return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "calendrier" });
            }

            // Détection des colonnes par en-tête
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var headerRow = ws.Row(1);
            for (int c = 1; c <= ws.LastColumnUsed()!.ColumnNumber(); c++)
            {
                var h = headerRow.Cell(c).GetString().Trim();
                if (!string.IsNullOrEmpty(h) && !headers.ContainsKey(h))
                    headers[h] = c;
            }

            int Col(params string[] names)
            {
                foreach (var n in names)
                    if (headers.TryGetValue(n, out var idx)) return idx;
                return -1;
            }

            int cDate      = Col("Date", "date");
            int cAdversaire= Col("Adversaire", "adversaire", "Équipe adverse", "Equipe adverse");
            int cDomicile  = Col("Domicile", "domicile", "Dom/Ext", "dom/ext");
            int cLieu      = Col("Lieu", "lieu", "Endroit");
            int cHDebut    = Col("Heure début", "Heure debut", "heure debut", "Heure", "heure");
            int cHArrivee  = Col("Heure arrivée vestiaire", "Heure arrivee vestiaire", "heure arrivee", "Heure vestiaire");
            int cHDepart   = Col("Heure départ autobus", "Heure depart autobus", "heure depart", "Heure bus");
            int cNotes     = Col("Notes", "notes", "Note", "note");

            if (cDate == -1 || cAdversaire == -1)
            {
                TempData["Error"] = "Colonnes obligatoires introuvables : « Date » et « Adversaire » sont requis.";
                return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "calendrier" });
            }

            for (int row = 2; row <= lastRow; row++)
            {
                var dateStr = ws.Cell(row, cDate).GetString().Trim();
                var adversaire = cAdversaire > 0 ? ws.Cell(row, cAdversaire).GetString().Trim() : "";

                if (string.IsNullOrWhiteSpace(dateStr) && string.IsNullOrWhiteSpace(adversaire))
                    continue; // ligne vide

                if (!DateTime.TryParse(dateStr, out var dateMatch))
                {
                    // Essayer avec le format numérique Excel (OADate)
                    if (double.TryParse(dateStr, out var oaDate))
                        dateMatch = DateTime.FromOADate(oaDate);
                    else
                    {
                        erreurs++;
                        continue;
                    }
                }

                if (string.IsNullOrWhiteSpace(adversaire))
                {
                    erreurs++;
                    continue;
                }

                var domicileStr = cDomicile > 0 ? ws.Cell(row, cDomicile).GetString().Trim().ToLowerInvariant() : "oui";
                var estDomicile = domicileStr is "oui" or "o" or "true" or "1" or "domicile" or "dom" or "";

                var match = new Match
                {
                    EquipeId      = equipeId,
                    DateMatch     = dateMatch,
                    Adversaire    = adversaire,
                    EstDomicile   = estDomicile,
                    Lieu          = cLieu > 0 ? NullIfEmpty(ws.Cell(row, cLieu).GetString()) : null,
                    HeureDebutMatch     = cHDebut > 0   ? NullIfEmpty(ws.Cell(row, cHDebut).GetString())   : null,
                    HeureArriveeVestiaire = cHArrivee > 0 ? NullIfEmpty(ws.Cell(row, cHArrivee).GetString()) : null,
                    HeureDepartAutobus  = cHDepart > 0  ? NullIfEmpty(ws.Cell(row, cHDepart).GetString())  : null,
                    Notes         = cNotes > 0 ? NullIfEmpty(ws.Cell(row, cNotes).GetString()) : null,
                };

                _matchService.CreateMatch(new MatchViewModel
                {
                    EquipeId              = match.EquipeId,
                    DateMatch             = match.DateMatch,
                    Adversaire            = match.Adversaire,
                    EstDomicile           = match.EstDomicile,
                    Lieu                  = match.Lieu,
                    HeureDebutMatch       = match.HeureDebutMatch,
                    HeureArriveeVestiaire = match.HeureArriveeVestiaire,
                    HeureDepartAutobus    = match.HeureDepartAutobus,
                    Notes                 = match.Notes,
                });
                importes++;
            }
        }
        catch (Exception)
        {
            TempData["Error"] = "Erreur lors de la lecture du fichier Excel.";
            return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "calendrier" });
        }

        TempData["Success"] = erreurs == 0
            ? $"{importes} match(s) importé(s) avec succès."
            : $"{importes} match(s) importé(s), {erreurs} ligne(s) ignorée(s) (date ou adversaire invalide).";

        return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "calendrier" });
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void SetTheme(Ecole ecole)
    {
        ViewBag.CouleurPrimaire = ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = ecole.CouleurSecondaire;
    }
}
