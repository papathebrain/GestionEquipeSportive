using GestionEquipeSportive.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using System.Text.Json;

namespace GestionEquipeSportive.Services;

public class UserService : IUserService
{
    private readonly string _filePath;
    private readonly PasswordHasher<ApplicationUser> _hasher = new();
    private readonly object _lock = new();

    // Verrouillage : max 5 tentatives, verrou 15 minutes
    private const int MaxTentatives = 5;
    private static readonly TimeSpan DureeVerrouillage = TimeSpan.FromMinutes(15);
    private readonly ConcurrentDictionary<string, (int Compteur, DateTime Debut)> _echecs = new();

    public UserService(IWebHostEnvironment env)
    {
        var dataPath = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataPath);
        _filePath = Path.Combine(dataPath, "users.json");
        InitialiserSiVide();
    }

    // ─── Authentification ────────────────────────────────────────────────────

    public ApplicationUser? Authentifier(string nomUtilisateur, string motDePasse)
    {
        if (EstVerrouille(nomUtilisateur))
            return null;

        var user = GetByNomUtilisateur(nomUtilisateur);
        if (user == null || !user.EstActif)
        {
            EnregistrerEchecConnexion(nomUtilisateur);
            return null;
        }

        var result = _hasher.VerifyHashedPassword(user, user.MotDePasseHash, motDePasse);
        if (result == PasswordVerificationResult.Failed)
        {
            EnregistrerEchecConnexion(nomUtilisateur);
            return null;
        }

        // Rehash si nécessaire (migration vers algorithme plus récent)
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.MotDePasseHash = _hasher.HashPassword(user, motDePasse);
            Modifier(user);
        }

        ReinitialiserEchecs(nomUtilisateur);
        user.DerniereConnexion = DateTime.UtcNow;
        Modifier(user);
        return user;
    }

    // ─── CRUD ─────────────────────────────────────────────────────────────────

    public IEnumerable<ApplicationUser> GetAllUsers()
        => Charger().OrderBy(u => u.NomUtilisateur);

    public ApplicationUser? GetById(string id)
        => Charger().FirstOrDefault(u => u.Id == id);

    public ApplicationUser? GetByNomUtilisateur(string nomUtilisateur)
        => Charger().FirstOrDefault(u =>
            u.NomUtilisateur.Equals(nomUtilisateur, StringComparison.OrdinalIgnoreCase));

    public void Ajouter(ApplicationUser user, string motDePasse)
    {
        user.Id = Guid.NewGuid().ToString();
        user.DateCreation = DateTime.UtcNow;
        user.MotDePasseHash = _hasher.HashPassword(user, motDePasse);

        var users = Charger().ToList();
        users.Add(user);
        Sauvegarder(users);
    }

    public void Modifier(ApplicationUser user)
    {
        var users = Charger().ToList();
        var idx = users.FindIndex(u => u.Id == user.Id);
        if (idx >= 0) users[idx] = user;
        Sauvegarder(users);
    }

    public void ChangerMotDePasse(string userId, string nouveauMotDePasse)
    {
        var users = Charger().ToList();
        var user = users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return;

        user.MotDePasseHash = _hasher.HashPassword(user, nouveauMotDePasse);
        user.ChangerMotDePasse = false;
        Sauvegarder(users);
    }

    public void Supprimer(string id)
    {
        var users = Charger().Where(u => u.Id != id).ToList();
        Sauvegarder(users);
    }

    public bool NomUtilisateurExiste(string nomUtilisateur, string? excludeId = null)
        => Charger().Any(u =>
            u.NomUtilisateur.Equals(nomUtilisateur, StringComparison.OrdinalIgnoreCase)
            && u.Id != excludeId);

    // ─── Verrouillage ─────────────────────────────────────────────────────────

    public bool EstVerrouille(string nomUtilisateur)
    {
        var key = nomUtilisateur.ToLowerInvariant();
        if (!_echecs.TryGetValue(key, out var info)) return false;
        if (info.Compteur < MaxTentatives) return false;

        if (DateTime.UtcNow - info.Debut >= DureeVerrouillage)
        {
            _echecs.TryRemove(key, out _);
            return false;
        }
        return true;
    }

    public void EnregistrerEchecConnexion(string nomUtilisateur)
    {
        var key = nomUtilisateur.ToLowerInvariant();
        _echecs.AddOrUpdate(key,
            _ => (1, DateTime.UtcNow),
            (_, old) =>
            {
                // Réinitialiser le compteur si la période de verrouillage est écoulée
                if (DateTime.UtcNow - old.Debut >= DureeVerrouillage)
                    return (1, DateTime.UtcNow);
                return (old.Compteur + 1, old.Debut);
            });
    }

    public void ReinitialiserEchecs(string nomUtilisateur)
        => _echecs.TryRemove(nomUtilisateur.ToLowerInvariant(), out _);

    public int TentativesRestantes(string nomUtilisateur)
    {
        var key = nomUtilisateur.ToLowerInvariant();
        if (!_echecs.TryGetValue(key, out var info)) return MaxTentatives;
        return Math.Max(0, MaxTentatives - info.Compteur);
    }

    // ─── Persistance JSON ─────────────────────────────────────────────────────

    private List<ApplicationUser> Charger()
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath)) return [];
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<ApplicationUser>>(json) ?? [];
        }
    }

    private void Sauvegarder(List<ApplicationUser> users)
    {
        lock (_lock)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_filePath, JsonSerializer.Serialize(users, options));
        }
    }

    private void InitialiserSiVide()
    {
        if (File.Exists(_filePath) && Charger().Count > 0) return;

        // Générer un mot de passe aléatoire sécurisé pour le premier admin
        var motDePasseInitial = GenererMotDePasse();

        var admin = new ApplicationUser
        {
            NomUtilisateur = "admin",
            NomComplet = "Administrateur",
            Role = Roles.Admin,
            EstActif = true,
            ChangerMotDePasse = true // Forcer changement au premier login
        };
        admin.MotDePasseHash = _hasher.HashPassword(admin, motDePasseInitial);
        Sauvegarder([admin]);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=================================================");
        Console.WriteLine(" PREMIER DÉMARRAGE — COMPTE ADMINISTRATEUR");
        Console.WriteLine($" Nom d'utilisateur : admin");
        Console.WriteLine($" Mot de passe      : {motDePasseInitial}");
        Console.WriteLine(" Changez ce mot de passe après la première connexion.");
        Console.WriteLine("=================================================");
        Console.ResetColor();
    }

    private static string GenererMotDePasse()
    {
        const string majuscules = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string minuscules = "abcdefghjkmnpqrstuvwxyz";
        const string chiffres = "23456789";
        const string speciaux = "@#$%!";

        var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[4];

        char Pick(string charset)
        {
            rng.GetBytes(bytes);
            return charset[BitConverter.ToUInt32(bytes) % charset.Length];
        }

        var chars = new List<char>
        {
            Pick(majuscules), Pick(majuscules),
            Pick(minuscules), Pick(minuscules),
            Pick(chiffres),   Pick(chiffres),
            Pick(speciaux),   Pick(speciaux)
        };

        // Mélanger avec Fisher-Yates
        for (int i = chars.Count - 1; i > 0; i--)
        {
            rng.GetBytes(bytes);
            int j = (int)(BitConverter.ToUInt32(bytes) % (uint)(i + 1));
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string([.. chars]);
    }
}
