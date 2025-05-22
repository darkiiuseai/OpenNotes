using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlusNotes.Models;
using PlusNotes.Services;

namespace PlusNotes.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly NoteService _noteService;
        private readonly ExportService _exportService;
        private readonly ThemeManager _themeManager;
        private readonly ExtensionManager _extensionManager;

        [ObservableProperty]
        private ObservableCollection<Note> _notes = new();

        [ObservableProperty]
        private Note? _selectedNote;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _showStatsView;

        [ObservableProperty]
        private NoteEditorViewModel? _noteEditorViewModel;

        [ObservableProperty]
        private StatsViewModel? _statsViewModel;

        [ObservableProperty]
        private ObservableCollection<string> _availableCategories = new();
        
        [ObservableProperty]
        private SettingsViewModel? _settingsViewModel;
        
        [ObservableProperty]
        private bool _showSettingsView;

        public MainWindowViewModel()
        {
            _noteService = new NoteService();
            _exportService = new ExportService();
            _themeManager = new ThemeManager();
            _extensionManager = new ExtensionManager();
            
            // Initialiser les ViewModels
            LoadNotesAsync().ConfigureAwait(false);
            StatsViewModel = new StatsViewModel(Notes);
            SettingsViewModel = new SettingsViewModel();
        }

        private async Task LoadNotesAsync()
        {
            try
            {
                Notes = await _noteService.LoadNotesAsync();
                StatusMessage = $"{Notes.Count} notes chargées";
                
                // Extraire les catégories uniques
                UpdateAvailableCategories();
                
                // Mettre à jour les statistiques
                if (StatsViewModel != null)
                {
                    StatsViewModel.UpdateStats();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des notes: {ex.Message}";
            }
        }

        private void UpdateAvailableCategories()
        {
            var categories = Notes
                .Select(n => n.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
                
            AvailableCategories.Clear();
            foreach (var category in categories)
            {
                AvailableCategories.Add(category);
            }
        }

        [RelayCommand]
        private async Task SaveNotesAsync()
        {
            try
            {
                await _noteService.SaveNotesAsync(Notes);
                StatusMessage = "Notes sauvegardées avec succès";
                
                // Mettre à jour les statistiques
                if (StatsViewModel != null)
                {
                    StatsViewModel.UpdateStats();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de la sauvegarde: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CreateNewNote()
        {
            var newNote = new Note("Nouvelle note", string.Empty);
            Notes.Add(newNote);
            SelectedNote = newNote;
            IsEditing = true;
        }

        [RelayCommand]
        private void DeleteNote(Note? note)
        {
            if (note != null && Notes.Contains(note))
            {
                Notes.Remove(note);
                if (SelectedNote == note)
                {
                    SelectedNote = Notes.FirstOrDefault();
                }
                SaveNotesCommand.Execute(null);
            }
        }

        [RelayCommand]
        private async Task SearchNotesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadNotesAsync();
                return;
            }

            try
            {
                var results = await _noteService.SearchNotesAsync(SearchText, Notes);
                Notes = new ObservableCollection<Note>(results);
                StatusMessage = $"{Notes.Count} résultats trouvés";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de la recherche: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ToggleFavorite(Note? note)
        {
            if (note != null)
            {
                note.IsFavorite = !note.IsFavorite;
                note.ModifiedAt = DateTime.Now;
                SaveNotesCommand.Execute(null);
            }
        }

        [RelayCommand]
        private void ToggleStatsView()
        {
            ShowStatsView = !ShowStatsView;
            ShowSettingsView = false;
            IsEditing = false;
            
            if (ShowStatsView && StatsViewModel != null)
            {
                StatsViewModel.UpdateStats();
            }
        }
        
        [RelayCommand]
        private void ShowSettings()
        {
            ShowSettingsView = true;
            ShowStatsView = false;
            IsEditing = false;
        }

        partial void OnSelectedNoteChanged(Note? value)
        {
            if (value != null)
            {
                IsEditing = false;
                NoteEditorViewModel = new NoteEditorViewModel(value, AvailableCategories, _exportService);
            }
            else
            {
                NoteEditorViewModel = null;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                LoadNotesAsync().ConfigureAwait(false);
            }
        }
    }
}
