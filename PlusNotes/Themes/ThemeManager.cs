using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO.Compression;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.RegularExpressions;

namespace OpenNotes.Themes
{
    public partial class ThemeManager : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly string _themesFolderPath;
        private readonly string _activeThemeFilePath;
        private readonly string _downloadedThemesDirectory;

        [ObservableProperty]
        private ObservableCollection<ThemeInfo> _availableThemes = new();

        [ObservableProperty]
        private ThemeInfo? _currentTheme;

        public ThemeManager()
        {
            _httpClient = new HttpClient();
            _themesFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenNotes", "Themes");
            _activeThemeFilePath = Path.Combine(_themesFolderPath, "active_theme.json");
            _downloadedThemesDirectory = Path.Combine(_themesFolderPath, "Downloaded");
            
            // Créer le dossier des thèmes s'il n'existe pas
            if (!Directory.Exists(_themesFolderPath))
            {
                Directory.CreateDirectory(_themesFolderPath);
            }
            
            // S'assurer que le répertoire de téléchargement existe
            Directory.CreateDirectory(_downloadedThemesDirectory);

            // Ajouter les thèmes par défaut
            AvailableThemes.Add(new ThemeInfo { Id = "default_light", Name = "Clair (Défaut)", Type = ThemeType.Light, IsBuiltIn = true });
            AvailableThemes.Add(new ThemeInfo { Id = "default_dark", Name = "Sombre", Type = ThemeType.Dark, IsBuiltIn = true });
            AvailableThemes.Add(new ThemeInfo { Id = "default_system", Name = "Système", Type = ThemeType.System, IsBuiltIn = true });
            
            // Ajouter le thème translucide Mica
            AvailableThemes.Add(TranslucentMicaTheme.ThemeInfo);
        }

        public async Task InitializeAsync()
        {
            // Charger les thèmes personnalisés
            await LoadCustomThemesAsync();
            
            // Charger le thème actif
            await LoadActiveThemeAsync();
        }

        public async Task ApplyThemeAsync(ThemeInfo theme)
        {
            if (theme == null) return;

            // Appliquer le thème à l'application
            var app = Application.Current;
            if (app != null)
            {
                switch (theme.Type)
                {
                    case ThemeType.Light:
                        app.RequestedThemeVariant = ThemeVariant.Light;
                        break;
                    case ThemeType.Dark:
                        app.RequestedThemeVariant = ThemeVariant.Dark;
                        break;
                    case ThemeType.System:
                        app.RequestedThemeVariant = ThemeVariant.Default;
                        break;
                    case ThemeType.Custom:
                        // Appliquer un thème personnalisé (à implémenter)
                        break;
                }
            }

            // Sauvegarder le thème actif
            CurrentTheme = theme;
            await SaveActiveThemeAsync();
        }

