using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenNotes.Services
{
    public partial class UpdateService : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly string _appDataPath;
        private readonly string _updateInfoPath;
        private readonly string _updatesFolderPath;
        private readonly string _pendingUpdatePath;
        private readonly string _githubRepoUrl = "https://github.com/darkiiuseai/OpenNotes";
        
        [ObservableProperty]
        private string _currentVersion = "1.0.0";
        
        [ObservableProperty]
        private string _latestVersion = "1.0.0";
        
        [ObservableProperty]
        private bool _updateAvailable = false;
        
        [ObservableProperty]
        private string _updateStatus = "";
        
        [ObservableProperty]
        private bool _isCheckingForUpdates = false;
        
        public UpdateService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "OpenNotes-App");
            
            _appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenNotes");
            _updateInfoPath = Path.Combine(_appDataPath, "update_info.json");
            _updatesFolderPath = Path.Combine(_appDataPath, "Updates");
            _pendingUpdatePath = Path.Combine(_appDataPath, "pending_update.json");
            
            // Créer les dossiers s'ils n'existent pas
            if (!Directory.Exists(_appDataPath))
            {
                Directory.CreateDirectory(_appDataPath);
            }
            
            if (!Directory.Exists(_updatesFolderPath))
            {
                Directory.CreateDirectory(_updatesFolderPath);
            }
            
            // Charger la version actuelle depuis l'assembly
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var assemblyVersion = assembly.GetName().Version;
            if (assemblyVersion != null)
            {
                CurrentVersion = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
            }
            
            // Charger les informations de mise à jour précédentes
            LoadUpdateInfo();
            
            // Vérifier s'il y a une mise à jour en attente
            CheckAndApplyPendingUpdateAsync().ConfigureAwait(false);
        }
        
        private void LoadUpdateInfo()
        {
            try
            {
                if (File.Exists(_updateInfoPath))
                {
                    var json = File.ReadAllText(_updateInfoPath);
                    var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(json);
                    if (updateInfo != null)
                    {
                        LatestVersion = updateInfo.LatestVersion;
                        UpdateAvailable = IsNewerVersion(LatestVersion, CurrentVersion);
                    }
                }
            }
            catch
            {
                // Ignorer les erreurs de chargement
            }
        }
        
        private async Task SaveUpdateInfo()
        {
            try
            {
                var updateInfo = new UpdateInfo
                {
                    LatestVersion = LatestVersion,
                    LastChecked = DateTime.Now
                };
                
                var json = JsonSerializer.Serialize(updateInfo, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_updateInfoPath, json);
            }
            catch
            {
                // Ignorer les erreurs de sauvegarde
            }
        }
        
        public async Task CheckForUpdatesAsync()
        {
            try
            {
                IsCheckingForUpdates = true;
                UpdateStatus = "Vérification des mises à jour...";
                
                // Vérifier si une vérification a été effectuée récemment (moins de 4h)
                if (File.Exists(_updateInfoPath))
                {
                    var json = File.ReadAllText(_updateInfoPath);
                    var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(json);
                    if (updateInfo != null && (DateTime.Now - updateInfo.LastChecked).TotalHours < 4)
                    {
                        // Utiliser les informations existantes
                        LatestVersion = updateInfo.LatestVersion;
                        UpdateAvailable = IsNewerVersion(LatestVersion, CurrentVersion);
                        UpdateStatus = UpdateAvailable ? $"Mise à jour disponible: {LatestVersion}" : "Aucune mise à jour disponible";
                        IsCheckingForUpdates = false;
                        return;
                    }
                }
                
                // Obtenir les informations de version depuis GitHub
                var apiUrl = _githubRepoUrl.Replace("github.com", "api.github.com/repos") + "/releases/latest";
                var response = await _httpClient.GetStringAsync(apiUrl);
                var releaseInfo = JsonSerializer.Deserialize<JsonElement>(response);
                
                if (releaseInfo.TryGetProperty("tag_name", out var tagElement))
                {
                    var tagName = tagElement.GetString();
                    if (!string.IsNullOrEmpty(tagName))
                    {
                        // Nettoyer le tag (enlever le 'v' au début si présent)
                        LatestVersion = tagName.TrimStart('v');
                        UpdateAvailable = IsNewerVersion(LatestVersion, CurrentVersion);
                        
                        // Sauvegarder les informations
                        await SaveUpdateInfo();
                        
                        UpdateStatus = UpdateAvailable ? $"Mise à jour disponible: {LatestVersion}" : "Aucune mise à jour disponible";
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus = $"Erreur lors de la vérification: {ex.Message}";
            }
            finally
            {
                IsCheckingForUpdates = false;
            }
        }
        
        public async Task DownloadAndInstallUpdateAsync()
        {
            try
            {
                if (!UpdateAvailable) return;
                
                UpdateStatus = "Téléchargement de la mise à jour...";
                
                // Obtenir l'URL de téléchargement
                var apiUrl = _githubRepoUrl.Replace("github.com", "api.github.com/repos") + "/releases/latest";
                var response = await _httpClient.GetStringAsync(apiUrl);
                var releaseInfo = JsonSerializer.Deserialize<JsonElement>(response);
                
                string? downloadUrl = null;
                string? releaseNotes = null;
                
                // Récupérer les notes de version
                if (releaseInfo.TryGetProperty("body", out var bodyElement))
                {
                    releaseNotes = bodyElement.GetString();
                }
                
                if (releaseInfo.TryGetProperty("assets", out var assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
                {
                    for (int i = 0; i < assetsElement.GetArrayLength(); i++)
                    {
                        var asset = assetsElement[i];
                        if (asset.TryGetProperty("name", out var nameElement) && 
                            nameElement.GetString()?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == true &&
                            asset.TryGetProperty("browser_download_url", out var urlElement))
                        {
                            downloadUrl = urlElement.GetString();
                            break;
                        }
                    }
                }
                
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    throw new Exception("Aucun fichier de mise à jour trouvé");
                }
                
                // Télécharger le fichier ZIP
                var updateDir = Path.Combine(_appDataPath, "Updates");
                if (!Directory.Exists(updateDir))
                {
                    Directory.CreateDirectory(updateDir);
                }
                
                var tempZipPath = Path.Combine(updateDir, $"opennotes_update_{LatestVersion}.zip");
                var downloadResponse = await _httpClient.GetAsync(downloadUrl);
                downloadResponse.EnsureSuccessStatusCode();
                
                using (var fs = new FileStream(tempZipPath, FileMode.Create))
                {
                    await downloadResponse.Content.CopyToAsync(fs);
                }
                
                UpdateStatus = "Installation de la mise à jour...";
                
                // Extraire le ZIP dans un dossier temporaire
                var extractPath = Path.Combine(updateDir, $"update_{LatestVersion}");
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                
                Directory.CreateDirectory(extractPath);
                ZipFile.ExtractToDirectory(tempZipPath, extractPath);
                
                // Créer un fichier d'installation pour la prochaine exécution de l'application
                var installInfoPath = Path.Combine(_appDataPath, "pending_update.json");
                var installInfo = new
                {
                    Version = LatestVersion,
                    SourcePath = extractPath,
                    ReleaseNotes = releaseNotes,
                    InstallDate = DateTime.Now
                };
                
                var json = JsonSerializer.Serialize(installInfo, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(installInfoPath, json);
                
                UpdateStatus = "Mise à jour téléchargée. Elle sera installée au prochain démarrage.";

                
                // Créer un script de mise à jour qui sera exécuté après la fermeture de l'application
                var updateScriptPath = Path.Combine(Path.GetTempPath(), "opennotes_update.bat");
                var appPath = Process.GetCurrentProcess().MainModule?.FileName;
                var appDir = Path.GetDirectoryName(appPath);
                
                if (string.IsNullOrEmpty(appDir))
                {
                    throw new Exception("Impossible de déterminer le répertoire de l'application");
                }
                
                // Créer le script de mise à jour
                var scriptContent = $@"@echo off
" +
                                    $"timeout /t 2 /nobreak > nul\r\n" +
                                    $"xcopy /E /Y \"{tempExtractPath}\*.*\" \"{appDir}\"\r\n" +
                                    $"start """ \"{appPath}\"\r\n" +
                                    $"del \"{tempZipPath}\"\r\n" +
                                    $"rmdir /S /Q \"{tempExtractPath}\"\r\n" +
                                    $"del \"{updateScriptPath}\"\r\n";
                
                await File.WriteAllTextAsync(updateScriptPath, scriptContent);
                
                // Lancer le script et fermer l'application
                UpdateStatus = "Redémarrage de l'application...";
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {updateScriptPath}",
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                
                // Fermer l'application
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                UpdateStatus = $"Erreur lors de la mise à jour: {ex.Message}";
            }
        }
        
        private bool IsNewerVersion(string version1, string version2)
        {
            try
            {
                var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
                var v2Parts = version2.Split('.').Select(int.Parse).ToArray();
                
                // Comparer les parties de la version
                for (int i = 0; i < Math.Min(v1Parts.Length, v2Parts.Length); i++)
                {
                    if (v1Parts[i] > v2Parts[i])
                        return true;
                    if (v1Parts[i] < v2Parts[i])
                        return false;
                }
                
                // Si toutes les parties sont égales, vérifier si v1 a plus de parties
                return v1Parts.Length > v2Parts.Length;
            }
            catch
            {
                // En cas d'erreur de parsing, considérer qu'il n'y a pas de nouvelle version
                return false;
            }
        }
    }
    
    public class UpdateInfo
    {
        public string LatestVersion { get; set; } = "";
        public DateTime LastChecked { get; set; } = DateTime.Now;
    }
}