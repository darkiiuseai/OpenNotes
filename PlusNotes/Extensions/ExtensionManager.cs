using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO.Compression;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PlusNotes.Extensions
{
    public partial class ExtensionManager : ObservableObject
    {
        private readonly string _extensionsFolderPath;
        private readonly string _enabledExtensionsFilePath;
        private readonly HttpClient _httpClient;
        private readonly string _downloadedExtensionsDirectory;

        [ObservableProperty]
        private ObservableCollection<ExtensionInfo> _availableExtensions = new();

        [ObservableProperty]
        private ObservableCollection<ExtensionInfo> _enabledExtensions = new();

        public ExtensionManager()
        {
            _httpClient = new HttpClient();
            _extensionsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlusNotes", "Extensions");
            _enabledExtensionsFilePath = Path.Combine(_extensionsFolderPath, "enabled_extensions.json");
            _downloadedExtensionsDirectory = Path.Combine(_extensionsFolderPath, "Downloaded");
            
            // Créer le dossier des extensions s'il n'existe pas
            if (!Directory.Exists(_extensionsFolderPath))
            {
                Directory.CreateDirectory(_extensionsFolderPath);
            }
            
            // S'assurer que le répertoire de téléchargement existe
            Directory.CreateDirectory(_downloadedExtensionsDirectory);
        }

        public async Task InitializeAsync()
        {
            // Découvrir les extensions disponibles
            await DiscoverExtensionsAsync();
            
            // Charger les extensions activées
            await LoadEnabledExtensionsAsync();
            
            // Initialiser les extensions activées
            await InitializeEnabledExtensionsAsync();
        }

        private async Task DiscoverExtensionsAsync()
        {
            // Vider la liste des extensions disponibles
            AvailableExtensions.Clear();

            // Rechercher les fichiers d'extension (.dll)
            var extensionFiles = Directory.GetFiles(_extensionsFolderPath, "*.dll");

            foreach (var file in extensionFiles)
            {
                try
                {
                    // Charger l'assembly
                    var assembly = Assembly.LoadFrom(file);
                    
                    // Rechercher les types qui implémentent IExtension
                    var extensionTypes = assembly.GetTypes()
                        .Where(t => typeof(IExtension).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var extensionType in extensionTypes)
                    {
                        // Créer une instance de l'extension
                        if (Activator.CreateInstance(extensionType) is IExtension extension)
                        {
                            var info = new ExtensionInfo
                            {
                                Id = extension.Id,
                                Name = extension.Name,
                                Description = extension.Description,
                                Version = extension.Version,
                                Author = extension.Author,
                                AssemblyPath = file,
                                Type = extensionType.FullName ?? extensionType.Name,
                                Instance = extension
                            };

                            AvailableExtensions.Add(info);
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignorer les fichiers d'extension invalides
                }
            }
        }

        private async Task LoadEnabledExtensionsAsync()
        {
            // Vider la liste des extensions activées
            EnabledExtensions.Clear();

            if (!File.Exists(_enabledExtensionsFilePath))
            {
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_enabledExtensionsFilePath);
                var enabledIds = JsonSerializer.Deserialize<List<string>>(json);
                
                if (enabledIds != null)
                {
                    foreach (var id in enabledIds)
                    {
                        var extension = AvailableExtensions.FirstOrDefault(e => e.Id == id);
                        if (extension != null)
                        {
                            EnabledExtensions.Add(extension);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, aucune extension n'est activée
            }
        }

        private async Task InitializeEnabledExtensionsAsync()
        {
            foreach (var extension in EnabledExtensions)
            {
                try
                {
                    await extension.Instance.InitializeAsync();
                    extension.IsInitialized = true;
                }
                catch (Exception)
                {
                    // Ignorer les erreurs d'initialisation
                }
            }
        }

        public async Task EnableExtensionAsync(ExtensionInfo extension)
        {
            if (extension == null || EnabledExtensions.Contains(extension))
            {
                return;
            }

            // Initialiser l'extension si ce n'est pas déjà fait
            if (!extension.IsInitialized)
            {
                try
                {
                    await extension.Instance.InitializeAsync();
                    extension.IsInitialized = true;
                }
                catch (Exception)
                {
                    // Ignorer les erreurs d'initialisation
                    return;
                }
            }

            // Ajouter l'extension à la liste des extensions activées
            EnabledExtensions.Add(extension);
            
            // Sauvegarder la liste des extensions activées
            await SaveEnabledExtensionsAsync();
        }

        public async Task DisableExtensionAsync(ExtensionInfo extension)
        {
            if (extension == null || !EnabledExtensions.Contains(extension))
            {
                return;
            }

            // Désactiver l'extension
            try
            {
                await extension.Instance.ShutdownAsync();
            }
            catch (Exception)
            {
                // Ignorer les erreurs de désactivation
            }

            // Retirer l'extension de la liste des extensions activées
            EnabledExtensions.Remove(extension);
            
            // Sauvegarder la liste des extensions activées
            await SaveEnabledExtensionsAsync();
        }

        private async Task SaveEnabledExtensionsAsync()
        {
            try
            {
                var enabledIds = EnabledExtensions.Select(e => e.Id).ToList();
                var json = JsonSerializer.Serialize(enabledIds);
                await File.WriteAllTextAsync(_enabledExtensionsFilePath, json);
            }
            catch (Exception)
            {
                // Ignorer les erreurs de sauvegarde
            }
        }

        public async Task InstallExtensionAsync(string filePath)
        {
            try
            {
                // Vérifier si c'est un fichier DLL ou un package d'extension
                if (Path.GetExtension(filePath).ToLower() == ".dll")
                {
                    if (!File.Exists(filePath))
                    {
                        return;
                    }
                    
                    // Créer un répertoire pour l'extension
                    var extensionId = Guid.NewGuid().ToString();
                    var extensionDir = Path.Combine(_downloadedExtensionsDirectory, extensionId);
                    Directory.CreateDirectory(extensionDir);
                    
                    // Copier le fichier DLL
                    var destPath = Path.Combine(extensionDir, Path.GetFileName(filePath));
                    File.Copy(filePath, destPath, true);
                    
                    // Essayer de charger l'assembly pour obtenir des informations
                    var extensionInfo = new ExtensionInfo
                    {
                        Id = extensionId,
                        Name = Path.GetFileNameWithoutExtension(filePath),
                        Description = $"Extension installée: {Path.GetFileNameWithoutExtension(filePath)}",
                        Path = extensionDir,
                        IsBuiltIn = false,
                        AssemblyPath = destPath
                    };
                    
                    try
                    {
                        var assembly = Assembly.LoadFrom(destPath);
                        var assemblyName = assembly.GetName();
                        
                        // Mettre à jour les informations si disponibles
                        extensionInfo.Name = assemblyName.Name ?? extensionInfo.Name;
                        extensionInfo.Version = assemblyName.Version?.ToString() ?? "1.0.0";
                        
                        // Rechercher des attributs personnalisés pour plus d'informations
                        var customAttributes = assembly.GetCustomAttributes();
                        foreach (var attr in customAttributes)
                        {
                            if (attr is AssemblyDescriptionAttribute descAttr)
                            {
                                extensionInfo.Description = descAttr.Description;
                            }
                            else if (attr is AssemblyCompanyAttribute companyAttr)
                            {
                                extensionInfo.Author = companyAttr.Company;
                            }
                        }
                    }
                    catch
                    {
                        // Ignorer les erreurs de chargement
                    }
                    
                    // Sauvegarder les informations de l'extension
                    var extensionJson = JsonSerializer.Serialize(extensionInfo, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(Path.Combine(extensionDir, "extension.json"), extensionJson);
                    
                    // Ajouter l'extension à la liste
                    AvailableExtensions.Add(extensionInfo);
                    
                    // Redécouvrir les extensions
                    await DiscoverExtensionsAsync();
                    
                    // Recharger les extensions activées
                    await LoadEnabledExtensionsAsync();
                }
                else if (Path.GetExtension(filePath).ToLower() == ".zip")
                {
                    // Extraire le package d'extension
                    var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempDir);
                    
                    ZipFile.ExtractToDirectory(filePath, tempDir);
                    
                    // Rechercher le fichier extension.json
                    var extensionJsonPath = Directory.GetFiles(tempDir, "extension.json", SearchOption.AllDirectories).FirstOrDefault();
                    if (extensionJsonPath == null)
                        throw new Exception("Package d'extension invalide: extension.json introuvable");
                    
                    // Charger les informations de l'extension
                    var extensionJson = await File.ReadAllTextAsync(extensionJsonPath);
                    var extensionInfo = JsonSerializer.Deserialize<ExtensionInfo>(extensionJson);
                    
                    if (extensionInfo == null)
                        throw new Exception("Format d'extension invalide");
                    
                    // Générer un ID unique si non spécifié
                    if (string.IsNullOrEmpty(extensionInfo.Id))
                        extensionInfo.Id = Guid.NewGuid().ToString();
                    
                    // Créer le répertoire pour l'extension
                    var extensionDir = Path.Combine(_downloadedExtensionsDirectory, extensionInfo.Id);
                    if (Directory.Exists(extensionDir))
                        Directory.Delete(extensionDir, true);
                    
                    Directory.CreateDirectory(extensionDir);
                    
                    // Copier les fichiers de l'extension
                    var extensionSourceDir = Path.GetDirectoryName(extensionJsonPath);
                    foreach (var file in Directory.GetFiles(extensionSourceDir))
                    {
                        File.Copy(file, Path.Combine(extensionDir, Path.GetFileName(file)));
                    }
                    
                    // Mettre à jour les informations de l'extension
                    extensionInfo.Path = extensionDir;
                    extensionInfo.IsBuiltIn = false;
                    
                    // Sauvegarder les informations mises à jour
                    await File.WriteAllTextAsync(
                        Path.Combine(extensionDir, "extension.json"), 
                        JsonSerializer.Serialize(extensionInfo, new JsonSerializerOptions { WriteIndented = true }));
                    
                    // Ajouter l'extension à la liste
                    AvailableExtensions.Add(extensionInfo);
                    
                    // Redécouvrir les extensions
                    await DiscoverExtensionsAsync();
                    
                    // Recharger les extensions activées
                    await LoadEnabledExtensionsAsync();
                    
                    // Nettoyer
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch
                    {
                        // Ignorer les erreurs de nettoyage
                    }
                }
                else
                {
                    throw new Exception("Format de fichier non pris en charge. Utilisez .dll ou .zip");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'installation de l'extension: {ex.Message}");
            }
        }

        public async Task UninstallExtensionAsync(ExtensionInfo extension)
        {
            if (extension == null)
            {
                return;
            }

            try
            {
                // Désactiver l'extension si elle est activée
                if (EnabledExtensions.Contains(extension))
                {
                    await DisableExtensionAsync(extension);
                }

                // Supprimer le fichier d'extension
                if (File.Exists(extension.AssemblyPath))
                {
                    File.Delete(extension.AssemblyPath);
                }

                // Retirer l'extension de la liste des extensions disponibles
                AvailableExtensions.Remove(extension);
            }
            catch (Exception)
            {
                // Gérer l'erreur de désinstallation
            }
        }
    }

    public class ExtensionInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Version { get; set; }
        public string? Author { get; set; }
        public string AssemblyPath { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public IExtension Instance { get; set; } = null!;
        public bool IsInitialized { get; set; }
        public string Path { get; set; } = string.Empty;
        public bool IsBuiltIn { get; set; }
        public string? RepositoryUrl { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public interface IExtension
    {
        string Id { get; }
        string Name { get; }
        string? Description { get; }
        string? Version { get; }
        string? Author { get; }
        
        Task InitializeAsync();
        Task ShutdownAsync();
    }
}