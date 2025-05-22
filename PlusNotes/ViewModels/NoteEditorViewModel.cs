using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlusNotes.Models;

namespace PlusNotes.ViewModels
{
    public partial class NoteEditorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Note? _currentNote;

        [ObservableProperty]
        private string _newTag = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _availableCategories = new();

        public NoteEditorViewModel(Note? note = null, ObservableCollection<string>? availableCategories = null)
        {
            CurrentNote = note;
            if (availableCategories != null)
            {
                AvailableCategories = availableCategories;
            }
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

        partial void OnCurrentNoteChanged(Note? value)
        {
            if (value != null && !string.IsNullOrWhiteSpace(value.Category) && !AvailableCategories.Contains(value.Category))
            {
                AvailableCategories.Add(value.Category);
            }
        }
    }
}