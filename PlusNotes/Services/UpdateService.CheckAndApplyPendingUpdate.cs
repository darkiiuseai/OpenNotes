using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

namespace OpenNotes.Services
{
    // Classe partielle pour la méthode de vérification et d'application des mises à jour en attente
    public partial class UpdateService
    {
        /// <summary>
        /// Vérifie et applique une mise à jour en attente au démarrage de l'application
        /// </summary>
        public async Task CheckAndApplyPendingUpdateAsync()
        {
            try
            {
                if (!File.Exists(_pendingUpdatePath))
                {
                    return;
                }
                
                UpdateStatus = "Application de la mise à jour...";
                
                // Charger les informations d'installation
                var json = await File.ReadAllTextAsync(_pendingUpdatePath);
                var installInfo = JsonSerializer.Deserialize<JsonElement>(json);
                
                var sourcePath = installInfo.GetProperty("SourcePath").GetString();
                var version = installInfo.GetProperty("Version").GetString();
                
                if (string.IsNullOrEmpty(sourcePath) || !Directory.Exists(sourcePath))
                {
                    // Supprimer le fichier d'installation si le chemin source n'existe pas
                    File.Delete(_pendingUpdatePath);
                    UpdateStatus = "Échec de l'application de la mise à jour: dossier source introuvable";
                    return;
                }
                
                // Obtenir le chemin de l'application actuelle
                var appPath = AppDomain.CurrentDomain.BaseDirectory;
                
                // Copier les fichiers de la mise à jour vers le dossier de l'application
                foreach (var file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var relativePath = file.Substring(sourcePath.Length).TrimStart('\\', '/');
                        var targetPath = Path.Combine(appPath, relativePath);
                        
                        // Créer le dossier cible si nécessaire
                        var targetDir = Path.GetDirectoryName(targetPath);
                        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }
                        
                        // Copier le fichier
                        File.Copy(file, targetPath, true);
                    }
                    catch
                    {
                        // Ignorer les erreurs de copie individuelles
                    }
                }
                
                // Supprimer le fichier d'installation et les fichiers temporaires
                File.Delete(_pendingUpdatePath);
                
                try
                {
                    Directory.Delete(sourcePath, true);
                }
                catch
                {
                    // Ignorer les erreurs de suppression
                }
                
                // Mettre à jour la version actuelle
                if (!string.IsNullOrEmpty(version))
                {
                    CurrentVersion = version;
                    LatestVersion = version;
                    UpdateAvailable = false;
                }
                
                UpdateStatus = $"Mise à jour vers la version {version} appliquée avec succès";
            }
            catch (Exception ex)
            {
                // En cas d'erreur, supprimer le fichier d'installation pour éviter des tentatives répétées
                if (File.Exists(_pendingUpdatePath))
                {
                    try
                    {
                        File.Delete(_pendingUpdatePath);
                    }
                    catch
                    {
                        // Ignorer les erreurs de suppression
                    }
                }
                
                UpdateStatus = $"Erreur lors de l'application de la mise à jour: {ex.Message}";
            }
        }
    }
}