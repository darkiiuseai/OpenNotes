using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenNotes.Extensions;
using OpenNotes.Services;
using OpenNotes.Themes;

namespace OpenNotes.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly ThemeManager _themeManager;
        private readonly ExtensionManager _extensionManager;
        private readonly UpdateService _updateService;

        [ObservableProperty]
        private ObservableCollection<ThemeInfo> _availableThemes = new();

        [ObservableProperty]
        private ThemeInfo? _currentTheme;

        [ObservableProperty]
        private ThemeInfo? _selectedTheme;

        [ObservableProperty]
        private ObservableCollection<ExtensionInfo> _availableExtensions = new();

        [ObservableProperty]
        private ObservableCollection<ExtensionInfo> _enabledExtensions = new();

        [ObservableProperty]
        private ExtensionInfo? _selectedExtension;

        [ObservableProperty]
        private string _statusMessage = string.Empty;
        
        // Options d'interface
        [ObservableProperty]
        private bool _darkModeEnabled = true;
        
        [ObservableProperty]
        private bool _autoSaveEnabled = true;
        
        [ObservableProperty]
        private int _autoSaveInterval = 5; // minutes

        public SettingsViewModel()
        {
            _themeManager = new ThemeManager();
            _extensionManager = new ExtensionManager();
            _updateService = new UpdateService();
            
            // Initialiser les thèmes et extensions
            InitializeAsync().ConfigureAwait(false);
        }

        private async Task InitializeAsync()
        {
            // Initialiser le gestionnaire de thèmes
            await _themeManager.InitializeAsync();
            AvailableThemes = _themeManager.AvailableThemes;
            CurrentTheme = _themeManager.CurrentTheme;

            // Initialiser le gestionnaire d'extensions
            await _extensionManager.InitializeAsync();
            AvailableExtensions = _extensionManager.AvailableExtensions;
            EnabledExtensions = _extensionManager.EnabledExtensions;
        }

        [RelayCommand]
        private async Task ApplyTheme()
        {
            if (CurrentTheme == null) return;

            await _themeManager.ApplyThemeAsync(CurrentTheme);
            StatusMessage = $"Thème {CurrentTheme.Name} appliqué";
        }

        [RelayCommand]
        private async Task ImportTheme()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Importer un thème",
                Filters = new()
                {
                    new() { Name = "Fichiers JSON", Extensions = new() { "json" } }
                }
            };

            var result = await dialog.ShowAsync(App.MainWindow);
            if (result != null && result.Length > 0)
            {
                await _themeManager.ImportThemeAsync(result[0]);
                StatusMessage = "Thème importé avec succès";
            }
        }

        [RelayCommand]
        private async Task EnableExtension(ExtensionInfo extension)
        {
            if (extension == null) return;

            await _extensionManager.EnableExtensionAsync(extension);
            EnabledExtensions = _extensionManager.EnabledExtensions;
            StatusMessage = $"Extension {extension.Name} activée";
        }

        [RelayCommand]
        private async Task DisableExtension(ExtensionInfo extension)
        {
            if (extension == null) return;

            await _extensionManager.DisableExtensionAsync(extension);
            EnabledExtensions = _extensionManager.EnabledExtensions;
            StatusMessage = $"Extension {extension.Name} désactivée";
        }

        [RelayCommand]
        private async Task InstallExtension()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Installer une extension",
                Filters = new()
                {
                    new() { Name = "Extensions (.dll)", Extensions = new() { "dll" } }
                }
            };

            var result = await dialog.ShowAsync(App.MainWindow);
            if (result != null && result.Length > 0)
            {
                await _extensionManager.InstallExtensionAsync(result[0]);
                AvailableExtensions = _extensionManager.AvailableExtensions;
                EnabledExtensions = _extensionManager.EnabledExtensions;
                StatusMessage = "Extension installée avec succès";
            }
        }

        [RelayCommand]
        private async Task UninstallExtension(ExtensionInfo extension)
        {
            if (extension == null) return;

            await _extensionManager.UninstallExtensionAsync(extension);
            AvailableExtensions = _extensionManager.AvailableExtensions;
            EnabledExtensions = _extensionManager.EnabledExtensions;
            SelectedExtension = null;
            StatusMessage = $"Extension {extension.Name} désinstallée";
        }
        
        [RelayCommand]
        private void SaveInterfaceSettings()
        {
            // Sauvegarder les paramètres d'interface
            // Dans une implémentation réelle, ces paramètres seraient sauvegardés dans un fichier de configuration
            
            StatusMessage = "Paramètres d'interface sauvegardés";
        }
        
        [RelayCommand]
        private async Task CheckForUpdates()
        {
            StatusMessage = "Recherche de mises à jour...";
            
            try
            {
                // Vérifier les mises à jour de l'application
                await _updateService.CheckForUpdatesAsync();
                
                // Vérifier les mises à jour des thèmes
                await _themeManager.CheckForUpdatesAsync();
                
                // Vérifier les mises à jour des extensions
                await _extensionManager.CheckForUpdatesAsync();
                
                if (_updateService.UpdateAvailable)
                {
                    StatusMessage = $"Mise à jour de l'application disponible: {_updateService.LatestVersion}";
                }
                else
                {
                    StatusMessage = "Vérification des mises à jour terminée";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de la vérification des mises à jour: {ex.Message}";
            }
        }
        
        [RelayCommand]
        private async Task InstallAppUpdate()
        {
            try
            {
                if (!_updateService.UpdateAvailable) return;
                
                StatusMessage = "Installation de la mise à jour...";
                await _updateService.DownloadAndInstallUpdateAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de l'installation de la mise à jour: {ex.Message}";
            }
        }
        
        [RelayCommand]
        private async Task CreateThemeTemplate()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Créer un modèle de thème",
                Filters = new()
                {
                    new() { Name = "Fichiers JSON", Extensions = new() { "json" } }
                },
                InitialFileName = "theme_template.json"
            };

            var result = await dialog.ShowAsync(App.MainWindow);
            if (!string.IsNullOrEmpty(result))
            {
                await _themeManager.CreateThemeTemplateAsync(result);
                StatusMessage = "Modèle de thème créé avec succès";
            }
        }
        
        [RelayCommand]
        private async Task CreateExtensionTemplate()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Créer un modèle d'extension",
                Filters = new()
                {
                    new() { Name = "Fichiers JSON", Extensions = new() { "json" } }
                },
                InitialFileName = "extension_template.json"
            };

            var result = await dialog.ShowAsync(App.MainWindow);
            if (!string.IsNullOrEmpty(result))
            {
                await _extensionManager.CreateExtensionTemplateAsync(result);
                StatusMessage = "Modèle d'extension créé avec succès";
            }
        }
        
        [RelayCommand]
        private async Task DownloadTheme(string repositoryUrl)
        {
            if (string.IsNullOrWhiteSpace(repositoryUrl))
            {
                StatusMessage = "URL du dépôt invalide";
                return;
            }
            
            try
            {
                StatusMessage = "Téléchargement du thème...";
                await _themeManager.DownloadThemeFromGitHubAsync(repositoryUrl);
                AvailableThemes = _themeManager.AvailableThemes;
                StatusMessage = "Thème téléchargé avec succès";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du téléchargement: {ex.Message}";
            }
        }
        
        [RelayCommand]
        private async Task DownloadExtension(string repositoryUrl)
        {
            if (string.IsNullOrWhiteSpace(repositoryUrl))
            {
                StatusMessage = "URL du dépôt invalide";
                return;
            }
            
            try
            {
                StatusMessage = "Téléchargement de l'extension...";
                await _extensionManager.DownloadExtensionFromGitHubAsync(repositoryUrl);
                AvailableExtensions = _extensionManager.AvailableExtensions;
                StatusMessage = "Extension téléchargée avec succès";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du téléchargement: {ex.Message}";
            }
        }
    }
}