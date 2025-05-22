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
using System.Text.RegularExpressions;

namespace OpenNotes.Extensions
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
            _extensionsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenNotes", "Extensions");
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

                // Supprimer le dossier de l'extension si nécessaire
                if (!string.IsNullOrEmpty(extension.Path) && Directory.Exists(extension.Path))
                {
                    Directory.Delete(extension.Path, true);
                }

                // Retirer l'extension de la liste des extensions disponibles
                AvailableExtensions.Remove(extension);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la désinstallation de l'extension: {ex.Message}");
            }
        }

        public async Task DownloadExtensionFromGitHubAsync(string repositoryUrl)
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
                var tempZipPath = Path.Combine(Path.GetTempPath(), $"extension_{Guid.NewGuid()}.zip");
                var response = await _httpClient.GetAsync(zipUrl);
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(tempZipPath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }

                // Extraire le ZIP dans un dossier temporaire
                var tempExtractPath = Path.Combine(Path.GetTempPath(), $"extension_{Guid.NewGuid()}");
                ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath);

                // Rechercher le fichier extension.json ou extension.dll
                var extensionJsonFiles = Directory.GetFiles(tempExtractPath, "extension.json", SearchOption.AllDirectories);
                var extensionDllFiles = Directory.GetFiles(tempExtractPath, "*.dll", SearchOption.AllDirectories);

                if (extensionJsonFiles.Length == 0 && extensionDllFiles.Length == 0)
                {
                    throw new Exception("Aucun fichier d'extension trouvé dans le dépôt");
                }

                ExtensionInfo? extension = null;

                // Priorité au fichier extension.json
                if (extensionJsonFiles.Length > 0)
                {
                    var extensionJsonPath = extensionJsonFiles[0];
                    var extensionJson = await File.ReadAllTextAsync(extensionJsonPath);
                    extension = JsonSerializer.Deserialize<ExtensionInfo>(extensionJson);

                    if (extension == null)
                    {
                        throw new Exception("Format d'extension invalide");
                    }

                    // Générer un ID unique si non spécifié
                    if (string.IsNullOrEmpty(extension.Id))
                    {
                        extension.Id = Guid.NewGuid().ToString();
                    }

                    // Rechercher le fichier DLL associé
                    var extensionDir = Path.GetDirectoryName(extensionJsonPath);
                    if (extensionDir != null)
                    {
                        var dllFiles = Directory.GetFiles(extensionDir, "*.dll");
                        if (dllFiles.Length > 0)
                        {
                            extension.AssemblyPath = dllFiles[0];
                        }
                    }
                }
                // Sinon, utiliser le premier fichier DLL trouvé
                else if (extensionDllFiles.Length > 0)
                {
                    var dllPath = extensionDllFiles[0];
                    extension = new ExtensionInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = Path.GetFileNameWithoutExtension(dllPath),
                        AssemblyPath = dllPath,
                        Description = $"Extension téléchargée depuis {repositoryUrl}"
                    };

                    // Essayer de charger l'assembly pour obtenir plus d'informations
                    try
                    {
                        var assembly = Assembly.LoadFrom(dllPath);
                        var assemblyName = assembly.GetName();
                        extension.Name = assemblyName.Name ?? extension.Name;
                        extension.Version = assemblyName.Version?.ToString() ?? "1.0.0";

                        // Rechercher des attributs personnalisés
                        var customAttributes = assembly.GetCustomAttributes();
                        foreach (var attr in customAttributes)
                        {
                            if (attr is AssemblyDescriptionAttribute descAttr)
                            {
                                extension.Description = descAttr.Description;
                            }
                            else if (attr is AssemblyCompanyAttribute companyAttr)
                            {
                                extension.Author = companyAttr.Company;
                            }
                        }
                    }
                    catch
                    {
                        // Ignorer les erreurs de chargement
                    }
                }

                if (extension == null)
                {
                    throw new Exception("Impossible de créer l'extension");
                }

                // Ajouter des informations supplémentaires
                extension.RepositoryUrl = repositoryUrl;
                extension.LastUpdated = DateTime.Now;

                // Créer un dossier pour l'extension téléchargée
                var extensionDir = Path.Combine(_downloadedExtensionsDirectory, extension.Id);
                if (Directory.Exists(extensionDir))
                {
                    Directory.Delete(extensionDir, true);
                }
                Directory.CreateDirectory(extensionDir);

                // Copier les fichiers de l'extension
                var sourceDir = Path.GetDirectoryName(extension.AssemblyPath);
                if (sourceDir != null)
                {
                    foreach (var file in Directory.GetFiles(sourceDir))
                    {
                        File.Copy(file, Path.Combine(extensionDir, Path.GetFileName(file)));
                    }
                }

                // Mettre à jour les chemins
                extension.Path = extensionDir;
                extension.AssemblyPath = Path.Combine(extensionDir, Path.GetFileName(extension.AssemblyPath));

                // Sauvegarder les informations de l'extension
                var extensionInfoPath = Path.Combine(extensionDir, "extension.json");
                await File.WriteAllTextAsync(extensionInfoPath, JsonSerializer.Serialize(extension, new JsonSerializerOptions { WriteIndented = true }));

                // Vérifier si l'extension existe déjà
                var existingExtension = AvailableExtensions.FirstOrDefault(e => e.Id == extension.Id);
                if (existingExtension != null)
                {
                    // Désactiver l'ancienne extension si nécessaire
                    if (EnabledExtensions.Contains(existingExtension))
                    {
                        await DisableExtensionAsync(existingExtension);
                    }

                    // Remplacer l'extension existante
                    AvailableExtensions.Remove(existingExtension);
                }

                // Redécouvrir les extensions pour charger la nouvelle
                await DiscoverExtensionsAsync();

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
                throw new Exception($"Erreur lors du téléchargement de l'extension: {ex.Message}");
            }
        }

        public async Task<List<ExtensionUpdateInfo>> CheckForUpdatesAsync(bool autoUpdate = false)
        {
            var extensionsToUpdate = AvailableExtensions
                .Where(e => !string.IsNullOrEmpty(e.RepositoryUrl) && !e.IsBuiltIn)
                .ToList();
            
            var updateResults = new List<ExtensionUpdateInfo>();

            foreach (var extension in extensionsToUpdate)
            {
                try
                {
                    // Vérifier si l'extension a été mise à jour récemment (moins de 24h)
                    if ((DateTime.Now - extension.LastUpdated).TotalHours < 24)
                    {
                        continue;
                    }

                    // Vérifier si une mise à jour est disponible
                    var updateInfo = await CheckExtensionVersionAsync(extension);
                    
                    if (updateInfo.UpdateAvailable)
                    {
                        updateResults.Add(updateInfo);
                        
                        // Mettre à jour automatiquement si demandé
                        if (autoUpdate)
                        {
                            await DownloadExtensionFromGitHubAsync(extension.RepositoryUrl);
                            updateInfo.Updated = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Ajouter l'erreur au résultat
                    updateResults.Add(new ExtensionUpdateInfo
                    {
                        Extension = extension,
                        Error = ex.Message
                    });
                }
            }
            
            return updateResults;
        }
        
        private async Task<ExtensionUpdateInfo> CheckExtensionVersionAsync(ExtensionInfo extension)
        {
            var result = new ExtensionUpdateInfo
            {
                Extension = extension,
                CurrentVersion = extension.Version ?? "0.0.0"
            };
            
            try
            {
                if (string.IsNullOrEmpty(extension.RepositoryUrl))
                {
                    return result;
                }
                
                // Construire l'URL de l'API GitHub pour obtenir les informations du dépôt
                var repoApiUrl = extension.RepositoryUrl.Replace("github.com", "api.github.com/repos");
                var response = await _httpClient.GetStringAsync(repoApiUrl);
                var repoInfo = JsonSerializer.Deserialize<JsonElement>(response);
                
                // Obtenir la branche par défaut
                var defaultBranch = "main";
                if (repoInfo.TryGetProperty("default_branch", out var branchElement))
                {
                    defaultBranch = branchElement.GetString() ?? "main";
                }
                
                // Construire l'URL pour obtenir le contenu du fichier extension.json
                var contentApiUrl = $"{repoApiUrl}/contents/extension.json?ref={defaultBranch}";
                
                try
                {
                    var contentResponse = await _httpClient.GetStringAsync(contentApiUrl);
                    var contentInfo = JsonSerializer.Deserialize<JsonElement>(contentResponse);
                    
                    if (contentInfo.TryGetProperty("content", out var contentElement))
                    {
                        // Décoder le contenu Base64
                        var base64Content = contentElement.GetString() ?? "";
                        base64Content = base64Content.Replace("\n", "");
                        var jsonBytes = Convert.FromBase64String(base64Content);
                        var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                        
                        // Désérialiser les informations de l'extension
                        var remoteExtensionInfo = JsonSerializer.Deserialize<ExtensionInfo>(json);
                        
                        if (remoteExtensionInfo != null && !string.IsNullOrEmpty(remoteExtensionInfo.Version))
                        {
                            result.RemoteVersion = remoteExtensionInfo.Version;
                            
                            // Comparer les versions
                            if (CompareVersions(result.CurrentVersion, result.RemoteVersion) < 0)
                            {
                                result.UpdateAvailable = true;
                                result.ReleaseNotes = remoteExtensionInfo.Description;
                            }
                        }
                    }
                }
                catch
                {
                    // Si nous ne pouvons pas obtenir le fichier extension.json, essayons de vérifier les releases
                    var releasesApiUrl = $"{repoApiUrl}/releases/latest";
                    
                    try
                    {
                        var releasesResponse = await _httpClient.GetStringAsync(releasesApiUrl);
                        var releaseInfo = JsonSerializer.Deserialize<JsonElement>(releasesResponse);
                        
                        if (releaseInfo.TryGetProperty("tag_name", out var tagElement))
                        {
                            var tagName = tagElement.GetString() ?? "";
                            // Nettoyer le tag (enlever 'v' au début si présent)
                            if (tagName.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                            {
                                tagName = tagName.Substring(1);
                            }
                            
                            result.RemoteVersion = tagName;
                            
                            // Comparer les versions
                            if (CompareVersions(result.CurrentVersion, result.RemoteVersion) < 0)
                            {
                                result.UpdateAvailable = true;
                                
                                // Obtenir les notes de version
                                if (releaseInfo.TryGetProperty("body", out var bodyElement))
                                {
                                    result.ReleaseNotes = bodyElement.GetString();
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignorer les erreurs
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }
            
            return result;
        }
        
        private int CompareVersions(string version1, string version2)
        {
            // Nettoyer les versions
            version1 = version1?.Trim() ?? "0.0.0";
            version2 = version2?.Trim() ?? "0.0.0";
            
            // Extraire les numéros de version avec regex
            var regex = new Regex(@"\d+(\.\d+)*");
            var match1 = regex.Match(version1);
            var match2 = regex.Match(version2);
            
            if (match1.Success) version1 = match1.Value;
            if (match2.Success) version2 = match2.Value;
            
            // Diviser en segments
            var segments1 = version1.Split('.').Select(int.Parse).ToArray();
            var segments2 = version2.Split('.').Select(int.Parse).ToArray();
            
            // Comparer segment par segment
            var minLength = Math.Min(segments1.Length, segments2.Length);
            
            for (int i = 0; i < minLength; i++)
            {
                if (segments1[i] < segments2[i]) return -1;
                if (segments1[i] > segments2[i]) return 1;
            }
            
            // Si tous les segments comparés sont égaux, la version avec plus de segments est considérée comme plus récente
            return segments1.Length.CompareTo(segments2.Length);
        }

        public async Task CreateExtensionTemplateAsync(string outputPath)
        {
            var template = new ExtensionInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Mon Extension Personnalisée",
                Description = "Description de mon extension personnalisée",
                Author = "Votre Nom",
                Version = "1.0.0",
                LastUpdated = DateTime.Now
            };

            var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(outputPath, json);
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