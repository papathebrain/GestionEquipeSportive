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

    // ── Liste joueurs par équipe (JoueurEquipes) ──────────────────────────────
    public IActionResult Index(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        equipe.Ecole = ecole;
        ViewBag.Equipe = equipe;
        ViewBag.PeutModifier = _access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId);
        ViewBag.EquipesEcole = _equipeService.GetEquipesByEcole(equipe.EcoleId)
            .Where(e => e.Id != equipeId).OrderBy(e => e.TypeSport.ToString()).ThenBy(e => e.AnneeScolaire).ToList();

        var joueurEquipes = _joueurService.GetJoueurEquipesByEquipe(equipeId);
        return View(joueurEquipes);
    }

    // ── Rechercher joueur dans l'école (pour import) ──────────────────────────
    [HttpGet]
    public IActionResult RechercherJoueur(string? q, int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return Json(new { joueurs = Array.Empty<object>() });

        var dejaAssignes = _joueurService.GetJoueurEquipesByEquipe(equipeId)
            .Select(je => je.JoueurId).ToHashSet();

        var tous = _joueurService.GetJoueursByEcole(equipe.EcoleId)
            .Where(j => j.Actif)
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q) && q.Trim().Length >= 1)
        {
            var terme = q.Trim();
            tous = tous.Where(j =>
                j.Nom.Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                j.Prenom.Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                (j.Prenom + " " + j.Nom).Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                (j.Nom + " " + j.Prenom).Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(j.NoFiche) && j.NoFiche.Contains(terme, StringComparison.OrdinalIgnoreCase)));
        }

        var result = tous.OrderBy(j => j.Nom).ThenBy(j => j.Prenom)
            .Select(j => new
            {
                id = j.Id,
                nom = j.Nom,
                prenom = j.Prenom,
                noFiche = j.NoFiche ?? "",
                dejaPresent = dejaAssignes.Contains(j.Id),
                photoPath = _joueurService.GetMediasByJoueur(j.Id)
                    .OrderByDescending(m => m.DateAjout)
                    .FirstOrDefault()?.CheminFichier
            }).ToList();

        return Json(new { joueurs = result });
    }

    // ── Assigner joueur(s) existant(s) à une équipe ───────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjouterExistant(string joueurSourceIds, int equipeId, string? returnUrl = null)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipeId, equipe?.EcoleId ?? 0)) return Forbid();

        var ids = (joueurSourceIds ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var i) ? i : 0)
            .Where(i => i > 0).ToList();

        var dejaAssignes = _joueurService.GetJoueurEquipesByEquipe(equipeId).Select(je => je.JoueurId).ToHashSet();
        int count = 0;
        foreach (var id in ids)
        {
            if (dejaAssignes.Contains(id)) continue;
            _joueurService.AssignerAEquipe(new JoueurEquipeViewModel { JoueurId = id, EquipeId = equipeId, Actif = true }, null, _env.WebRootPath);
            count++;
        }
        TempData["Success"] = count == 1 ? "1 joueur ajouté à l'équipe." : $"{count} joueurs ajoutés à l'équipe.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "joueurs" });
    }

    // ── Déplacer assignation ───────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Deplacer(int joueurEquipeId, int nouvelleEquipeId)
    {
        var je = _joueurService.GetJoueurEquipeById(joueurEquipeId);
        if (je == null) return NotFound();

        var equipeOriginale = _equipeService.GetEquipeById(je.EquipeId);
        if (!_access.PeutModifierEquipe(User, je.EquipeId, equipeOriginale?.EcoleId ?? 0)) return Forbid();

        // Vérifier pas déjà dans la destination
        var dejaPresent = _joueurService.GetJoueurEquipesByEquipe(nouvelleEquipeId).Any(x => x.JoueurId == je.JoueurId);
        if (!dejaPresent)
        {
            je.EquipeId = nouvelleEquipeId;
            _joueurService.UpdateAssignation(_joueurService.ToAssignationViewModel(je), null, _env.WebRootPath);
        }

        TempData["Success"] = "Joueur déplacé.";
        return RedirectToAction("Details", "Equipe", new { id = je.EquipeId, tab = "joueurs" });
    }

    // ── Créer joueur au niveau école ───────────────────────────────────────────
    [HttpGet]
    public IActionResult Create(int ecoleId, int? equipeId = null)
    {
        var ecole = _ecoleService.GetEcoleById(ecoleId);
        if (ecole == null) return NotFound();
        if (!_access.PeutModifier(User, ecoleId)) return Forbid();
        SetTheme(ecole);

        ViewBag.Ecole = ecole;
        ViewBag.EquipeId = equipeId;
        return View(new JoueurViewModel { EcoleId = ecoleId, NomEcole = ecole.Nom });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(JoueurViewModel vm, int? equipeId = null)
    {
        var ecole = _ecoleService.GetEcoleById(vm.EcoleId);
        if (ecole == null) return NotFound();
        if (!_access.PeutModifier(User, vm.EcoleId)) return Forbid();

        if (!ModelState.IsValid)
        {
            SetTheme(ecole);
            ViewBag.Ecole = ecole;
            ViewBag.EquipeId = equipeId;
            return View(vm);
        }

        var joueur = _joueurService.CreateJoueur(vm);
        TempData["Success"] = $"{joueur.Prenom} {joueur.Nom} créé.";

        // Si on vient d'une équipe, rediriger vers l'assignation
        if (equipeId.HasValue)
            return RedirectToAction(nameof(AssignerEquipe), new { joueurId = joueur.Id, equipeId = equipeId.Value });

        return RedirectToAction("Edit", "Ecole", new { id = vm.EcoleId, tab = "joueurs" });
    }

    // ── Éditer joueur (infos de base) ─────────────────────────────────────────
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var joueur = _joueurService.GetJoueurById(id);
        if (joueur == null) return NotFound();

        var ecole = _ecoleService.GetEcoleById(joueur.EcoleId);
        if (!_access.PeutModifier(User, joueur.EcoleId)) return Forbid();
        if (ecole != null) SetTheme(ecole);

        var vm = _joueurService.ToViewModel(joueur);
        vm.NomEcole = ecole?.Nom;

        ViewBag.Ecole = ecole;
        ViewBag.JoueurMedias = _joueurService.GetMediasByJoueur(id);

        // Assignations avec équipes
        var assignations = _joueurService.GetJoueurEquipesByJoueur(id);
        foreach (var je in assignations)
            je.Equipe = _equipeService.GetEquipeById(je.EquipeId);
        ViewBag.Assignations = assignations;

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(JoueurViewModel vm)
    {
        var ecole = _ecoleService.GetEcoleById(vm.EcoleId);
        if (!_access.PeutModifier(User, vm.EcoleId)) return Forbid();

        if (!ModelState.IsValid)
        {
            if (ecole != null) SetTheme(ecole);
            ViewBag.Ecole = ecole;
            ViewBag.JoueurMedias = _joueurService.GetMediasByJoueur(vm.Id);
            var assignations2 = _joueurService.GetJoueurEquipesByJoueur(vm.Id);
            foreach (var je in assignations2) je.Equipe = _equipeService.GetEquipeById(je.EquipeId);
            ViewBag.Assignations = assignations2;
            return View(vm);
        }

        var joueur = _joueurService.UpdateJoueur(vm);
        TempData["Success"] = $"{joueur.Prenom} {joueur.Nom} modifié.";
        return RedirectToAction("Edit", "Ecole", new { id = vm.EcoleId, tab = "joueurs" });
    }

    // ── Assigner joueur à une équipe (créer JoueurEquipe) ─────────────────────
    [HttpGet]
    public IActionResult AssignerEquipe(int joueurId, int equipeId)
    {
        var joueur = _joueurService.GetJoueurById(joueurId);
        if (joueur == null) return NotFound();
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipeId, equipe.EcoleId)) return Forbid();

        var ecole = _ecoleService.GetEcoleById(equipe.EcoleId);
        if (ecole != null) SetTheme(ecole);

        var sport = equipe.TypeSport.ToString();
        var vm = new JoueurEquipeViewModel
        {
            JoueurId = joueurId,
            EquipeId = equipeId,
            NomJoueur = $"{joueur.Prenom} {joueur.Nom}",
            NomEquipe = equipe.Nom,
            EcoleId = equipe.EcoleId,
            Actif = true,
            PositionsDisponibles = _dictionnaireService.GetPositions(sport),
            PositionsSpecifiquesDisponibles = _dictionnaireService.GetPositionsSpecifiques(sport),
            PositionsSpecifiquesParGroupe = _dictionnaireService.GetPositionsSpecifiquesParGroupe(sport)
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AssignerEquipe(JoueurEquipeViewModel vm)
    {
        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (!_access.PeutModifierEquipe(User, vm.EquipeId, equipe?.EcoleId ?? 0)) return Forbid();

        // Vérifier pas déjà assigné
        var dejaPresent = _joueurService.GetJoueurEquipesByEquipe(vm.EquipeId).Any(je => je.JoueurId == vm.JoueurId);
        if (dejaPresent)
        {
            TempData["Error"] = "Ce joueur est déjà assigné à cette équipe.";
            return RedirectToAction("Details", "Equipe", new { id = vm.EquipeId, tab = "joueurs" });
        }

        if (!ModelState.IsValid)
        {
            var ecole2 = equipe != null ? _ecoleService.GetEcoleById(equipe.EcoleId) : null;
            if (ecole2 != null) SetTheme(ecole2);
            var joueur2 = _joueurService.GetJoueurById(vm.JoueurId);
            vm.NomJoueur = joueur2 != null ? $"{joueur2.Prenom} {joueur2.Nom}" : null;
            vm.NomEquipe = equipe?.Nom;
            if (equipe != null)
            {
                var sport2 = equipe.TypeSport.ToString();
                vm.PositionsDisponibles = _dictionnaireService.GetPositions(sport2);
                vm.PositionsSpecifiquesDisponibles = _dictionnaireService.GetPositionsSpecifiques(sport2);
                vm.PositionsSpecifiquesParGroupe = _dictionnaireService.GetPositionsSpecifiquesParGroupe(sport2);
            }
            return View(vm);
        }

        _joueurService.AssignerAEquipe(vm, vm.PhotoFile, _env.WebRootPath);
        TempData["Success"] = "Joueur assigné à l'équipe.";
        return RedirectToAction("Details", "Equipe", new { id = vm.EquipeId, tab = "joueurs" });
    }

    // ── Éditer assignation (position, numéro, photo, description) ────────────
    [HttpGet]
    public IActionResult EditAssignation(int id)
    {
        var je = _joueurService.GetJoueurEquipeById(id);
        if (je == null) return NotFound();
        var equipe = _equipeService.GetEquipeById(je.EquipeId);
        if (!_access.PeutModifierEquipe(User, je.EquipeId, equipe?.EcoleId ?? 0)) return Forbid();

        var ecole = equipe != null ? _ecoleService.GetEcoleById(equipe.EcoleId) : null;
        if (ecole != null) SetTheme(ecole);

        var vm = _joueurService.ToAssignationViewModel(je);
        vm.NomEquipe = equipe?.Nom;
        vm.EcoleId = equipe?.EcoleId ?? 0;
        if (equipe != null)
        {
            var sport = equipe.TypeSport.ToString();
            vm.PositionsDisponibles = _dictionnaireService.GetPositions(sport);
            vm.PositionsSpecifiquesDisponibles = _dictionnaireService.GetPositionsSpecifiques(sport);
            vm.PositionsSpecifiquesParGroupe = _dictionnaireService.GetPositionsSpecifiquesParGroupe(sport);
        }
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditAssignation(JoueurEquipeViewModel vm)
    {
        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (!_access.PeutModifierEquipe(User, vm.EquipeId, equipe?.EcoleId ?? 0)) return Forbid();

        if (!ModelState.IsValid)
        {
            var ecole2 = equipe != null ? _ecoleService.GetEcoleById(equipe.EcoleId) : null;
            if (ecole2 != null) SetTheme(ecole2);
            vm.NomEquipe = equipe?.Nom;
            if (equipe != null)
            {
                var sport2 = equipe.TypeSport.ToString();
                vm.PositionsDisponibles = _dictionnaireService.GetPositions(sport2);
                vm.PositionsSpecifiquesDisponibles = _dictionnaireService.GetPositionsSpecifiques(sport2);
                vm.PositionsSpecifiquesParGroupe = _dictionnaireService.GetPositionsSpecifiquesParGroupe(sport2);
            }
            return View(vm);
        }

        _joueurService.UpdateAssignation(vm, vm.PhotoFile, _env.WebRootPath);
        TempData["Success"] = "Assignation mise à jour.";
        return RedirectToAction("Details", "Equipe", new { id = vm.EquipeId, tab = "joueurs" });
    }

    // ── Supprimer joueur ───────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id, int ecoleId)
    {
        if (!_access.PeutModifier(User, ecoleId)) return Forbid();
        var joueur = _joueurService.GetJoueurById(id);
        if (joueur == null) return NotFound();
        _joueurService.DeleteJoueur(id, _env.WebRootPath);
        TempData["Success"] = $"{joueur.Prenom} {joueur.Nom} supprimé.";
        return RedirectToAction("Edit", "Ecole", new { id = ecoleId, tab = "joueurs" });
    }

    // ── Supprimer assignation ─────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SupprimerAssignation(int id, int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipeId, equipe?.EcoleId ?? 0)) return Forbid();
        _joueurService.SupprimerAssignation(id, _env.WebRootPath);
        TempData["Success"] = "Joueur retiré de l'équipe.";
        return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "joueurs" });
    }

    // ── Toggle actif (sur assignation) ────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActif(int joueurEquipeId, int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipeId, equipe?.EcoleId ?? 0)) return Forbid();
        var je = _joueurService.GetJoueurEquipeById(joueurEquipeId);
        if (je != null)
        {
            var vm = _joueurService.ToAssignationViewModel(je);
            vm.Actif = !je.Actif;
            _joueurService.UpdateAssignation(vm, null, _env.WebRootPath);
        }
        return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "joueurs" });
    }

    // ── Médias joueur ─────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjouterMedia(int joueurId, IFormFile fichier, string? description)
    {
        var joueur = _joueurService.GetJoueurById(joueurId);
        if (joueur == null) return NotFound();
        if (!_access.PeutModifier(User, joueur.EcoleId)) return Forbid();
        _joueurService.AddJoueurMedia(joueurId, fichier, _env.WebRootPath);
        TempData["Success"] = "Photo ajoutée.";
        return RedirectToAction(nameof(Edit), new { id = joueurId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SupprimerMedia(int id, int joueurId)
    {
        var joueur = _joueurService.GetJoueurById(joueurId);
        if (joueur == null) return NotFound();
        if (!_access.PeutModifier(User, joueur.EcoleId)) return Forbid();
        _joueurService.DeleteJoueurMedia(id, _env.WebRootPath);
        TempData["Success"] = "Photo supprimée.";
        return RedirectToAction(nameof(Edit), new { id = joueurId });
    }

    // ── Gabarit Excel ─────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult GabaritExcelJoueurs()
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Joueurs");
        var headers = new[] { "No Fiche", "Nom", "Prénom", "Numéro chandail", "Position", "Position spécifique" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3a5c");
            cell.Style.Font.FontColor = XLColor.White;
        }
        ws.Cell(2, 1).Value = "";
        ws.Cell(2, 2).Value = "Tremblay";
        ws.Cell(2, 3).Value = "Marc";
        ws.Cell(2, 4).Value = "12";
        ws.Cell(2, 5).Value = "Attaque";
        ws.Cell(2, 6).Value = "Attaque|QB";
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "gabarit_joueurs.xlsx");
    }

    // ── Importer Excel ────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ImporterExcel(int equipeId, IFormFile fichierExcel, string? returnUrl = null)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipeId, equipe?.EcoleId ?? 0)) return Forbid();
        if (fichierExcel == null || fichierExcel.Length == 0)
        {
            TempData["Error"] = "Veuillez sélectionner un fichier Excel.";
            return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "joueurs" });
        }

        int crees = 0, assignes = 0, misAJour = 0;
        var joueursDeLEcole = _joueurService.GetJoueursByEcole(equipe!.EcoleId);
        var assignationsExistantes = _joueurService.GetJoueurEquipesByEquipe(equipeId);

        using var stream = fichierExcel.OpenReadStream();
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.First();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int c = 1; c <= (ws.LastColumnUsed()?.ColumnNumber() ?? 1); c++)
        {
            var h = ws.Cell(1, c).GetString().Trim();
            if (!string.IsNullOrEmpty(h)) headers[h] = c;
        }

        int ColIdx(params string[] names) { foreach (var n in names) if (headers.TryGetValue(n, out var idx)) return idx; return -1; }
        int colNoFiche = ColIdx("NoFiche", "No Fiche", "Fiche");
        int colNom = ColIdx("Nom");
        int colPrenom = ColIdx("Prenom", "Prénom");
        int colNumero = ColIdx("Numero", "Numéro", "No Chandail", "Numéro chandail");
        int colPosition = ColIdx("Position");
        int colPosSpec = ColIdx("PositionSpecifique", "Position Specifique", "Position spécifique");
        string Cell(int row, int col) => col > 0 ? ws.Cell(row, col).GetString().Trim() : "";

        for (int row = 2; row <= lastRow; row++)
        {
            var nom = Cell(row, colNom);
            var prenom = Cell(row, colPrenom);
            if (string.IsNullOrWhiteSpace(nom) && string.IsNullOrWhiteSpace(prenom)) continue;

            var noFiche = Cell(row, colNoFiche);
            var numero = Cell(row, colNumero);
            var position = Cell(row, colPosition);
            var posSpec = Cell(row, colPosSpec);

            // Trouver joueur existant dans l'école par NoFiche ou Nom+Prénom
            Joueur? joueur = null;
            if (!string.IsNullOrWhiteSpace(noFiche))
                joueur = joueursDeLEcole.FirstOrDefault(j => !string.IsNullOrWhiteSpace(j.NoFiche) &&
                    string.Equals(j.NoFiche.Trim(), noFiche, StringComparison.OrdinalIgnoreCase));
            if (joueur == null && !string.IsNullOrWhiteSpace(nom) && !string.IsNullOrWhiteSpace(prenom))
                joueur = joueursDeLEcole.FirstOrDefault(j =>
                    string.Equals(j.Nom.Trim(), nom, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(j.Prenom.Trim(), prenom, StringComparison.OrdinalIgnoreCase));

            if (joueur == null)
            {
                // Créer nouveau joueur dans l'école
                joueur = _joueurService.CreateJoueur(new JoueurViewModel
                {
                    EcoleId = equipe.EcoleId,
                    Nom = nom,
                    Prenom = prenom,
                    NoFiche = string.IsNullOrWhiteSpace(noFiche) ? null : noFiche,
                    ConsentementPhoto = true
                });
                joueursDeLEcole.Add(joueur);
                crees++;
            }

            // Créer ou mettre à jour l'assignation
            var assignation = assignationsExistantes.FirstOrDefault(je => je.JoueurId == joueur.Id);
            if (assignation == null)
            {
                _joueurService.AssignerAEquipe(new JoueurEquipeViewModel
                {
                    JoueurId = joueur.Id,
                    EquipeId = equipeId,
                    Numero = numero,
                    PositionPrincipale = string.IsNullOrWhiteSpace(position) ? "" : position.Trim(),
                    PositionPairsRaw = string.IsNullOrWhiteSpace(posSpec) ? "" : posSpec.Trim(),
                    Actif = true
                }, null, _env.WebRootPath);
                assignes++;
            }
            else
            {
                var vm = _joueurService.ToAssignationViewModel(assignation);
                if (!string.IsNullOrWhiteSpace(numero)) vm.Numero = numero;
                if (!string.IsNullOrWhiteSpace(position)) vm.PositionPrincipale = position.Trim();
                if (!string.IsNullOrWhiteSpace(posSpec)) vm.PositionPairsRaw = posSpec.Trim();
                _joueurService.UpdateAssignation(vm, null, _env.WebRootPath);
                misAJour++;
            }
        }

        TempData["Success"] = $"Import terminé : {crees} joueur(s) créé(s), {assignes} assigné(s), {misAJour} mis à jour.";
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "joueurs" });
    }

    private void SetTheme(Ecole ecole)
    {
        ViewBag.CouleurPrimaire = ecole.CouleurPrimaire;
        ViewBag.CouleurSecondaire = ecole.CouleurSecondaire;
    }
}
