using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenNotes.Extensions
{
    /// <summary>
    /// Extension permettant d'organiser automatiquement les notes
    /// </summary>
    public class AutoNoteOrganizerExtension : IExtension
    {
        public string Id => "com.opennotes.extension.auto_organizer";
        public string Name => "Organisateur Automatique";
        public string? Description => "Organise et catégorise automatiquement vos notes en fonction de leur contenu";
        public string? Version => "1.0.0";
        public string? Author => "OpenNotes Team";

        private readonly Dictionary<string, List<string>> _categoryKeywords = new Dictionary<string, List<string>>();

        /// <summary>
        /// Méthode appelée lors de l'activation de l'extension
        /// </summary>
        public async Task InitializeAsync()
        {
            // Initialiser les catégories et mots-clés par défaut
            InitializeDefaultCategories();
            Console.WriteLine($"Extension {Name} initialisée avec {_categoryKeywords.Count} catégories");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Méthode appelée lors de la désactivation de l'extension
        /// </summary>
        public async Task ShutdownAsync()
        {
            // Nettoyage des ressources
            Console.WriteLine($"Extension {Name} désactivée");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Initialise les catégories par défaut avec leurs mots-clés associés
        /// </summary>
        private void InitializeDefaultCategories()
        {
            _categoryKeywords.Clear();

            // Catégorie Travail
            _categoryKeywords.Add("Travail", new List<string>
            {
                "projet", "réunion", "deadline", "client", "tâche", "objectif", "planning",
                "rapport", "présentation", "collègue", "bureau", "entreprise", "professionnel"
            });

            // Catégorie Personnel
            _categoryKeywords.Add("Personnel", new List<string>
            {
                "famille", "ami", "loisir", "vacances", "maison", "santé", "sport",
                "hobby", "weekend", "personnel", "privé", "vie", "relation"
            });

            // Catégorie Études
            _categoryKeywords.Add("Études", new List<string>
            {
                "cours", "examen", "devoir", "étude", "université", "école", "formation",
                "professeur", "enseignant", "étudiant", "apprentissage", "recherche", "diplôme"
            });

            // Catégorie Idées
            _categoryKeywords.Add("Idées", new List<string>
            {
                "idée", "concept", "créativité", "innovation", "inspiration", "brainstorming", "réflexion",
                "pensée", "suggestion", "proposition", "invention", "création", "développement"
            });

            // Catégorie Achats
            _categoryKeywords.Add("Achats", new List<string>
            {
                "achat", "shopping", "magasin", "prix", "produit", "article", "boutique",
                "commande", "livraison", "paiement", "facture", "remise", "promotion"
            });
        }

        /// <summary>
        /// Analyse le contenu d'une note et suggère des catégories appropriées
        /// </summary>
        /// <param name="noteContent">Contenu de la note à analyser</param>
        /// <returns>Liste des catégories suggérées avec leur score de pertinence</returns>
        public Dictionary<string, double> AnalyzeNoteContent(string noteContent)
        {
            if (string.IsNullOrEmpty(noteContent))
                return new Dictionary<string, double>();

            var result = new Dictionary<string, double>();
            var normalizedContent = noteContent.ToLower();

            foreach (var category in _categoryKeywords)
            {
                double score = 0;
                foreach (var keyword in category.Value)
                {
                    var regex = new Regex($"\\b{Regex.Escape(keyword)}\\b", RegexOptions.IgnoreCase);
                    var matches = regex.Matches(normalizedContent);
                    score += matches.Count;
                }

                // Normaliser le score en fonction du nombre de mots-clés dans la catégorie
                if (score > 0)
                {
                    score = score / category.Value.Count;
                    result.Add(category.Key, Math.Round(score, 2));
                }
            }

            return result.OrderByDescending(x => x.Value)
                         .ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Suggère un titre pour la note en fonction de son contenu
        /// </summary>
        /// <param name="noteContent">Contenu de la note</param>
        /// <returns>Titre suggéré</returns>
        public string SuggestNoteTitle(string noteContent)
        {
            if (string.IsNullOrEmpty(noteContent))
                return "Nouvelle Note";

            // Extraire la première ligne non vide comme titre potentiel
            var lines = noteContent.Split('\n')
                                  .Select(l => l.Trim())
                                  .Where(l => !string.IsNullOrEmpty(l))
                                  .ToList();

            if (lines.Count > 0)
            {
                var firstLine = lines[0];
                // Limiter la longueur du titre
                if (firstLine.Length > 50)
                    return firstLine.Substring(0, 47) + "...";
                return firstLine;
            }

            return "Nouvelle Note";
        }

        /// <summary>
        /// Ajoute une nouvelle catégorie avec ses mots-clés associés
        /// </summary>
        /// <param name="categoryName">Nom de la catégorie</param>
        /// <param name="keywords">Liste des mots-clés</param>
        public void AddCategory(string categoryName, List<string> keywords)
        {
            if (string.IsNullOrEmpty(categoryName) || keywords == null || keywords.Count == 0)
                throw new ArgumentException("Le nom de la catégorie et les mots-clés sont requis");

            if (_categoryKeywords.ContainsKey(categoryName))
                _categoryKeywords[categoryName] = keywords;
            else
                _categoryKeywords.Add(categoryName, keywords);
        }

        /// <summary>
        /// Supprime une catégorie
        /// </summary>
        /// <param name="categoryName">Nom de la catégorie à supprimer</param>
        /// <returns>True si la suppression a réussi, sinon False</returns>
        public bool RemoveCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return false;

            return _categoryKeywords.Remove(categoryName);
        }

        /// <summary>
        /// Retourne la liste des catégories disponibles
        /// </summary>
        public IReadOnlyList<string> GetCategories()
        {
            return _categoryKeywords.Keys.ToList().AsReadOnly();
        }
    }
}