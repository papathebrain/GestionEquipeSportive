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
    }

    private void InitEcolesFile()
    {
        if (!File.Exists(EcolesFile))
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Ecoles");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Nom";
            ws.Cell(1, 3).Value = "CodeEcole";
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
                    CodeEcole = ws.Cell(row, 3).GetString(),
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
        ws.Cell(row, 3).Value = ecole.CodeEcole;
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
                    Nom = ws.Cell(row, 6).GetString()
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
                    PhotoPath = ws.Cell(row, 7).GetString() is string p && p.Length > 0 ? p : null
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
}
