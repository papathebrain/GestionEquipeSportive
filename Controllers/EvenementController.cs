using GestionEquipeSportive.Models;
using GestionEquipeSportive.Services;
using GestionEquipeSportive.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionEquipeSportive.Controllers;

[Authorize]
public class EvenementController : Controller
{
    private readonly IEvenementService _evenementService;
    private readonly IEquipeService _equipeService;
    private readonly IEcoleAccessService _access;

    public EvenementController(
        IEvenementService evenementService,
        IEquipeService equipeService,
        IEcoleAccessService access)
    {
        _evenementService = evenementService;
        _equipeService = equipeService;
        _access = access;
    }

    // ── Create ──────────────────────────────────────────────────────────────────

    public IActionResult Create(int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        ViewBag.Types = Enum.GetValues<TypeEvenement>();
        return View(new EvenementViewModel
        {
            EquipeId = equipeId,
            NomEquipe = equipe.Nom,
            DateStr = DateTime.Today.ToString("yyyy-MM-dd"),
            HeureDebut = "09:00"
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(EvenementViewModel vm)
    {
        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (equipe == null) return NotFound();
        if (!_access.PeutModifierEquipe(User, equipe.Id, equipe.EcoleId)) return Forbid();

        if (!ModelState.IsValid)
        {
            vm.NomEquipe = equipe.Nom;
            ViewBag.Types = Enum.GetValues<TypeEvenement>();
            return View(vm);
        }

        var events = BuildEvenements(vm);
        foreach (var ev in events)
            _evenementService.CreateEvenement(ev);

        TempData["Success"] = events.Count > 1
            ? $"{events.Count} événements ajoutés."
            : "Événement ajouté.";
        return RedirectToAction("Details", "Equipe", new { id = vm.EquipeId, tab = "calendrier" });
    }

    // ── Edit ────────────────────────────────────────────────────────────────────

    public IActionResult Edit(int id)
    {
        var ev = _evenementService.GetEvenementById(id);
        if (ev == null) return NotFound();
        var equipe = _equipeService.GetEquipeById(ev.EquipeId);
        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0)) return Forbid();

        ViewBag.Types = Enum.GetValues<TypeEvenement>();
        return View(new EvenementViewModel
        {
            Id = ev.Id,
            EquipeId = ev.EquipeId,
            NomEquipe = equipe?.Nom,
            Titre = ev.Titre,
            Type = ev.Type,
            DateStr = ev.DateDebut.ToString("yyyy-MM-dd"),
            HeureDebut = ev.DateDebut.ToString("HH:mm"),
            HeureFin = ev.DateFin.HasValue ? ev.DateFin.Value.ToString("HH:mm") : null,
            Lieu = ev.Lieu,
            Notes = ev.Notes
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, EvenementViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        var equipe = _equipeService.GetEquipeById(vm.EquipeId);
        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? vm.EquipeId)) return Forbid();

        if (!ModelState.IsValid)
        {
            vm.NomEquipe = equipe?.Nom;
            ViewBag.Types = Enum.GetValues<TypeEvenement>();
            return View(vm);
        }

        var ev = ToEvenement(vm);
        _evenementService.UpdateEvenement(ev);
        TempData["Success"] = "Événement modifié.";
        return RedirectToAction("Details", "Equipe", new { id = vm.EquipeId, tab = "calendrier" });
    }

    // ── Delete ──────────────────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id, int equipeId)
    {
        var equipe = _equipeService.GetEquipeById(equipeId);
        if (!_access.PeutModifierEquipe(User, equipe?.Id ?? 0, equipe?.EcoleId ?? 0)) return Forbid();
        _evenementService.DeleteEvenement(id);
        TempData["Success"] = "Événement supprimé.";
        return RedirectToAction("Details", "Equipe", new { id = equipeId, tab = "calendrier" });
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private static List<Evenement> BuildEvenements(EvenementViewModel vm)
    {
        // Parse date and times
        if (!DateTime.TryParse(vm.DateStr, out var dateBase))
            dateBase = DateTime.Today;

        var heureDebut = TimeSpan.Zero;
        if (TimeSpan.TryParse(vm.HeureDebut, out var hd)) heureDebut = hd;

        TimeSpan? heureFin = null;
        if (!string.IsNullOrWhiteSpace(vm.HeureFin) && TimeSpan.TryParse(vm.HeureFin, out var hf))
            heureFin = hf;

        var list = new List<Evenement>();

        if (!vm.Recurrence || vm.JoursSemaine.Count == 0)
        {
            // Événement unique
            list.Add(BuildSingle(vm, dateBase, heureDebut, heureFin));
            return list;
        }

        // Récurrence hebdomadaire
        if (!DateTime.TryParse(vm.RecurrenceDateFin, out var dateFin))
            dateFin = dateBase.AddMonths(3); // fallback 3 mois

        // Pour chaque jour sélectionné (DayOfWeek : 1=Lundi … 7=Dimanche)
        for (var d = dateBase.Date; d <= dateFin.Date; d = d.AddDays(1))
        {
            int dow = d.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)d.DayOfWeek;
            if (vm.JoursSemaine.Contains(dow))
                list.Add(BuildSingle(vm, d, heureDebut, heureFin));
        }

        return list;
    }

    private static Evenement BuildSingle(EvenementViewModel vm, DateTime date, TimeSpan heureDebut, TimeSpan? heureFin)
    {
        var dateDebut = date.Date + heureDebut;
        DateTime? dateFin = heureFin.HasValue ? date.Date + heureFin.Value : null;

        // Si heure de fin < heure de début → lendemain
        if (dateFin.HasValue && dateFin.Value <= dateDebut)
            dateFin = dateFin.Value.AddDays(1);

        return new Evenement
        {
            Id = vm.Id,
            EquipeId = vm.EquipeId,
            Titre = vm.Titre,
            Type = vm.Type,
            DateDebut = dateDebut,
            DateFin = dateFin,
            Lieu = string.IsNullOrWhiteSpace(vm.Lieu) ? null : vm.Lieu.Trim(),
            Notes = string.IsNullOrWhiteSpace(vm.Notes) ? null : vm.Notes.Trim()
        };
    }

    private static Evenement ToEvenement(EvenementViewModel vm)
    {
        if (!DateTime.TryParse(vm.DateStr, out var dateBase)) dateBase = DateTime.Today;
        var heureDebut = TimeSpan.Zero;
        if (TimeSpan.TryParse(vm.HeureDebut, out var hd)) heureDebut = hd;
        TimeSpan? heureFin = null;
        if (!string.IsNullOrWhiteSpace(vm.HeureFin) && TimeSpan.TryParse(vm.HeureFin, out var hf)) heureFin = hf;
        return BuildSingle(vm, dateBase, heureDebut, heureFin);
    }
}
