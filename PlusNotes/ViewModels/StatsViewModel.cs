using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PlusNotes.Models;

namespace PlusNotes.ViewModels
{
    public partial class StatsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int _totalNotes;

        [ObservableProperty]
        private int _favoriteNotes;

        [ObservableProperty]
        private int _categoryCount;

        [ObservableProperty]
        private DateTime? _lastModified;

        [ObservableProperty]
        private ObservableCollection<KeyValuePair<string, int>> _topCategories = new();

        private ObservableCollection<Note> _notes;

        public StatsViewModel(ObservableCollection<Note> notes)
        {
            _notes = notes;
            UpdateStats();
        }

        public void UpdateStats()
        {
            if (_notes == null || _notes.Count == 0)
            {
                TotalNotes = 0;
                FavoriteNotes = 0;
                CategoryCount = 0;
                LastModified = null;
                TopCategories.Clear();
                return;
            }

            TotalNotes = _notes.Count;
            FavoriteNotes = _notes.Count(n => n.IsFavorite);

            // Compter les catégories uniques non vides
            var categories = _notes
                .Select(n => n.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();
            
            CategoryCount = categories.Count;

            // Trouver la dernière note modifiée
            LastModified = _notes.Max(n => n.ModifiedAt);

            // Calculer les catégories les plus populaires
            var categoryStats = _notes
                .Where(n => !string.IsNullOrWhiteSpace(n.Category))
                .GroupBy(n => n.Category)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .OrderByDescending(kvp => kvp.Value)
                .Take(5);

            TopCategories.Clear();
            foreach (var category in categoryStats)
            {
                TopCategories.Add(category);
            }
        }
    }
}