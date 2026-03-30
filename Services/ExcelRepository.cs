using ClosedXML.Excel;
using GestionEquipeSportive.Models;

namespace GestionEquipeSportive.Services;

public class ExcelRepository : IExcelRepository
{
    private readonly string _dataPath;
    private readonly object _lock = new object();

    private string EcolesFile => Path.Combine(_dataPath, "ecoles.xlsx");
    private string EquipesFile => Path.Combine(_dataPath, "equipes.xlsx");
    private string JoueursFile => Path.Combine(_dataPath, "joueurs.xlsx");
    private string GalerieFile => Path.Combine(_dataPath, "galerie.xlsx");
    private string StaffFile => Path.Combine(_dataPath, "staff.xlsx");
    private string MatchsFile => Path.Combine(_dataPath, "matchs.xlsx");
    private string MatchMediasFile => Path.Combine(_dataPath, "match_medias.xlsx");

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
        InitJoueursFile();
        InitGalerieFile();
        InitStaffFile();
        InitMatchsFile();
        InitMatchMediasFile();
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
            wb.SaveAs(EquipesFile);
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

                ecoles.Add(new Ecole
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    Nom = ws.Cell(row, 2).GetString(),
                    NomEquipe = ws.Cell(row, 3).GetString(),
                    LogoPath = ws.Cell(row, 4).GetString() is string s && s.Length > 0 ? s : null,
                    CouleurPrimaire = ws.Cell(row, 5).GetString() is string cp && cp.Length > 0 ? cp : "#1a3a5c",
                    CouleurSecondaire = ws.Cell(row, 6).GetString() is string cs && cs.Length > 0 ? cs : "#e8a020"
                });
            }
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

                equipes.Add(new Equipe
                {
                    Id = (int)ws.Cell(row, 1).GetDouble(),
                    EcoleId = (int)ws.Cell(row, 2).GetDouble(),
                    AnneeScolaire = ws.Cell(row, 3).GetString(),
                    TypeSport = Enum.Parse<TypeSport>(ws.Cell(row, 4).GetString()),
                    Niveau = Enum.Parse<NiveauEquipe>(ws.Cell(row, 5).GetString()),
                    Nom = ws.Cell(row, 6).GetString(),
                    AfficherPublic = ws.Cell(row, 7).GetString().Equals("true", StringComparison.OrdinalIgnoreCase)
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
                    Position = ws.Cell(row, 6).GetString(),
                    PhotoPath = ws.Cell(row, 7).GetString() is string p && p.Length > 0 ? p : null,
                    NoFiche = ws.Cell(row, 8).GetString() is string nf && nf.Length > 0 ? nf : null,
                    PositionSpecifique = ws.Cell(row, 9).GetString() is string ps && ps.Length > 0 ? ps : null,
                    Description = ws.Cell(row, 10).GetString() is string desc && desc.Length > 0 ? desc : null
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
            ws.Cell(1, 7).Value = "PhotoPath";
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
                    ResponsableDe = ws.Cell(row, 6).GetString() is string r && r.Length > 0 ? r : null,
                    PhotoPath = ws.Cell(row, 7).GetString() is string p && p.Length > 0 ? p : null
                });
            }
            return list;
        }
    }

    public List<Staff> GetStaffByEquipe(int equipeId)
        => GetAllStaff().Where(s => s.EquipeId == equipeId).ToList();

    public Staff? GetStaffById(int id)
        => GetAllStaff().FirstOrDefault(s => s.Id == id);

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
        ws.Cell(row, 6).Value = staff.ResponsableDe ?? string.Empty;
        ws.Cell(row, 7).Value = staff.PhotoPath ?? string.Empty;
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
}
