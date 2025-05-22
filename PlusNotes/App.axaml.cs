using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using OpenNotes.Extensions;
using OpenNotes.Themes;
using OpenNotes.ViewModels;
using OpenNotes.Views;

namespace OpenNotes;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private ExtensionManager? _extensionManager;
    private ThemeManager? _themeManager;
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            // Initialiser le gestionnaire de thèmes
            _themeManager = new ThemeManager();
            Task.Run(async () => await _themeManager.InitializeAsync()).Wait();
            
            // Initialiser le gestionnaire d'extensions
            _extensionManager = new ExtensionManager();
            Task.Run(async () => await _extensionManager.InitializeAsync()).Wait();
            
            // Enregistrer les extensions intégrées
            RegisterBuiltInExtensions();
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            
            MainWindow = desktop.MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
    
    private void RegisterBuiltInExtensions()
    {
        if (_extensionManager == null) return;
        
        // Créer des instances des extensions intégrées
        var exportExtension = new ExportExtension();
        var autoOrganizerExtension = new AutoNoteOrganizerExtension();
        var templatesExtension = new TemplatesExtension();
        var syncExtension = new SyncExtension();
        
        // Ajouter les extensions à la liste des extensions disponibles
        var exportInfo = new ExtensionInfo
        {
            Id = exportExtension.Id,
            Name = exportExtension.Name,
            Description = exportExtension.Description,
            Version = exportExtension.Version,
            Author = exportExtension.Author,
            Instance = exportExtension,
            IsBuiltIn = true
        };
        
        var organizerInfo = new ExtensionInfo
        {
            Id = autoOrganizerExtension.Id,
            Name = autoOrganizerExtension.Name,
            Description = autoOrganizerExtension.Description,
            Version = autoOrganizerExtension.Version,
            Author = autoOrganizerExtension.Author,
            Instance = autoOrganizerExtension,
            IsBuiltIn = true
        };
        
        var templatesInfo = new ExtensionInfo
        {
            Id = templatesExtension.Id,
            Name = templatesExtension.Name,
            Description = templatesExtension.Description,
            Version = templatesExtension.Version,
            Author = templatesExtension.Author,
            Instance = templatesExtension,
            IsBuiltIn = true
        };
        
        var syncInfo = new ExtensionInfo
        {
            Id = syncExtension.Id,
            Name = syncExtension.Name,
            Description = syncExtension.Description,
            Version = syncExtension.Version,
            Author = syncExtension.Author,
            Instance = syncExtension,
            IsBuiltIn = true
        };
        
        // Ajouter les extensions à la liste des extensions disponibles
        _extensionManager.AvailableExtensions.Add(exportInfo);
        _extensionManager.AvailableExtensions.Add(organizerInfo);
        _extensionManager.AvailableExtensions.Add(templatesInfo);
        _extensionManager.AvailableExtensions.Add(syncInfo);
        
        // Activer les extensions par défaut
        Task.Run(async () => await _extensionManager.EnableExtensionAsync(exportInfo)).Wait();
        Task.Run(async () => await _extensionManager.EnableExtensionAsync(organizerInfo)).Wait();
        Task.Run(async () => await _extensionManager.EnableExtensionAsync(templatesInfo)).Wait();
        Task.Run(async () => await _extensionManager.EnableExtensionAsync(syncInfo)).Wait();
    }
}