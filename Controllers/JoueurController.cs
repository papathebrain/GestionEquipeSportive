using ClosedXML.Excel;
using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class JoueurController : Controller
{
    private readonly IJoueurService _joueurService;
    private readonly IEquipeService _equipeService;
    private readonly IEcoleService _ecoleService;
    private readonly IEcoleAccessService _access;
    private readonly IWebHostEnvironment _env;
    private readonly IDictionnaireService _dictionnaireService;

    public JoueurController(IJoueurService joueurService, IEquipeService equipeService,
        IEcoleService ecoleService, IEcoleAccessService access, IWebHostEnvironment env,
        IDictionnaireService dictionnaireService)
    {
        _joueurService = joueurService;
        _equipeService = equipeService;
        _ecoleService = ecoleService;
        _access = access;
        _env = env;
        _dictionnaireService = dictionnaireService;
    }

    public IActionResult Index(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        equipe.Ecole = ecole;
        ViewBag.Equipe = equipe;
        ViewBag.PeutModifier = _access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId);

        // Autres équipes de l'école pour le déplacement
        ViewBag.EquipesEcole = _equipeService.GetEquipesByEcole(equipe.EcoleId)
            .Where(e => e.Id != equipeId)
            .OrderBy(e => e.TypeSport.ToString())
            .ThenBy(e => e.Niveau.ToString())
            .ThenBy(e => e.AnneeScolaire)
            .ToList();

        var joueurs = _joueurService.GetJoueursByEquipe(equipeId);
        return View(joueurs);
    }

    [HttpGet]
    public IActionResult RechercherJoueur(string q, int equipeId)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Json(new { joueurs = Array.Empty<object>(), message = "Saisissez au moins 2 caractères." });

        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return Json(new { joueurs = Array.Empty<object>() });

        var equipesDeLEcole = _equipeService.GetEquipesByEcole(equipe.EcoleId);
        var equipeIds = equipesDeLEcole.Select(e => e.Id).ToHashSet();

        // No de fiche déjà présents dans cette équipe (pour marquer les doublons)
        var fichesDejaDansEquipe = _joueurService.GetJoueursByEquipe(equipeId)
            .Where(j => !string.IsNullOrWhiteSpace(j.NoFiche))
            .Select(j => j.NoFiche!.Trim().ToLower())
            .ToHashSet();

        var terme = q.Trim();

        // Recherche dans tous les joueurs de l'école
        var tous = _joueurService.GetAllJoueurs()
            .Where(j => equipeIds.Contains(j.EquipeId))
            .Where(j => j.Actif)
            .Where(j =>
                j.Nom.Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                j.Prenom.Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                (j.Prenom + " " + j.Nom).Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                (j.Nom + " " + j.Prenom).Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(j.NoFiche) &&
                 j.NoFiche.Contains(terme, StringComparison.OrdinalIgnoreCase)))
            // Dédoublonner par NoFiche — garder l'enregistrement le plus récent
            .GroupBy(j => !string.IsNullOrWhiteSpace(j.NoFiche)
                ? j.NoFiche.Trim().ToLower()
                : $"_id_{j.Id}")
            .Select(g => g.OrderByDescending(j => j.Id).First())
            .OrderBy(j => j.Nom).ThenBy(j => j.Prenom)
            .Take(40)
            .ToList();

        var equipeNoms = equipesDeLEcole.ToDictionary(e => e.Id, e => e.Nom);

        var result = tous.Select(j => new
        {
            id = j.Id,
            nom = j.Nom,
            prenom = j.Prenom,
            numero = j.Numero,
            position = j.Position,  // virgule-séparé
            positionSpecifique = j.PositionSpecifique ?? "",
            noFiche = j.NoFiche ?? "",
            equipeActuelle = equipeNoms.TryGetValue(j.EquipeId, out var en) ? en : "",
            dejaPresent = !string.IsNullOrWhiteSpace(j.NoFiche) &&
                          fichesDejaDansEquipe.Contains(j.NoFiche.Trim().ToLower())
        }).ToList();

        return Json(new { joueurs = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjouterExistant(string joueurSourceIds, int equipeId, string? returnUrl = null)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipeId, equipe?.EcoleId ?? 0))
            return Forbid();

        var ids = (joueurSourceIds ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var i) ? i : 0)
            .Where(i => i > 0)
            .ToList();

        if (ids.Count == 0)
        {
            TempData["Error"] = "Aucun joueur sélectionné.";
        }
        else
        {
            _joueurService.CopierVersEquipe(ids, equipeId);
            TempData["Success"] = ids.Count == 1
                ? "1 joueur importé dans l'équipe."
                : $"{ids.Count} joueurs importés dans l'équipe.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index), new { equipeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Deplacer(int joueurId, int nouvelleEquipeId)
    {
        var joueur = _joueurService.GetJoueurById(joueurId);
        if (joueur == null) return NotFound();

        var equipeOrigId = joueur.EquipeId;
        var equipeOriginale = _equipeService.GetEquipeById(equipeOrigId);
        if (!_access.PeutModifierEquipe(User, equipeOrigId, equipeOriginale?.EcoleId ?? 0))
            return Forbid();

        _joueurService.DeplacerJoueur(joueurId, nouvelleEquipeId);
        var destination = _equipeService.GetEquipeById(nouvelleEquipeId);
        TempData["Success"] = $"{joueur.Prenom} {joueur.Nom} déplacé vers {destination?.Nom ?? "l'équipe"}.";
        return RedirectToAction(nameof(Index), new { equipeId = equipeOrigId });
    }

    public IActionResult Details(int id)
    {
        var joueur = _joueurService.GetJoueurById(id);
        if (joueur == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(joueur.EquipeId);
        var ecole = equipe != null ? _ecoleService.GetEcoleById(equipe.EcoleId) : null;
        if (ecole != null) SetTheme(ecole);

        joueur.Equipe = equipe;
        if (equipe != null) equipe.Ecole = ecole;
        ViewBag.PeutModifier = _access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0);

        // Historique : toutes les participations du joueur (par NoFiche ou nom/prénom)
        var historique = _joueurService.GetHistoriqueJoueur(joueur);
        foreach (var j in historique)
        {
            j.Equipe = _equipeService.GetEquipeById(j.EquipeId);
            if (j.Equipe != null)
                j.Equipe.Ecole = _ecoleService.GetEcoleById(j.Equipe.EcoleId);
        }
        ViewBag.Historique = historique;

        return View(joueur);
    }

    public IActionResult Create(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();

        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId))
            return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        var sport0 = equipe.TypeSport.ToString();
        var vm = new JoueurViewModel
        {
            EquipeId = equipeId,
            NomEquipe = equipe.Nom,
            EcoleId = equipe.EcoleId,
            PositionsDisponibles = _dictionnaireService.GetPositions(sport0),
            PositionsSpecifiquesDisponibles = _dictionnaireService.GetPositionsSpecifiques(sport0),
            PositionsSpecifiquesParGroupe = _dictionnaireService.GetPositionsSpecifiquesParGroupe(sport0)
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(JoueurViewModel vm)
    {
        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (equipe == null) return NotFound();

        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId))
            return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = _ecoleService.GetEcoleById(equipe.EcoleId);
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEquipe = equipe.Nom;
            var sportC = equipe.TypeSport.ToString();
            vm.PositionsDisponibles = _dictionnaireService.GetPositions(sportC);
            vm.PositionsSpecifiquesDisponibles = _dictionnaireService.GetPositionsSpecifiques(sportC);
            vm.PositionsSpecifiquesParGroupe = _dictionnaireService.GetPositionsSpecifiquesParGroupe(sportC);
            return View(vm);
        }

        _joueurService.CreateJoueur(vm, vm.PhotoFile, _env.WebRootPath);
        TempData["Success"] = "Joueur ajouté avec succès.";
        return RedirectToAction(nameof(Index), new { equipeId = vm.EquipeId });
    }

    public IActionResult Edit(int id)
    {
        var joueur = _joueurService.GetJoueurById(id);
        if (joueur == null) return NotFound();

        var equipe = _equipeService.GetEquipeById(joueur.EquipeId);
        var ecole = equipe != null ? _ecoleService.GetEcoleById(equipe.EcoleId) : null;

        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0))
            return Forbid();

        if (ecole != null) SetTheme(ecole);

        var vm = _joueurService.ToViewModel(joueur);
        vm.NomEquipe = equipe?.Nom;
        vm.EcoleId = equipe?.EcoleId ?? 0;
        if (equipe != null)
        {
            var sportE = equipe.TypeSport.ToString();
            vm.PositionsDisponibles = _dictionnaireService.GetPositions(sportE);
            vm.PositionsSpecifiquesDisponibles = _dictionnaireService.GetPositionsSpecifiques(sportE);
            vm.PositionsSpecifiquesParGroupe = _dictionnaireService.GetPositionsSpecifiquesParGroupe(sportE);
        }
        ViewBag.JoueurMedias = _joueurService.GetMediasByJoueur(id);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, JoueurViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? vm.EcoleId))
            return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = equipe != null ? _ecoleService.GetEcoleById(equipe.EcoleId) : null;
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEquipe = equipe?.Nom;
            if (equipe != null)
            {
                var sportP = equipe.TypeSport.ToString();
                vm.PositionsDisponibles = _dictionnaireService.GetPositions(sportP);
                vm.PositionsSpecifiquesDisponibles = _dictionnaireService.GetPositionsSpecifiques(sportP);
                vm.PositionsSpecifiquesParGroupe = _dictionnaireService.GetPositionsSpecifiquesParGroupe(sportP);
            }
            return View(vm);
        }

        _joueurService.UpdateJoueur(vm, vm.PhotoFile, _env.WebRootPath);
        TempData["Success"] = "Joueur modifié avec succès.";
        return RedirectToAction("Details", "Equipe", new { id = vm.EquipeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id, int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipeId, equipe?.EcoleId ?? 0))
            return Forbid();

        _joueurService.DeleteJoueur(id, _env.WebRootPath);
        TempData["Success"] = "Joueur supprimé avec succès.";
        return RedirectToAction(nameof(Index), new { equipeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjouterMedia(int id, IFormFile fichier)
    {
        var joueur = _joueurService.GetJoueurById(id);
        if (joueur == null) return NotFound();
        var equipe = _equipeService.GetEquipeById(joueur.EquipeId);
        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0))
            return Forbid();
        if (fichier != null && fichier.Length > 0)
            _joueurService.AddJoueurMedia(id, fichier, _env.WebRootPath);
        TempData["Success"] = "Photo ajoutée.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SupprimerMedia(int mediaId, int joueurId)
    {
        var joueur = _joueurService.GetJoueurById(joueurId);
        var equipe = joueur != null ? _equipeService.GetEquipeById(joueur.EquipeId) : null;
        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0))
            return Forbid();
        _joueurService.DeleteJoueurMedia(mediaId, _env.WebRootPath);
        TempData["Success"] = "Photo supprimée.";
        return RedirectToAction(nameof(Edit), new { id = joueurId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ImporterExcel(int equipeId, IFormFile fichierExcel)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipeId, equipe?.EcoleId ?? 0))
            return Forbid();

        if (fichierExcel == null || fichierExcel.Length == 0)
        {
            TempData["Error"] = "Veuillez sélectionner un fichier Excel.";
            return RedirectToAction(nameof(Index), new { equipeId });
        }

        int crees = 0, mis_a_jour = 0;
        var joueursExistants = _joueurService.GetJoueursByEquipe(equipeId);

        using var stream = fichierExcel.OpenReadStream();
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.First();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        // Detect column indices from header row (case-insensitive)
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int c = 1; c <= (ws.LastColumnUsed()?.ColumnNumber() ?? 1); c++)
        {
            var h = ws.Cell(1, c).GetString().Trim();
            if (!string.IsNullOrEmpty(h)) headers[h] = c;
        }

        int ColIdx(params string[] names)
        {
            foreach (var n in names)
                if (headers.TryGetValue(n, out var idx)) return idx;
            return -1;
        }

        int colNoFiche  = ColIdx("NoFiche", "No Fiche", "no_fiche", "Fiche");
        int colNom      = ColIdx("Nom");
        int colPrenom   = ColIdx("Prenom", "Prénom");
        int colNumero   = ColIdx("Numero", "Numéro", "No Chandail", "NoChandail", "Chandail");
        int colPosition = ColIdx("Position");
        int colPosSpec  = ColIdx("PositionSpecifique", "Position Specifique", "Position spécifique");

        string Cell(int row, int col) => col > 0 ? ws.Cell(row, col).GetString().Trim() : "";

        for (int row = 2; row <= lastRow; row++)
        {
            var nom    = Cell(row, colNom);
            var prenom = Cell(row, colPrenom);
            if (string.IsNullOrWhiteSpace(nom) && string.IsNullOrWhiteSpace(prenom)) continue;

            var noFiche  = Cell(row, colNoFiche);
            var numero   = Cell(row, colNumero);
            var position = Cell(row, colPosition);
            var posSpec  = Cell(row, colPosSpec);

            // Try to match by NoFiche within this team
            var existant = !string.IsNullOrWhiteSpace(noFiche)
                ? joueursExistants.FirstOrDefault(j =>
                    !string.IsNullOrWhiteSpace(j.NoFiche) &&
                    string.Equals(j.NoFiche.Trim(), noFiche, StringComparison.OrdinalIgnoreCase))
                : null;

            if (existant != null)
            {
                // Update existing
                var vm = _joueurService.ToViewModel(existant);
                if (!string.IsNullOrWhiteSpace(nom))    vm.Nom    = nom;
                if (!string.IsNullOrWhiteSpace(prenom)) vm.Prenom = prenom;
                if (!string.IsNullOrWhiteSpace(numero)) vm.Numero = numero;
                if (!string.IsNullOrWhiteSpace(position)) vm.PositionPrincipale = position.Trim();
                if (!string.IsNullOrWhiteSpace(posSpec))  vm.PositionPairsRaw = posSpec.Trim();
                _joueurService.UpdateJoueur(vm, null, _env.WebRootPath);
                mis_a_jour++;
            }
            else
            {
                // Create new
                var vm = new GestionEquipeSportive.ViewModels.JoueurViewModel
                {
                    EquipeId           = equipeId,
                    Nom                = nom,
                    Prenom             = prenom,
                    Numero             = numero,
                    PositionPrincipale  = string.IsNullOrWhiteSpace(position) ? "" : position.Trim(),
                    PositionPairsRaw    = string.IsNullOrWhiteSpace(posSpec) ? "" : posSpec.Trim(),
                    NoFiche            = string.IsNullOrWhiteSpace(noFiche) ? null : noFiche,
                    ConsentementPhoto  = true,
                    Actif              = true
                };
                _joueurService.CreateJoueur(vm, null, _env.WebRootPath);
                crees++;
            }
        }

        TempData["Success"] = $"Import terminé : {crees} joueur(s) créé(s), {mis_a_jour} mis à jour.";
        return RedirectToAction(nameof(Index), new { equipeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActif(int id, int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipeId, equipe?.EcoleId ?? 0))
            return Forbid();

        _joueurService.ToggleActif(id);
        return RedirectToAction(nameof(Index), new { equipeId });
    }

    private void SetTheme(Ecole ecole)
    {
        ViewBag.CouleurPrimaire = ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = ecole.CouleurSecondaire;
    }
}
