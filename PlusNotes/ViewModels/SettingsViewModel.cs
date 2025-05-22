using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlusNotes.Extensions;
using PlusNotes.Services;
using PlusNotes.Themes;

namespace PlusNotes.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly ThemeManager _themeManager;
        private readonly ExtensionManager _extensionManager;

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
            // Simuler une vérification des mises à jour
            StatusMessage = "Recherche de mises à jour...";
            
            // Simuler un délai de recherche
            await Task.Delay(1500);
            
            // Vérifier les mises à jour des thèmes
            await _themeManager.CheckForUpdatesAsync();
            
            // Vérifier les mises à jour des extensions
            await _extensionManager.CheckForUpdatesAsync();
            
            StatusMessage = "Vérification des mises à jour terminée";
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