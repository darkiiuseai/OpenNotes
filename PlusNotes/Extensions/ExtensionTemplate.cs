using System;
using System.Threading.Tasks;

namespace OpenNotes.Extensions
{
    /// <summary>
    /// Modèle de base pour créer une extension OpenNotes
    /// </summary>
    public class ExtensionTemplate : IExtension
    {
        public string Id => "com.example.extension";
        public string Name => "Extension Exemple";
        public string? Description => "Une extension exemple pour OpenNotes";
        public string? Version => "1.0.0";
        public string? Author => "Votre Nom";

        /// <summary>
        /// Méthode appelée lors de l'activation de l'extension
        /// </summary>
        public async Task InitializeAsync()
        {
            // Code d'initialisation de l'extension
            // Par exemple: enregistrer des commandes, des hooks, etc.
            await Task.CompletedTask;
        }

        /// <summary>
        /// Méthode appelée lors de la désactivation de l'extension
        /// </summary>
        public async Task ShutdownAsync()
        {
            // Code de nettoyage lors de la désactivation
            // Par exemple: libérer des ressources, désinscrire des événements, etc.
            await Task.CompletedTask;
        }

        // Ajoutez ici vos méthodes personnalisées
    }
}