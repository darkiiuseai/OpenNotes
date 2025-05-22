using System;

namespace OpenNotes.Extensions
{
    /// <summary>
    /// Classe contenant les informations sur les mises à jour disponibles pour une extension
    /// </summary>
    public class ExtensionUpdateInfo
    {
        /// <summary>
        /// L'extension concernée par la mise à jour
        /// </summary>
        public ExtensionInfo Extension { get; set; } = null!;
        
        /// <summary>
        /// Version actuelle de l'extension
        /// </summary>
        public string CurrentVersion { get; set; } = "0.0.0";
        
        /// <summary>
        /// Version disponible sur le dépôt distant
        /// </summary>
        public string RemoteVersion { get; set; } = "0.0.0";
        
        /// <summary>
        /// Indique si une mise à jour est disponible
        /// </summary>
        public bool UpdateAvailable { get; set; }
        
        /// <summary>
        /// Indique si la mise à jour a été installée
        /// </summary>
        public bool Updated { get; set; }
        
        /// <summary>
        /// Notes de version pour la mise à jour
        /// </summary>
        public string? ReleaseNotes { get; set; }
        
        /// <summary>
        /// Message d'erreur en cas d'échec de la vérification
        /// </summary>
        public string? Error { get; set; }
    }
}