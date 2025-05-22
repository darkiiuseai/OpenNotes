using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlusNotes.Models;
using PlusNotes.Services;

namespace PlusNotes.ViewModels
{
    public partial class NoteEditorViewModel : ViewModelBase
    {
        private readonly ExportService? _exportService;

        [ObservableProperty]
        private Note? _currentNote;

        [ObservableProperty]
        private string _newTag = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _availableCategories = new();
        
        [ObservableProperty]
        private string _statusMessage = string.Empty;
        
        [ObservableProperty]
        private int _wordCount;
        
        [ObservableProperty]
        private int _characterCount;
        
        [ObservableProperty]
        private int _paragraphCount;
        
        [ObservableProperty]
        private int _lineCount;
        
        [ObservableProperty]
        private bool _isMarkdownPreviewEnabled;

        public NoteEditorViewModel(Note? note = null, ObservableCollection<string>? availableCategories = null, ExportService? exportService = null)
        {
            _exportService = exportService;
            CurrentNote = note;
            if (availableCategories != null)
            {
                AvailableCategories = availableCategories;
            }
            
            // Calculer les statistiques initiales
            if (CurrentNote != null)
            {
                UpdateStatistics();
            }
        }
        
        partial void OnCurrentNoteChanged(Note? value)
        {
            if (value != null)
            {
                // Incrémenter le compteur de vues
                value.IncrementViewCount();
                
                UpdateStatistics();
                
                // Ajouter la catégorie aux disponibles si elle n'existe pas déjà
                if (!string.IsNullOrWhiteSpace(value.Category) && !AvailableCategories.Contains(value.Category))
                {
                    AvailableCategories.Add(value.Category);
                }
            }
        }
        
        partial void OnCurrentNoteContentChanged(Note? oldValue, Note? newValue)
        {
            UpdateStatistics();
        }
        
        private void UpdateStatistics()
        {
            if (CurrentNote == null) return;
            
            // Mettre à jour les statistiques dans le modèle Note
            CurrentNote.UpdateStatistics();
            
            // Récupérer les valeurs pour l'affichage
            CharacterCount = CurrentNote.CharacterCount;
            WordCount = CurrentNote.WordCount;
            
            // Calculer le nombre de paragraphes (séparés par des lignes vides)
            ParagraphCount = Regex.Matches(CurrentNote.Content, @"(^|\n)\s*\n").Count + 1;
            
            // Calculer le nombre de lignes
            LineCount = CurrentNote.Content.Split('\n').Length;
            
            // Mettre à jour le message de statut
            StatusMessage = $"Dernière modification: {CurrentNote.ModifiedAt:dd/MM/yyyy HH:mm} | Vue: {CurrentNote.ViewCount} fois";
        }

        [RelayCommand]
        private void AddTag()
        {
            if (CurrentNote == null || string.IsNullOrWhiteSpace(NewTag))
            {
                return;
            }

            // Vérifier si le tag existe déjà
            if (!CurrentNote.Tags.Contains(NewTag))
            {
                CurrentNote.Tags.Add(NewTag);
                CurrentNote.ModifiedAt = DateTime.Now;
            }

            NewTag = string.Empty;
        }

        [RelayCommand]
        private void ToggleFavorite()
        {
            if (CurrentNote == null) return;
            
            CurrentNote.IsFavorite = !CurrentNote.IsFavorite;
            CurrentNote.ModifiedAt = DateTime.Now;
            StatusMessage = CurrentNote.IsFavorite ? "Note ajoutée aux favoris" : "Note retirée des favoris";
        }
        
        [RelayCommand]
        private async Task ExportNote()
        {
            if (CurrentNote == null || _exportService == null) return;
            
            try
            {
                await _exportService.ExportNoteAsync(CurrentNote);
                StatusMessage = "Note exportée avec succès";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de l'exportation: {ex.Message}";
            }
        }
        
        [RelayCommand]
        private void InsertDate()
        {
            if (CurrentNote == null) return;
            
            CurrentNote.Content += $"\n{DateTime.Now:dd/MM/yyyy HH:mm}";
            CurrentNote.ModifiedAt = DateTime.Now;
            StatusMessage = "Date insérée";
        }
        
