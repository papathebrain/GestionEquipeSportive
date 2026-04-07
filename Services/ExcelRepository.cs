using ClosedXML.Excel;
using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.Services;

public class ExcelRepository : IExcelRepository
{
    private readonly string _dataPath;
    private readonly object _lock = new object();

    private string EcolesFile => Path.Combine(_dataPath, "ecoles.xlsx");
    private string EquipesFile => Path.Combine(_dataPath, "equipes.xlsx");
    private string ThemesFile => Path.Combine(_dataPath, "themes.xlsx");
    private string JoueursFile => Path.Combine(_dataPath, "joueurs.xlsx");
    private string GalerieFile => Path.Combine(_dataPath, "galerie.xlsx");
    private string StaffFile => Path.Combine(_dataPath, "staff.xlsx");
    private string MatchsFile => Path.Combine(_dataPath, "matchs.xlsx");
    private string MatchMediasFile => Path.Combine(_dataPath, "match_medias.xlsx");
    private string JoueurMediasFile => Path.Combine(_dataPath, "joueur_medias.xlsx");
    private string AbsencesFile => Path.Combine(_dataPath, "absences_match.xlsx");
    private string EvenementsFile => Path.Combine(_dataPath, "evenements.xlsx");
    private string DictionnaireFile => Path.Combine(_dataPath, "dictionnaires.xlsx");

    public ExcelRepository(IConfiguration configuration, IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(_dataPath);
        InitializeFiles();
    }

    private void InitializeFiles()
    {
        InitEcolesFile();
        InitEquipesFile();
        InitThemesFile();
        InitJoueursFile();
        InitGalerieFile();
        InitStaffFile();
        InitMatchsFile();
        InitMatchMediasFile();
        InitJoueurMediasFile();
        InitAbsencesFile();
        InitEvenementsFile();
        InitDictionnaireFile();
    }

