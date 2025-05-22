using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenNotes.Extensions
{
    /// <summary>
    /// Extension fournissant des modèles de notes prédéfinis
    /// </summary>
    public class TemplatesExtension : IExtension
    {
        public string Id => "com.opennotes.extension.templates";
        public string Name => "Modèles de Notes";
        public string? Description => "Fournit des modèles prédéfinis pour différents types de notes";
        public string? Version => "1.0.0";
        public string? Author => "OpenNotes Team";

        private readonly string _templatesDirectory;
        private readonly List<NoteTemplate> _builtInTemplates = new List<NoteTemplate>();
        private List<NoteTemplate> _customTemplates = new List<NoteTemplate>();

        public TemplatesExtension()
        {
            _templatesDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OpenNotes", "Templates");

            // Initialiser les modèles intégrés
            InitializeBuiltInTemplates();
        }

        /// <summary>
        /// Méthode appelée lors de l'activation de l'extension
        /// </summary>
        public async Task InitializeAsync()
        {
            // Créer le répertoire des modèles s'il n'existe pas
            if (!Directory.Exists(_templatesDirectory))
            {
                Directory.CreateDirectory(_templatesDirectory);
            }

            // Charger les modèles personnalisés
            await LoadCustomTemplatesAsync();

            Console.WriteLine($"Extension {Name} initialisée avec {_builtInTemplates.Count + _customTemplates.Count} modèles");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Méthode appelée lors de la désactivation de l'extension
        /// </summary>
        public async Task ShutdownAsync()
        {
            // Sauvegarder les modèles personnalisés
            await SaveCustomTemplatesAsync();

            Console.WriteLine($"Extension {Name} désactivée");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Initialise les modèles de notes intégrés
        /// </summary>
        private void InitializeBuiltInTemplates()
        {
            // Modèle de réunion
            _builtInTemplates.Add(new NoteTemplate
            {
                Id = "meeting",
                Name = "Compte-rendu de réunion",
                Description = "Modèle pour les comptes-rendus de réunion",
                Category = "Professionnel",
                IsBuiltIn = true,
                Content = "# Compte-rendu de réunion\n\n**Date :** {{date}}\n**Heure :** {{time}}\n**Lieu :** \n**Participants :** \n\n## Ordre du jour\n\n1. \n2. \n3. \n\n## Points discutés\n\n### 1. \n\n### 2. \n\n### 3. \n\n## Actions à suivre\n\n- [ ] Action 1 - Responsable: , Échéance: \n- [ ] Action 2 - Responsable: , Échéance: \n\n## Prochaine réunion\n\n**Date :** \n**Heure :** \n**Lieu :** "
            });

            // Modèle de projet
            _builtInTemplates.Add(new NoteTemplate
            {
                Id = "project",
                Name = "Plan de projet",
                Description = "Modèle pour la planification de projet",
                Category = "Professionnel",
                IsBuiltIn = true,
                Content = "# Plan de projet : {{projectName}}\n\n## Aperçu du projet\n\nDescription brève du projet.\n\n## Objectifs\n\n- \n- \n- \n\n## Livrables\n\n- \n- \n\n## Calendrier\n\n| Étape | Date de début | Date de fin | Responsable |
|-------|--------------|------------|-------------|
|       |              |            |             |
|       |              |            |             |
\n## Ressources\n\n- \n- \n\n## Risques et atténuations\n\n| Risque | Impact | Probabilité | Stratégie d'atténuation |
|--------|--------|------------|-------------------------|
|        |        |            |                         |
|        |        |            |                         |"
            });

            // Modèle de journal
            _builtInTemplates.Add(new NoteTemplate
            {
                Id = "journal",
                Name = "Journal quotidien",
                Description = "Modèle pour tenir un journal quotidien",
                Category = "Personnel",
                IsBuiltIn = true,
                Content = "# Journal - {{date}}\n\n## Comment je me sens aujourd'hui\n\n\n\n## Trois choses pour lesquelles je suis reconnaissant\n\n1. \n2. \n3. \n\n## Ce que j'ai accompli aujourd'hui\n\n- \n- \n- \n\n## Ce que j'ai appris aujourd'hui\n\n\n\n## Objectifs pour demain\n\n- \n- \n- "
            });

            // Modèle de recette
            _builtInTemplates.Add(new NoteTemplate
            {
                Id = "recipe",
                Name = "Recette de cuisine",
                Description = "Modèle pour enregistrer des recettes de cuisine",
                Category = "Personnel",
                IsBuiltIn = true,
                Content = "# Recette : {{recipeName}}\n\n## Ingrédients\n\n- \n- \n- \n\n## Instructions\n\n1. \n2. \n3. \n\n## Notes\n\n\n\n## Source\n\n"
            });

            // Modèle de prise de notes pour cours
            _builtInTemplates.Add(new NoteTemplate
            {
                Id = "lecture",
                Name = "Notes de cours",
                Description = "Modèle pour la prise de notes pendant un cours",
                Category = "Éducation",
                IsBuiltIn = true,
                Content = "# Notes de cours : {{subject}}\n\n**Date :** {{date}}\n**Professeur :** \n**Cours :** \n\n## Points clés\n\n- \n- \n- \n\n## Résumé\n\n\n\n## Questions à poser\n\n- \n- \n\n## Devoirs et échéances\n\n- [ ] \n- [ ] "
            });
        }

        /// <summary>
        /// Charge les modèles personnalisés depuis le disque
        /// </summary>
        private async Task LoadCustomTemplatesAsync()
        {
            _customTemplates.Clear();

            var templateFiles = Directory.GetFiles(_templatesDirectory, "*.json");
            foreach (var file in templateFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var template = JsonSerializer.Deserialize<NoteTemplate>(json);
                    if (template != null)
                    {
                        _customTemplates.Add(template);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du chargement du modèle {file}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Sauvegarde les modèles personnalisés sur le disque
        /// </summary>
        private async Task SaveCustomTemplatesAsync()
        {
            foreach (var template in _customTemplates)
            {
                try
                {
                    var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
                    var filePath = Path.Combine(_templatesDirectory, $"{template.Id}.json");
                    await File.WriteAllTextAsync(filePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la sauvegarde du modèle {template.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Retourne tous les modèles disponibles (intégrés et personnalisés)
        /// </summary>
        public IReadOnlyList<NoteTemplate> GetAllTemplates()
        {
            return _builtInTemplates.Concat(_customTemplates).ToList().AsReadOnly();
        }

        /// <summary>
        /// Retourne les modèles filtrés par catégorie
        /// </summary>
        public IReadOnlyList<NoteTemplate> GetTemplatesByCategory(string category)
        {
            return _builtInTemplates.Concat(_customTemplates)
                .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Ajoute un nouveau modèle personnalisé
        /// </summary>
        public async Task AddCustomTemplateAsync(NoteTemplate template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            // Générer un ID s'il n'est pas défini
            if (string.IsNullOrEmpty(template.Id))
            {
                template.Id = Guid.NewGuid().ToString();
            }

            // Vérifier si le modèle existe déjà
            var existingTemplate = _customTemplates.FirstOrDefault(t => t.Id == template.Id);
            if (existingTemplate != null)
            {
                _customTemplates.Remove(existingTemplate);
            }

            template.IsBuiltIn = false;
            _customTemplates.Add(template);

            // Sauvegarder le modèle
            await SaveCustomTemplatesAsync();
        }

        /// <summary>
        /// Supprime un modèle personnalisé
        /// </summary>
        public async Task<bool> RemoveCustomTemplateAsync(string templateId)
        {
            var template = _customTemplates.FirstOrDefault(t => t.Id == templateId);
            if (template == null) return false;

            _customTemplates.Remove(template);

            // Supprimer le fichier du modèle
            var filePath = Path.Combine(_templatesDirectory, $"{templateId}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await SaveCustomTemplatesAsync();
            return true;
        }

        /// <summary>
        /// Applique les variables au contenu du modèle
        /// </summary>
        public string ApplyTemplateVariables(string content)
        {
            // Remplacer les variables de date et heure
            content = content.Replace("{{date}}", DateTime.Now.ToShortDateString());
            content = content.Replace("{{time}}", DateTime.Now.ToShortTimeString());

            // Autres variables peuvent être ajoutées ici

            return content;
        }
    }

    /// <summary>
    /// Représente un modèle de note
    /// </summary>
    public class NoteTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsBuiltIn { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}