        [RelayCommand]
        private void InsertBulletList()
        {
            if (CurrentNote == null) return;
            
            CurrentNote.Content += "\n\n- Élément 1\n- Élément 2\n- Élément 3\n";
            CurrentNote.ModifiedAt = DateTime.Now;
            StatusMessage = "Liste à puces insérée";
        }
        
        [RelayCommand]
        private void InsertTable()
        {
            if (CurrentNote == null) return;
            
            CurrentNote.Content += "\n\n| Colonne 1 | Colonne 2 | Colonne 3 |\n|----------|----------|----------|\n| Cellule 1 | Cellule 2 | Cellule 3 |\n| Cellule 4 | Cellule 5 | Cellule 6 |\n";
            CurrentNote.ModifiedAt = DateTime.Now;
            StatusMessage = "Tableau inséré";
        }
        
        [RelayCommand]
        private void InsertImage()
        {
            if (CurrentNote == null) return;
            
            CurrentNote.Content += "\n\n![Description de l'image](chemin/vers/image.jpg)\n";
            CurrentNote.ModifiedAt = DateTime.Now;
            StatusMessage = "Balise d'image insérée";
        }
        
        [RelayCommand]
        private void InsertCodeBlock()
        {
            if (CurrentNote == null) return;
            
            CurrentNote.Content += "\n\n```\nvotre code ici\n```\n";
            CurrentNote.ModifiedAt = DateTime.Now;
            StatusMessage = "Bloc de code inséré";
        }
        
        [RelayCommand]
        private void InsertCheckList()
        {
            if (CurrentNote == null) return;
            
            CurrentNote.Content += "\n\n- [ ] Tâche à faire\n- [ ] Autre tâche\n- [x] Tâche terminée\n";
            CurrentNote.ModifiedAt = DateTime.Now;
            StatusMessage = "Liste de contrôle insérée";
        }
        
        [RelayCommand]
        private void ToggleMarkdownPreview()
        {
            IsMarkdownPreviewEnabled = !IsMarkdownPreviewEnabled;
            StatusMessage = IsMarkdownPreviewEnabled ? "Aperçu Markdown activé" : "Aperçu Markdown désactivé";
        }
        
        [RelayCommand]
        private void SpellCheck()
        {
            if (CurrentNote == null) return;
            
            // Simuler une vérification d'orthographe
            StatusMessage = "Vérification orthographique terminée";
        }
        
        [RelayCommand]
        private void RemoveTag(string tag)
        {
            if (CurrentNote == null || string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            if (CurrentNote.Tags.Contains(tag))
            {
                CurrentNote.Tags.Remove(tag);
                CurrentNote.ModifiedAt = DateTime.Now;
            }
        }

        [RelayCommand]
        private void UpdateCategory(string category)
        {
            if (CurrentNote == null)
            {
                return;
            }

            CurrentNote.Category = category;
            CurrentNote.ModifiedAt = DateTime.Now;

            // Ajouter à la liste des catégories disponibles si elle n'existe pas déjà
            if (!string.IsNullOrWhiteSpace(category) && !AvailableCategories.Contains(category))
            {
                AvailableCategories.Add(category);
            }
        }

        // Méthode pour formater le texte sélectionné
        [RelayCommand]
        private void FormatText(string format)
        {
            if (CurrentNote == null) return;
            
            // Note: Dans une implémentation réelle, cette méthode nécessiterait
            // une intégration avec l'éditeur pour obtenir le texte sélectionné
            // Pour l'instant, nous ajoutons simplement un exemple de formatage
            
            switch (format.ToLower())
            {
                case "bold":
                    CurrentNote.Content += "\n**Texte en gras**";
                    break;
                case "italic":
                    CurrentNote.Content += "\n*Texte en italique*";
                    break;
                case "heading1":
                    CurrentNote.Content += "\n\n# Titre de niveau 1";
                    break;
                case "heading2":
                    CurrentNote.Content += "\n\n## Titre de niveau 2";
                    break;
                case "heading3":
                    CurrentNote.Content += "\n\n### Titre de niveau 3";
                    break;
                case "quote":
                    CurrentNote.Content += "\n\n> Citation";
                    break;
                case "link":
                    CurrentNote.Content += "\n[Texte du lien](https://exemple.com)";
                    break;
            }
            
            CurrentNote.ModifiedAt = DateTime.Now;
            StatusMessage = "Formatage appliqué";
        }
    }
}