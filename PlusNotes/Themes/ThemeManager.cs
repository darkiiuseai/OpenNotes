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

namespace PlusNotes.Themes
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
            _themesFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlusNotes", "Themes");
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
                    AvailableThemes.Add(theme);

                    // Sauvegarder le thème
                    var themePath = Path.Combine(_themesFolderPath, $"{theme.Id}.json");
                    await File.WriteAllTextAsync(themePath, json);

                    // Appliquer le thème importé
                    await ApplyThemeAsync(theme);
                }
            }
            catch (Exception)
            {
                // Gérer l'erreur d'importation
            }
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