    private void InitEcolesFile()
    {
        if (!File.Exists(EcolesFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Ecoles");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Nom";
            ws.Cell(1, 3).Value = "NomEquipe";
            ws.Cell(1, 4).Value = "LogoPath";
            ws.Cell(1, 5).Value = "CouleurPrimaire";
            ws.Cell(1, 6).Value = "CouleurSecondaire";
            ws.Cell(1, 7).Value = "LiensSociaux";
            wb.SaveAs(EcolesFile);
        }
    }

    private void InitEquipesFile()
    {
        if (!File.Exists(EquipesFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Equipes");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "EcoleId";
            ws.Cell(1, 3).Value = "AnneeScolaire";
            ws.Cell(1, 4).Value = "TypeSport";
            ws.Cell(1, 5).Value = "Niveau";
            ws.Cell(1, 6).Value = "Nom";
            ws.Cell(1, 7).Value = "AfficherPublic";
            ws.Cell(1, 8).Value = "ThemeId";
            wb.SaveAs(EquipesFile);
        }
    }

    private void InitThemesFile()
    {
        if (!File.Exists(ThemesFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Themes");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "EcoleId";
            ws.Cell(1, 3).Value = "NomEquipe";
            ws.Cell(1, 4).Value = "CouleurPrimaire";
            ws.Cell(1, 5).Value = "CouleurSecondaire";
            ws.Cell(1, 6).Value = "LogoPath";
            wb.SaveAs(ThemesFile);
        }
    }

    private void InitJoueursFile()
    {
        if (!File.Exists(JoueursFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Joueurs");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "EquipeId";
            ws.Cell(1, 3).Value = "Nom";
            ws.Cell(1, 4).Value = "Prenom";
            ws.Cell(1, 5).Value = "Numero";
            ws.Cell(1, 6).Value = "Position";
            ws.Cell(1, 7).Value = "PhotoPath";
            ws.Cell(1, 8).Value = "NoFiche";
            ws.Cell(1, 9).Value = "PositionSpecifique";
            ws.Cell(1, 10).Value = "Description";
            ws.Cell(1, 11).Value = "ConsentementPhoto";
            ws.Cell(1, 12).Value = "Actif";
            wb.SaveAs(JoueursFile);
        }
    }

    private void InitGalerieFile()
    {
        if (!File.Exists(GalerieFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Galerie");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "EquipeId";
            ws.Cell(1, 3).Value = "CheminPhoto";
            ws.Cell(1, 4).Value = "Description";
            ws.Cell(1, 5).Value = "DateAjout";
            wb.SaveAs(GalerieFile);
        }
    }

    // ==================== ÉCOLES ====================

    public List<Ecole> GetAllEcoles()
    {
        lock (_lock)
        {
            var ecoles = new List<Ecole>();
            using var wb = new XLWorkbook(EcolesFile);
            var ws = wb.Worksheet("Ecoles");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                var idCell = ws.Cell(row, 1).Value;
                if (idCell.IsBlank) continue;

                var liensJson = ws.Cell(row, 7).GetString();
                List<LienSocial> liens = new();
                if (!string.IsNullOrWhiteSpace(liensJson))
                {
                    try { liens = System.Text.Json.JsonSerializer.Deserialize<List<LienSocial>>(liensJson) ?? new(); }
                    catch { liens = new(); }
                }
                ecoles.Add(new Ecole
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    Nom = ws.Cell(row, 2).GetString(),
                    NomEquipe = ws.Cell(row, 3).GetString(),
                    LogoPath = ws.Cell(row, 4).GetString() is string s && s.Length > 0 ? s : null,
                    CouleurPrimaire = ws.Cell(row, 5).GetString() is string cp && cp.Length > 0 ? cp : "#1a3a5c",
                    CouleurSecondaire = ws.Cell(row, 6).GetString() is string cs && cs.Length > 0 ? cs : "#e8a020",
                    LiensSociaux = liens
                });
            }
            // Charger les thèmes dans chaque école
            var themes = GetAllThemes();
            foreach (var ecole in ecoles)
                ecole.Themes = themes.Where(t => t.EcoleId == ecole.Id).ToList();
            return ecoles;
        }
    }

    public Ecole? GetEcoleById(int id)
        => GetAllEcoles().FirstOrDefault(e => e.Id == id);

    public Ecole AddEcole(Ecole ecole)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(EcolesFile);
            var ws = wb.Worksheet("Ecoles");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var ecoles = GetAllEcoles();
            ecole.Id = ecoles.Count > 0 ? ecoles.Max(e => e.Id) + 1 : 1;
            int newRow = lastRow + 1;
            WriteEcoleRow(ws, newRow, ecole);
            wb.Save();
            return ecole;
        }
    }

    public Ecole UpdateEcole(Ecole ecole)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(EcolesFile);
            var ws = wb.Worksheet("Ecoles");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == ecole.Id)
                {
                    WriteEcoleRow(ws, row, ecole);
                    break;
                }
            }
            wb.Save();
            return ecole;
        }
    }

    public bool DeleteEcole(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(EcolesFile);
            var ws = wb.Worksheet("Ecoles");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    private static void WriteEcoleRow(IXLWorksheet ws, int row, Ecole ecole)
    {
        ws.Cell(row, 1).Value = ecole.Id;
        ws.Cell(row, 2).Value = ecole.Nom;
        ws.Cell(row, 3).Value = ecole.NomEquipe;
        ws.Cell(row, 4).Value = ecole.LogoPath ?? string.Empty;
        ws.Cell(row, 5).Value = ecole.CouleurPrimaire;
        ws.Cell(row, 6).Value = ecole.CouleurSecondaire;
        ws.Cell(row, 7).Value = ecole.LiensSociaux.Count > 0
            ? System.Text.Json.JsonSerializer.Serialize(ecole.LiensSociaux)
            : string.Empty;
    }

    // ==================== ÉQUIPES ====================

    public List<Equipe> GetAllEquipes()
    {
        lock (_lock)
        {
            var equipes = new List<Equipe>();
            using var wb = new XLWorkbook(EquipesFile);
            var ws = wb.Worksheet("Equipes");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (ws.Cell(row, 1).Value.IsBlank) continue;

                var themeIdStr = ws.Cell(row, 8).GetString();
                equipes.Add(new Equipe
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    EcoleId = (int)ws.Cell(row, 2).GetDouble(),
                    AnneeScolaire = ws.Cell(row, 3).GetString(),
                    TypeSport = Enum.Parse<TypeSport>(ws.Cell(row, 4).GetString()),
                    Niveau = Enum.Parse<NiveauEquipe>(ws.Cell(row, 5).GetString()),
                    Nom = ws.Cell(row, 6).GetString(),
                    AfficherPublic = ws.Cell(row, 7).GetString().Equals("true", StringComparison.OrdinalIgnoreCase),
                    ThemeId = int.TryParse(themeIdStr, out var tid) ? tid : null
                });
            }
            return equipes;
        }
    }

    public List<Equipe> GetEquipesByEcole(int ecoleId)
        => GetAllEquipes().Where(e => e.EcoleId == ecoleId).ToList();

    public Equipe? GetEquipeById(int id)
        => GetAllEquipes().FirstOrDefault(e => e.Id == id);

    public Equipe AddEquipe(Equipe equipe)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(EquipesFile);
            var ws = wb.Worksheet("Equipes");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var equipes = GetAllEquipes();
            equipe.Id = equipes.Count > 0 ? equipes.Max(e => e.Id) + 1 : 1;
            WriteEquipeRow(ws, lastRow + 1, equipe);
            wb.Save();
            return equipe;
        }
    }

    public Equipe UpdateEquipe(Equipe equipe)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(EquipesFile);
            var ws = wb.Worksheet("Equipes");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == equipe.Id)
                {
                    WriteEquipeRow(ws, row, equipe);
                    break;
                }
            }
            wb.Save();
            return equipe;
        }
    }

    public bool DeleteEquipe(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(EquipesFile);
            var ws = wb.Worksheet("Equipes");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    private static void WriteEquipeRow(IXLWorksheet ws, int row, Equipe equipe)
    {
        ws.Cell(row, 1).Value = equipe.Id;
        ws.Cell(row, 2).Value = equipe.EcoleId;
        ws.Cell(row, 3).Value = equipe.AnneeScolaire;
        ws.Cell(row, 4).Value = equipe.TypeSport.ToString();
        ws.Cell(row, 5).Value = equipe.Niveau.ToString();
        ws.Cell(row, 6).Value = equipe.Nom;
        ws.Cell(row, 7).Value = equipe.AfficherPublic.ToString().ToLower();
        ws.Cell(row, 8).Value = equipe.ThemeId.HasValue ? equipe.ThemeId.Value.ToString() : string.Empty;
    }

    // ==================== THÈMES ====================

    public List<ThemeEcole> GetAllThemes()
    {
        lock (_lock)
        {
            var themes = new List<ThemeEcole>();
            using var wb = new XLWorkbook(ThemesFile);
            var ws = wb.Worksheet("Themes");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (ws.Cell(row, 1).Value.IsBlank) continue;
                themes.Add(new ThemeEcole
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    EcoleId = (int)ws.Cell(row, 2).GetDouble(),
                    NomEquipe = ws.Cell(row, 3).GetString(),
                    CouleurPrimaire = ws.Cell(row, 4).GetString() is string cp && cp.Length > 0 ? cp : "#1a3a5c",
                    CouleurSecondaire = ws.Cell(row, 5).GetString() is string cs && cs.Length > 0 ? cs : "#e8a020",
                    LogoPath = ws.Cell(row, 6).GetString() is string lp && lp.Length > 0 ? lp : null
                });
            }
            return themes;
        }
    }

    public List<ThemeEcole> GetThemesByEcole(int ecoleId)
        => GetAllThemes().Where(t => t.EcoleId == ecoleId).ToList();

    public ThemeEcole? GetThemeById(int id)
        => GetAllThemes().FirstOrDefault(t => t.Id == id);

    public ThemeEcole AddTheme(ThemeEcole theme)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(ThemesFile);
            var ws = wb.Worksheet("Themes");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var all = GetAllThemes();
            theme.Id = all.Count > 0 ? all.Max(t => t.Id) + 1 : 1;
            WriteThemeRow(ws, lastRow + 1, theme);
            wb.Save();
            return theme;
        }
    }

    public ThemeEcole UpdateTheme(ThemeEcole theme)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(ThemesFile);
            var ws = wb.Worksheet("Themes");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == theme.Id)
                {
                    WriteThemeRow(ws, row, theme);
                    break;
                }
            }
            wb.Save();
            return theme;
        }
    }

    public bool DeleteTheme(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(ThemesFile);
            var ws = wb.Worksheet("Themes");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    private static void WriteThemeRow(IXLWorksheet ws, int row, ThemeEcole theme)
    {
        ws.Cell(row, 1).Value = theme.Id;
        ws.Cell(row, 2).Value = theme.EcoleId;
        ws.Cell(row, 3).Value = theme.NomEquipe;
        ws.Cell(row, 4).Value = theme.CouleurPrimaire;
        ws.Cell(row, 5).Value = theme.CouleurSecondaire;
        ws.Cell(row, 6).Value = theme.LogoPath ?? string.Empty;
    }

    // ==================== JOUEURS ====================

    public List<Joueur> GetAllJoueurs()
    {
        lock (_lock)
        {
            var joueurs = new List<Joueur>();
            using var wb = new XLWorkbook(JoueursFile);
            var ws = wb.Worksheet("Joueurs");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (ws.Cell(row, 1).Value.IsBlank) continue;

                joueurs.Add(new Joueur
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    EquipeId = (int)ws.Cell(row, 2).GetDouble(),
                    Nom = ws.Cell(row, 3).GetString(),
                    Prenom = ws.Cell(row, 4).GetString(),
                    Numero = ws.Cell(row, 5).GetString(),
                    Position = System.Net.WebUtility.HtmlDecode(ws.Cell(row, 6).GetString()),
                    PhotoPath = ws.Cell(row, 7).GetString() is string p && p.Length > 0 ? p : null,
                    NoFiche = ws.Cell(row, 8).GetString() is string nf && nf.Length > 0 ? nf : null,
                    PositionSpecifique = ws.Cell(row, 9).GetString() is string ps && ps.Length > 0 ? System.Net.WebUtility.HtmlDecode(ps) : null,
                    Description = ws.Cell(row, 10).GetString() is string desc && desc.Length > 0 ? desc : null,
                    ConsentementPhoto = ws.Cell(row, 11).Value.IsBlank || ws.Cell(row, 11).GetString().ToLowerInvariant() != "false",
                    Actif = ws.Cell(row, 12).Value.IsBlank || ws.Cell(row, 12).GetString().ToLowerInvariant() != "false"
                });
            }
            return joueurs;
        }
    }

    public List<Joueur> GetJoueursByEquipe(int equipeId)
        => GetAllJoueurs().Where(j => j.EquipeId == equipeId).ToList();

    public Joueur? GetJoueurById(int id)
        => GetAllJoueurs().FirstOrDefault(j => j.Id == id);

    public Joueur AddJoueur(Joueur joueur)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(JoueursFile);
            var ws = wb.Worksheet("Joueurs");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var joueurs = GetAllJoueurs();
            joueur.Id = joueurs.Count > 0 ? joueurs.Max(j => j.Id) + 1 : 1;
            WriteJoueurRow(ws, lastRow + 1, joueur);
            wb.Save();
            return joueur;
        }
    }

    public Joueur UpdateJoueur(Joueur joueur)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(JoueursFile);
            var ws = wb.Worksheet("Joueurs");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == joueur.Id)
                {
                    WriteJoueurRow(ws, row, joueur);
                    break;
                }
            }
            wb.Save();
            return joueur;
        }
    }

    public bool DeleteJoueur(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(JoueursFile);
            var ws = wb.Worksheet("Joueurs");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    private static void WriteJoueurRow(IXLWorksheet ws, int row, Joueur joueur)
    {
        ws.Cell(row, 1).Value = joueur.Id;
        ws.Cell(row, 2).Value = joueur.EquipeId;
        ws.Cell(row, 3).Value = joueur.Nom;
        ws.Cell(row, 4).Value = joueur.Prenom;
        ws.Cell(row, 5).Value = joueur.Numero;
        ws.Cell(row, 6).Value = joueur.Position;
        ws.Cell(row, 7).Value = joueur.PhotoPath ?? string.Empty;
        ws.Cell(row, 8).Value = joueur.NoFiche ?? string.Empty;
        ws.Cell(row, 9).Value = joueur.PositionSpecifique ?? string.Empty;
        ws.Cell(row, 10).Value = joueur.Description ?? string.Empty;
        ws.Cell(row, 11).Value = joueur.ConsentementPhoto.ToString().ToLower();
        ws.Cell(row, 12).Value = joueur.Actif.ToString().ToLower();
    }

    // ==================== GALERIE ====================

    public List<GaleriePhoto> GetAllPhotos()
    {
        lock (_lock)
        {
            var photos = new List<GaleriePhoto>();
            using var wb = new XLWorkbook(GalerieFile);
            var ws = wb.Worksheet("Galerie");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (ws.Cell(row, 1).Value.IsBlank) continue;

                DateTime dateAjout = DateTime.Now;
                var dateCell = ws.Cell(row, 5);
                if (!dateCell.Value.IsBlank)
                {
                    if (dateCell.Value.IsDateTime)
                        dateAjout = dateCell.GetDateTime();
                    else if (DateTime.TryParse(dateCell.GetString(), out var d))
                        dateAjout = d;
                }

                photos.Add(new GaleriePhoto
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    EquipeId = (int)ws.Cell(row, 2).GetDouble(),
                    CheminPhoto = ws.Cell(row, 3).GetString(),
                    Description = ws.Cell(row, 4).GetString() is string desc && desc.Length > 0 ? desc : null,
                    DateAjout = dateAjout
                });
            }
            return photos;
        }
    }

    public List<GaleriePhoto> GetPhotosByEquipe(int equipeId)
        => GetAllPhotos().Where(p => p.EquipeId == equipeId).ToList();

    public GaleriePhoto? GetPhotoById(int id)
        => GetAllPhotos().FirstOrDefault(p => p.Id == id);

    public GaleriePhoto AddPhoto(GaleriePhoto photo)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(GalerieFile);
            var ws = wb.Worksheet("Galerie");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var photos = GetAllPhotos();
            photo.Id = photos.Count > 0 ? photos.Max(p => p.Id) + 1 : 1;
            WritePhotoRow(ws, lastRow + 1, photo);
            wb.Save();
            return photo;
        }
    }

    public GaleriePhoto UpdatePhoto(GaleriePhoto photo)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(GalerieFile);
            var ws = wb.Worksheet("Galerie");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == photo.Id)
                {
                    WritePhotoRow(ws, row, photo);
                    break;
                }
            }
            wb.Save();
            return photo;
        }
    }

    public bool DeletePhoto(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(GalerieFile);
            var ws = wb.Worksheet("Galerie");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    private static void WritePhotoRow(IXLWorksheet ws, int row, GaleriePhoto photo)
    {
        ws.Cell(row, 1).Value = photo.Id;
        ws.Cell(row, 2).Value = photo.EquipeId;
        ws.Cell(row, 3).Value = photo.CheminPhoto;
        ws.Cell(row, 4).Value = photo.Description ?? string.Empty;
        ws.Cell(row, 5).Value = photo.DateAjout;
    }

    // ==================== STAFF ====================

    private void InitStaffFile()
    {
        if (!File.Exists(StaffFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Staff");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "EquipeId";
            ws.Cell(1, 3).Value = "Nom";
            ws.Cell(1, 4).Value = "Prenom";
            ws.Cell(1, 5).Value = "Titre";
            ws.Cell(1, 6).Value = "ResponsableDe";
            ws.Cell(1, 7).Value = "Description";
            ws.Cell(1, 8).Value = "PhotoPath";
            ws.Cell(1, 9).Value = "NoFiche";
            wb.SaveAs(StaffFile);
        }
    }

    public List<Staff> GetAllStaff()
    {
        lock (_lock)
        {
            var list = new List<Staff>();
            using var wb = new XLWorkbook(StaffFile);
            var ws = wb.Worksheet("Staff");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (ws.Cell(row, 1).Value.IsBlank) continue;
                list.Add(new Staff
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    EquipeId = (int)ws.Cell(row, 2).GetDouble(),
                    Nom = ws.Cell(row, 3).GetString(),
                    Prenom = ws.Cell(row, 4).GetString(),
                    Titre = ws.Cell(row, 5).GetString(),
                    Description = ws.Cell(row, 7).GetString() is string d && d.Length > 0 ? d : null,
                    PhotoPath = ws.Cell(row, 8).GetString() is string p && p.Length > 0 ? p : null,
                    NoFiche = ws.Cell(row, 9).GetString() is string nf && nf.Length > 0 ? nf : null
                });
            }
            return list;
        }
    }

    public List<Staff> GetStaffByEquipe(int equipeId)
        => GetAllStaff().Where(s => s.EquipeId == equipeId).ToList();

    public Staff? GetStaffById(int id)
        => GetAllStaff().FirstOrDefault(s => s.Id == id);

    public List<Staff> GetStaffByNoFiche(string noFiche)
        => GetAllStaff().Where(s => s.NoFiche == noFiche).ToList();

    public Staff AddStaff(Staff staff)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(StaffFile);
            var ws = wb.Worksheet("Staff");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var list = GetAllStaff();
            staff.Id = list.Count > 0 ? list.Max(s => s.Id) + 1 : 1;
            WriteStaffRow(ws, lastRow + 1, staff);
            wb.Save();
            return staff;
        }
    }

    public Staff UpdateStaff(Staff staff)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(StaffFile);
            var ws = wb.Worksheet("Staff");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == staff.Id)
                {
                    WriteStaffRow(ws, row, staff);
                    break;
                }
            }
            wb.Save();
            return staff;
        }
    }

    public bool DeleteStaff(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(StaffFile);
            var ws = wb.Worksheet("Staff");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    private static void WriteStaffRow(IXLWorksheet ws, int row, Staff staff)
    {
        ws.Cell(row, 1).Value = staff.Id;
        ws.Cell(row, 2).Value = staff.EquipeId;
        ws.Cell(row, 3).Value = staff.Nom;
        ws.Cell(row, 4).Value = staff.Prenom;
        ws.Cell(row, 5).Value = staff.Titre;
        ws.Cell(row, 6).Value = string.Empty; // ancien champ Role (conservé pour compatibilité colonne)
        ws.Cell(row, 7).Value = staff.Description ?? string.Empty;
        ws.Cell(row, 8).Value = staff.PhotoPath ?? string.Empty;
        ws.Cell(row, 9).Value = staff.NoFiche ?? string.Empty;
    }

    // ==================== MATCHS ====================

    private void InitMatchsFile()
    {
        if (!File.Exists(MatchsFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Matchs");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "EquipeId";
            ws.Cell(1, 3).Value = "DateMatch";
            ws.Cell(1, 4).Value = "HeureArriveeVestiaire";
            ws.Cell(1, 5).Value = "HeureDepartAutobus";
            ws.Cell(1, 6).Value = "HeureDebutMatch";
            ws.Cell(1, 7).Value = "EstDomicile";
            ws.Cell(1, 8).Value = "Adversaire";
            ws.Cell(1, 9).Value = "Lieu";
            ws.Cell(1, 10).Value = "ScoreEquipe";
            ws.Cell(1, 11).Value = "ScoreAdversaire";
            ws.Cell(1, 12).Value = "Notes";
            wb.SaveAs(MatchsFile);
        }
    }

    private void InitMatchMediasFile()
    {
        if (!File.Exists(MatchMediasFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("MatchMedias");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "MatchId";
            ws.Cell(1, 3).Value = "CheminFichier";
            ws.Cell(1, 4).Value = "TypeMedia";
            ws.Cell(1, 5).Value = "Description";
            ws.Cell(1, 6).Value = "DateAjout";
            wb.SaveAs(MatchMediasFile);
        }
    }

    private void InitJoueurMediasFile()
    {
        if (!File.Exists(JoueurMediasFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("JoueurMedias");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "JoueurId";
            ws.Cell(1, 3).Value = "CheminFichier";
            ws.Cell(1, 4).Value = "Description";
            ws.Cell(1, 5).Value = "DateAjout";
            wb.SaveAs(JoueurMediasFile);
        }
    }

    private void InitAbsencesFile()
    {
        if (!File.Exists(AbsencesFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Absences");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "MatchId";
            ws.Cell(1, 3).Value = "JoueurId";
            ws.Cell(1, 4).Value = "Raison";
            wb.SaveAs(AbsencesFile);
        }
    }

    public List<Match> GetAllMatchs()
    {
        lock (_lock)
        {
            var matchs = new List<Match>();
            using var wb = new XLWorkbook(MatchsFile);
            var ws = wb.Worksheet("Matchs");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (ws.Cell(row, 1).Value.IsBlank) continue;

                DateTime dateMatch = DateTime.Today;
                var dateCell = ws.Cell(row, 3);
                if (!dateCell.Value.IsBlank)
                {
                    if (dateCell.Value.IsDateTime) dateMatch = dateCell.GetDateTime();
                    else if (DateTime.TryParse(dateCell.GetString(), out var d)) dateMatch = d;
                }

                int? scoreEquipe = null, scoreAdversaire = null;
                var scoreEq = ws.Cell(row, 10);
                var scoreAdv = ws.Cell(row, 11);
                if (!scoreEq.Value.IsBlank) scoreEquipe = (int)scoreEq.GetDouble();
                if (!scoreAdv.Value.IsBlank) scoreAdversaire = (int)scoreAdv.GetDouble();

                matchs.Add(new Match
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    EquipeId = (int)ws.Cell(row, 2).GetDouble(),
                    DateMatch = dateMatch,
                    HeureArriveeVestiaire = ws.Cell(row, 4).GetString() is string h1 && h1.Length > 0 ? h1 : null,
                    HeureDepartAutobus = ws.Cell(row, 5).GetString() is string h2 && h2.Length > 0 ? h2 : null,
                    HeureDebutMatch = ws.Cell(row, 6).GetString() is string h3 && h3.Length > 0 ? h3 : null,
                    EstDomicile = ws.Cell(row, 7).GetString().Equals("true", StringComparison.OrdinalIgnoreCase),
                    Adversaire = ws.Cell(row, 8).GetString(),
                    Lieu = ws.Cell(row, 9).GetString() is string lieu && lieu.Length > 0 ? lieu : null,
                    ScoreEquipe = scoreEquipe,
                    ScoreAdversaire = scoreAdversaire,
                    Notes = ws.Cell(row, 12).GetString() is string notes && notes.Length > 0 ? notes : null
                });
            }
            return matchs;
        }
    }

    public List<Match> GetMatchsByEquipe(int equipeId)
        => GetAllMatchs().Where(m => m.EquipeId == equipeId).ToList();

    public Match? GetMatchById(int id)
        => GetAllMatchs().FirstOrDefault(m => m.Id == id);

    public Match AddMatch(Match match)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(MatchsFile);
            var ws = wb.Worksheet("Matchs");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var matchs = GetAllMatchs();
            match.Id = matchs.Count > 0 ? matchs.Max(m => m.Id) + 1 : 1;
            WriteMatchRow(ws, lastRow + 1, match);
            wb.Save();
            return match;
        }
    }

    public Match UpdateMatch(Match match)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(MatchsFile);
            var ws = wb.Worksheet("Matchs");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == match.Id)
                {
                    WriteMatchRow(ws, row, match);
                    break;
                }
            }
            wb.Save();
            return match;
        }
    }

    public bool DeleteMatch(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(MatchsFile);
            var ws = wb.Worksheet("Matchs");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    private static void WriteMatchRow(IXLWorksheet ws, int row, Match match)
    {
        ws.Cell(row, 1).Value = match.Id;
        ws.Cell(row, 2).Value = match.EquipeId;
        ws.Cell(row, 3).Value = match.DateMatch;
        ws.Cell(row, 4).Value = match.HeureArriveeVestiaire ?? string.Empty;
        ws.Cell(row, 5).Value = match.HeureDepartAutobus ?? string.Empty;
        ws.Cell(row, 6).Value = match.HeureDebutMatch ?? string.Empty;
        ws.Cell(row, 7).Value = match.EstDomicile.ToString().ToLower();
        ws.Cell(row, 8).Value = match.Adversaire;
        ws.Cell(row, 9).Value = match.Lieu ?? string.Empty;
        if (match.ScoreEquipe.HasValue) ws.Cell(row, 10).Value = match.ScoreEquipe.Value;
        else ws.Cell(row, 10).Value = XLCellValue.FromObject(null);
        if (match.ScoreAdversaire.HasValue) ws.Cell(row, 11).Value = match.ScoreAdversaire.Value;
        else ws.Cell(row, 11).Value = XLCellValue.FromObject(null);
        ws.Cell(row, 12).Value = match.Notes ?? string.Empty;
    }

    // ==================== MÉDIAS DE MATCH ====================

    public List<MatchMedia> GetAllMatchMedias()
    {
        lock (_lock)
        {
            var medias = new List<MatchMedia>();
            using var wb = new XLWorkbook(MatchMediasFile);
            var ws = wb.Worksheet("MatchMedias");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (ws.Cell(row, 1).Value.IsBlank) continue;

                DateTime dateAjout = DateTime.UtcNow;
                var dateCell = ws.Cell(row, 6);
                if (!dateCell.Value.IsBlank)
                {
                    if (dateCell.Value.IsDateTime) dateAjout = dateCell.GetDateTime();
                    else if (DateTime.TryParse(dateCell.GetString(), out var d)) dateAjout = d;
                }

                Enum.TryParse<TypeMedia>(ws.Cell(row, 4).GetString(), out var typeMedia);

                medias.Add(new MatchMedia
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    MatchId = (int)ws.Cell(row, 2).GetDouble(),
                    CheminFichier = ws.Cell(row, 3).GetString(),
                    TypeMedia = typeMedia,
                    Description = ws.Cell(row, 5).GetString() is string desc && desc.Length > 0 ? desc : null,
                    DateAjout = dateAjout
                });
            }
            return medias;
        }
    }

    public List<MatchMedia> GetMediasByMatch(int matchId)
        => GetAllMatchMedias().Where(m => m.MatchId == matchId).ToList();

    public MatchMedia? GetMatchMediaById(int id)
        => GetAllMatchMedias().FirstOrDefault(m => m.Id == id);

    public MatchMedia AddMatchMedia(MatchMedia media)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(MatchMediasFile);
            var ws = wb.Worksheet("MatchMedias");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var medias = GetAllMatchMedias();
            media.Id = medias.Count > 0 ? medias.Max(m => m.Id) + 1 : 1;
            WriteMatchMediaRow(ws, lastRow + 1, media);
            wb.Save();
            return media;
        }
    }

    public bool DeleteMatchMedia(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(MatchMediasFile);
            var ws = wb.Worksheet("MatchMedias");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    private static void WriteMatchMediaRow(IXLWorksheet ws, int row, MatchMedia media)
    {
        ws.Cell(row, 1).Value = media.Id;
        ws.Cell(row, 2).Value = media.MatchId;
        ws.Cell(row, 3).Value = media.CheminFichier;
        ws.Cell(row, 4).Value = media.TypeMedia.ToString();
        ws.Cell(row, 5).Value = media.Description ?? string.Empty;
        ws.Cell(row, 6).Value = media.DateAjout;
    }

    // ==================== MÉDIAS JOUEUR ====================

    public List<JoueurMedia> GetAllJoueurMedias()
    {
        lock (_lock)
        {
            var medias = new List<JoueurMedia>();
            using var wb = new XLWorkbook(JoueurMediasFile);
            var ws = wb.Worksheet("JoueurMedias");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (ws.Cell(row, 1).Value.IsBlank) continue;
                DateTime dateAjout = DateTime.UtcNow;
                var dateCell = ws.Cell(row, 5);
                if (!dateCell.Value.IsBlank)
                {
                    if (dateCell.Value.IsDateTime) dateAjout = dateCell.GetDateTime();
                    else if (DateTime.TryParse(dateCell.GetString(), out var d)) dateAjout = d;
                }
                medias.Add(new JoueurMedia
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    JoueurId = (int)ws.Cell(row, 2).GetDouble(),
                    CheminFichier = ws.Cell(row, 3).GetString(),
                    Description = ws.Cell(row, 4).GetString() is string desc && desc.Length > 0 ? desc : null,
                    DateAjout = dateAjout
                });
            }
            return medias;
        }
    }

    public List<JoueurMedia> GetMediasByJoueur(int joueurId)
        => GetAllJoueurMedias().Where(m => m.JoueurId == joueurId).ToList();

    public JoueurMedia AddJoueurMedia(JoueurMedia media)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(JoueurMediasFile);
            var ws = wb.Worksheet("JoueurMedias");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var all = GetAllJoueurMedias();
            media.Id = all.Count > 0 ? all.Max(m => m.Id) + 1 : 1;
            ws.Cell(lastRow + 1, 1).Value = media.Id;
            ws.Cell(lastRow + 1, 2).Value = media.JoueurId;
            ws.Cell(lastRow + 1, 3).Value = media.CheminFichier;
            ws.Cell(lastRow + 1, 4).Value = media.Description ?? string.Empty;
            ws.Cell(lastRow + 1, 5).Value = media.DateAjout;
            wb.Save();
            return media;
        }
    }

    public bool DeleteJoueurMedia(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(JoueurMediasFile);
            var ws = wb.Worksheet("JoueurMedias");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    // ==================== ABSENCES MATCH ====================

    public List<AbsenceMatch> GetAllAbsences()
    {
        lock (_lock)
        {
            var list = new List<AbsenceMatch>();
            using var wb = new XLWorkbook(AbsencesFile);
            var ws = wb.Worksheet("Absences");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (ws.Cell(row, 1).Value.IsBlank) continue;
                list.Add(new AbsenceMatch
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    MatchId = (int)ws.Cell(row, 2).GetDouble(),
                    JoueurId = (int)ws.Cell(row, 3).GetDouble(),
                    Raison = ws.Cell(row, 4).GetString() is string r && r.Length > 0 ? r : null
                });
            }
            return list;
        }
    }

    public List<AbsenceMatch> GetAbsencesByMatch(int matchId)
        => GetAllAbsences().Where(a => a.MatchId == matchId).ToList();

    public AbsenceMatch AddAbsence(AbsenceMatch absence)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(AbsencesFile);
            var ws = wb.Worksheet("Absences");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var all = GetAllAbsences();
            absence.Id = all.Count > 0 ? all.Max(a => a.Id) + 1 : 1;
            ws.Cell(lastRow + 1, 1).Value = absence.Id;
            ws.Cell(lastRow + 1, 2).Value = absence.MatchId;
            ws.Cell(lastRow + 1, 3).Value = absence.JoueurId;
            ws.Cell(lastRow + 1, 4).Value = absence.Raison ?? string.Empty;
            wb.Save();
            return absence;
        }
    }

    public bool DeleteAbsence(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(AbsencesFile);
            var ws = wb.Worksheet("Absences");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    private void InitEvenementsFile()
    {
        if (!File.Exists(EvenementsFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Evenements");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "EquipeId";
            ws.Cell(1, 3).Value = "Titre";
            ws.Cell(1, 4).Value = "Type";
            ws.Cell(1, 5).Value = "DateDebut";
            ws.Cell(1, 6).Value = "DateFin";
            ws.Cell(1, 7).Value = "Lieu";
            ws.Cell(1, 8).Value = "Notes";
            wb.SaveAs(EvenementsFile);
        }
    }

    // ==================== ÉVÉNEMENTS ====================

    public List<Evenement> GetAllEvenements()
    {
        lock (_lock)
        {
            var list = new List<Evenement>();
            using var wb = new XLWorkbook(EvenementsFile);
            var ws = wb.Worksheet("Evenements");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (ws.Cell(row, 1).Value.IsBlank) continue;
                DateTime dateDebut = DateTime.Today;
                var dbCell = ws.Cell(row, 5);
                if (!dbCell.Value.IsBlank)
                {
                    if (dbCell.Value.IsDateTime) dateDebut = dbCell.GetDateTime();
                    else if (DateTime.TryParse(dbCell.GetString(), out var d)) dateDebut = d;
                }
                DateTime? dateFin = null;
                var dfCell = ws.Cell(row, 6);
                if (!dfCell.Value.IsBlank)
                {
                    if (dfCell.Value.IsDateTime) dateFin = dfCell.GetDateTime();
                    else if (DateTime.TryParse(dfCell.GetString(), out var df)) dateFin = df;
                }
                Enum.TryParse<TypeEvenement>(ws.Cell(row, 4).GetString(), out var type);
                list.Add(new Evenement
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    EquipeId = (int)ws.Cell(row, 2).GetDouble(),
                    Titre = ws.Cell(row, 3).GetString(),
                    Type = type,
                    DateDebut = dateDebut,
                    DateFin = dateFin,
                    Lieu = ws.Cell(row, 7).GetString() is string l && l.Length > 0 ? l : null,
                    Notes = ws.Cell(row, 8).GetString() is string n && n.Length > 0 ? n : null,
                });
            }
            return list;
        }
    }

    public List<Evenement> GetEvenementsByEquipe(int equipeId)
        => GetAllEvenements().Where(e => e.EquipeId == equipeId).ToList();

    public Evenement? GetEvenementById(int id)
        => GetAllEvenements().FirstOrDefault(e => e.Id == id);

    public Evenement AddEvenement(Evenement ev)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(EvenementsFile);
            var ws = wb.Worksheet("Evenements");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var all = GetAllEvenements();
            ev.Id = all.Count > 0 ? all.Max(e => e.Id) + 1 : 1;
            WriteEvenementRow(ws, lastRow + 1, ev);
            wb.Save();
            return ev;
        }
    }

    public Evenement UpdateEvenement(Evenement ev)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(EvenementsFile);
            var ws = wb.Worksheet("Evenements");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == ev.Id)
                {
                    WriteEvenementRow(ws, row, ev);
                    break;
                }
            }
            wb.Save();
            return ev;
        }
    }

    public bool DeleteEvenement(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(EvenementsFile);
            var ws = wb.Worksheet("Evenements");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    private static void WriteEvenementRow(IXLWorksheet ws, int row, Evenement ev)
    {
        ws.Cell(row, 1).Value = ev.Id;
        ws.Cell(row, 2).Value = ev.EquipeId;
        ws.Cell(row, 3).Value = ev.Titre;
        ws.Cell(row, 4).Value = ev.Type.ToString();
        ws.Cell(row, 5).Value = ev.DateDebut.ToString("yyyy-MM-dd HH:mm");
        ws.Cell(row, 6).Value = ev.DateFin.HasValue ? ev.DateFin.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty;
        ws.Cell(row, 7).Value = ev.Lieu ?? string.Empty;
        ws.Cell(row, 8).Value = ev.Notes ?? string.Empty;
    }

    public bool DeleteAbsenceByMatchJoueur(int matchId, int joueurId)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(AbsencesFile);
            var ws = wb.Worksheet("Absences");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            bool found = false;
            for (int row = lastRow; row >= 2; row--)
            {
                if (!ws.Cell(row, 1).Value.IsBlank
                    && (int)ws.Cell(row, 2).GetDouble() == matchId
                    && (int)ws.Cell(row, 3).GetDouble() == joueurId)
                {
                    ws.Row(row).Delete();
                    found = true;
                }
            }
            if (found) wb.Save();
            return found;
        }
    }

    // ==================== DICTIONNAIRES ====================

    private void InitDictionnaireFile()
    {
        if (!File.Exists(DictionnaireFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Dictionnaires");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Categorie";
            ws.Cell(1, 3).Value = "Sport";
            ws.Cell(1, 4).Value = "Valeur";
            ws.Cell(1, 5).Value = "Label";
            ws.Cell(1, 6).Value = "Acronyme";
            ws.Cell(1, 7).Value = "Description";
            ws.Cell(1, 8).Value = "Ordre";
            wb.SaveAs(DictionnaireFile);
        }
    }

    public List<DictionnaireEntree> GetAllDictionnaire()
    {
        lock (_lock)
        {
            var list = new List<DictionnaireEntree>();
            using var wb = new XLWorkbook(DictionnaireFile);
            var ws = wb.Worksheet("Dictionnaires");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (ws.Cell(row, 1).Value.IsBlank) continue;
                list.Add(new DictionnaireEntree
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    Categorie = ws.Cell(row, 2).GetString(),
                    Sport = ws.Cell(row, 3).GetString(),
                    Valeur = ws.Cell(row, 4).GetString(),
                    Label = ws.Cell(row, 5).GetString(),
                    Acronyme = ws.Cell(row, 6).GetString(),
                    Description = ws.Cell(row, 7).GetString(),
                    Ordre = (int)ws.Cell(row, 8).GetDouble(),
                    ParentValeur = ws.Cell(row, 9).GetString(),
                    Actif = ws.Cell(row, 10).GetString() is not "false"
                });
            }
            return list;
        }
    }

    public DictionnaireEntree AddDictionnaire(DictionnaireEntree entree)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(DictionnaireFile);
            var ws = wb.Worksheet("Dictionnaires");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var list = GetAllDictionnaire();
            entree.Id = list.Count > 0 ? list.Max(e => e.Id) + 1 : 1;
            if (entree.Ordre == 0)
                entree.Ordre = list.Count(e => e.Categorie == entree.Categorie && e.Sport == entree.Sport) + 1;
            WriteDictionnaireRow(ws, lastRow + 1, entree);
            wb.Save();
            return entree;
        }
    }

    public bool DeleteDictionnaire(int id)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(DictionnaireFile);
            var ws = wb.Worksheet("Dictionnaires");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == id)
                {
                    ws.Row(row).Delete();
                    wb.Save();
                    return true;
                }
            }
            return false;
        }
    }

    public DictionnaireEntree UpdateDictionnaire(DictionnaireEntree entree)
    {
        lock (_lock)
        {
            using var wb = new XLWorkbook(DictionnaireFile);
            var ws = wb.Worksheet("Dictionnaires");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                if (!ws.Cell(row, 1).Value.IsBlank && (int)ws.Cell(row, 1).GetDouble() == entree.Id)
                {
                    WriteDictionnaireRow(ws, row, entree);
                    break;
                }
            }
            wb.Save();
            return entree;
        }
    }

    private static void WriteDictionnaireRow(IXLWorksheet ws, int row, DictionnaireEntree entree)
    {
        ws.Cell(row, 1).Value = entree.Id;
        ws.Cell(row, 2).Value = entree.Categorie;
        ws.Cell(row, 3).Value = entree.Sport;
        ws.Cell(row, 4).Value = entree.Valeur;
        ws.Cell(row, 5).Value = entree.Label;
        ws.Cell(row, 6).Value = entree.Acronyme;
        ws.Cell(row, 7).Value = entree.Description;
        ws.Cell(row, 8).Value = entree.Ordre;
        ws.Cell(row, 9).Value = entree.ParentValeur;
        ws.Cell(row, 10).Value = entree.Actif ? "true" : "false";
    }
}
