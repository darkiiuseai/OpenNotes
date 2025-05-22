using System;
using System.Collections.ObjectModel;
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

        public MainWindowViewModel()
        {
            _noteService = new NoteService();
            LoadNotesAsync().ConfigureAwait(false);
        }

        private async Task LoadNotesAsync()
        {
            try
            {
                Notes = await _noteService.LoadNotesAsync();
                StatusMessage = $"{Notes.Count} notes chargées";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des notes: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task SaveNotesAsync()
        {
            try
            {
                await _noteService.SaveNotesAsync(Notes);
                StatusMessage = "Notes sauvegardées avec succès";
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
                SaveNotesCommand.Execute(null);
            }
        }

        partial void OnSelectedNoteChanged(Note? value)
        {
            if (value != null)
            {
                IsEditing = false;
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