        private async Task LoadCustomThemesAsync()
        {
            // Rechercher les fichiers de thèmes personnalisés
            var themeFiles = Directory.GetFiles(_themesFolderPath, "*.json")
                .Where(f => !Path.GetFileName(f).Equals("active_theme.json", StringComparison.OrdinalIgnoreCase));

            foreach (var file in themeFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var theme = JsonSerializer.Deserialize<ThemeInfo>(json);
                    if (theme != null && !AvailableThemes.Any(t => t.Id == theme.Id))
                    {
                        theme.Type = ThemeType.Custom;
                        AvailableThemes.Add(theme);
                    }
                }
                catch (Exception)
                {
                    // Ignorer les fichiers de thème invalides
                }
            }
        }

        private async Task LoadActiveThemeAsync()
        {
            if (!File.Exists(_activeThemeFilePath))
            {
                // Par défaut, utiliser le thème système
                CurrentTheme = AvailableThemes.FirstOrDefault(t => t.Id == "default_system");
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_activeThemeFilePath);
                var themeId = JsonSerializer.Deserialize<string>(json);
                
                CurrentTheme = AvailableThemes.FirstOrDefault(t => t.Id == themeId) ?? 
                               AvailableThemes.FirstOrDefault(t => t.Id == "default_system");
            }
            catch (Exception)
            {
                // En cas d'erreur, utiliser le thème système
                CurrentTheme = AvailableThemes.FirstOrDefault(t => t.Id == "default_system");
            }

            // Appliquer le thème chargé
            if (CurrentTheme != null)
            {
                await ApplyThemeAsync(CurrentTheme);
            }
        }

        private async Task SaveActiveThemeAsync()
        {
            if (CurrentTheme == null) return;

            try
            {
                var json = JsonSerializer.Serialize(CurrentTheme.Id);
                await File.WriteAllTextAsync(_activeThemeFilePath, json);
            }
            catch (Exception)
            {
                // Ignorer les erreurs de sauvegarde
            }
        }

        public async Task ImportThemeAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var theme = JsonSerializer.Deserialize<ThemeInfo>(json);
                
                if (theme != null && !string.IsNullOrEmpty(theme.Id) && !string.IsNullOrEmpty(theme.Name))
                {
                    // Vérifier si le thème existe déjà
                    var existingTheme = AvailableThemes.FirstOrDefault(t => t.Id == theme.Id);
                    if (existingTheme != null)
                    {
                        // Remplacer le thème existant
                        AvailableThemes.Remove(existingTheme);
                    }

                    // Ajouter le nouveau thème
                    theme.Type = ThemeType.Custom;
                    theme.LastUpdated = DateTime.Now;
                    AvailableThemes.Add(theme);

                    // Sauvegarder le thème
                    var themePath = Path.Combine(_themesFolderPath, $"{theme.Id}.json");
                    await File.WriteAllTextAsync(themePath, json);

                    // Appliquer le thème importé
                    await ApplyThemeAsync(theme);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'importation du thème: {ex.Message}");
            }
        }

        public async Task DownloadThemeFromGitHubAsync(string repositoryUrl)
        {
            try
            {
                // Vérifier que l'URL est valide
                if (!Uri.TryCreate(repositoryUrl, UriKind.Absolute, out var uri) || 
                    !uri.Host.Contains("github.com", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("URL GitHub invalide");
                }

                // Construire l'URL pour télécharger le ZIP du dépôt
                var zipUrl = repositoryUrl.TrimEnd('/');
                if (!zipUrl.EndsWith("/archive/main.zip") && !zipUrl.EndsWith("/archive/master.zip"))
                {
                    // Déterminer la branche par défaut (main ou master)
                    var defaultBranch = "main";
                    try
                    {
                        var repoApiUrl = repositoryUrl.Replace("github.com", "api.github.com/repos");
                        var response = await _httpClient.GetStringAsync(repoApiUrl);
                        var repoInfo = JsonSerializer.Deserialize<JsonElement>(response);
                        if (repoInfo.TryGetProperty("default_branch", out var branchElement))
                        {
                            defaultBranch = branchElement.GetString() ?? "main";
                        }
                    }
                    catch
                    {
                        // En cas d'erreur, on essaie avec "main" par défaut
                    }

                    zipUrl = $"{zipUrl}/archive/{defaultBranch}.zip";
                }

                // Télécharger le fichier ZIP
                var tempZipPath = Path.Combine(Path.GetTempPath(), $"theme_{Guid.NewGuid()}.zip");
                var response = await _httpClient.GetAsync(zipUrl);
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(tempZipPath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }

                // Extraire le ZIP dans un dossier temporaire
                var tempExtractPath = Path.Combine(Path.GetTempPath(), $"theme_{Guid.NewGuid()}");
                ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath);

                // Rechercher le fichier theme.json
                var themeJsonFiles = Directory.GetFiles(tempExtractPath, "theme.json", SearchOption.AllDirectories);
                if (themeJsonFiles.Length == 0)
                {
                    throw new Exception("Aucun fichier theme.json trouvé dans le dépôt");
                }

                // Charger le premier fichier theme.json trouvé
                var themeJsonPath = themeJsonFiles[0];
                var themeJson = await File.ReadAllTextAsync(themeJsonPath);
                var theme = JsonSerializer.Deserialize<ThemeInfo>(themeJson);

                if (theme == null)
                {
                    throw new Exception("Format de thème invalide");
                }

                // Générer un ID unique si non spécifié
                if (string.IsNullOrEmpty(theme.Id))
                {
                    theme.Id = Guid.NewGuid().ToString();
                }

                // Ajouter des informations supplémentaires
                theme.RepositoryUrl = repositoryUrl;
                theme.LastUpdated = DateTime.Now;
                theme.Type = ThemeType.Custom;

                // Créer un dossier pour le thème téléchargé
                var themeDir = Path.Combine(_downloadedThemesDirectory, theme.Id);
                if (Directory.Exists(themeDir))
                {
                    Directory.Delete(themeDir, true);
                }
                Directory.CreateDirectory(themeDir);

                // Copier les fichiers du thème
                var themeSourceDir = Path.GetDirectoryName(themeJsonPath);
                if (themeSourceDir != null)
                {
                    foreach (var file in Directory.GetFiles(themeSourceDir))
                    {
                        File.Copy(file, Path.Combine(themeDir, Path.GetFileName(file)));
                    }
                }

                // Mettre à jour le chemin du thème
                theme.Path = themeDir;

                // Sauvegarder le thème
                var themePath = Path.Combine(_themesFolderPath, $"{theme.Id}.json");
                await File.WriteAllTextAsync(themePath, JsonSerializer.Serialize(theme, new JsonSerializerOptions { WriteIndented = true }));

                // Vérifier si le thème existe déjà
                var existingTheme = AvailableThemes.FirstOrDefault(t => t.Id == theme.Id);
                if (existingTheme != null)
                {
                    // Remplacer le thème existant
                    AvailableThemes.Remove(existingTheme);
                }

                // Ajouter le thème à la liste
                AvailableThemes.Add(theme);

                // Nettoyer les fichiers temporaires
                try
                {
                    File.Delete(tempZipPath);
                    Directory.Delete(tempExtractPath, true);
                }
                catch
                {
                    // Ignorer les erreurs de nettoyage
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du téléchargement du thème: {ex.Message}");
            }
        }

        public async Task CheckForUpdatesAsync()
        {
            var themesToUpdate = AvailableThemes
                .Where(t => !string.IsNullOrEmpty(t.RepositoryUrl) && !t.IsBuiltIn)
                .ToList();

            foreach (var theme in themesToUpdate)
            {
                try
                {
                    // Vérifier si le thème a été mis à jour récemment (moins de 24h)
                    if ((DateTime.Now - theme.LastUpdated).TotalHours < 24)
                    {
                        continue;
                    }

                    // Télécharger à nouveau le thème pour le mettre à jour
                    await DownloadThemeFromGitHubAsync(theme.RepositoryUrl);
                }
                catch
                {
                    // Ignorer les erreurs de mise à jour
                }
            }
        }

        public async Task CreateThemeTemplateAsync(string outputPath)
        {
            var template = new ThemeInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Mon Thème Personnalisé",
                Description = "Description de mon thème personnalisé",
                Author = "Votre Nom",
                Type = ThemeType.Custom,
                ColorPalette = new Dictionary<string, string>
                {
                    { "Primary", "#3498db" },
                    { "Secondary", "#2ecc71" },
                    { "Background", "#ffffff" },
                    { "Text", "#333333" },
                    { "Accent", "#e74c3c" }
                },
                LastUpdated = DateTime.Now
            };

            var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(outputPath, json);
        }
    }

    public enum ThemeType
    {
        Light,
        Dark,
        System,
        Custom
    }

    public class ThemeInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Author { get; set; }
        public ThemeType Type { get; set; }
        public Dictionary<string, string>? ColorPalette { get; set; }
        public string? Path { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? RepositoryUrl { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
    }
}