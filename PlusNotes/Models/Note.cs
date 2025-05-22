using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PlusNotes.Models
{
    public partial class Note : ObservableObject
    {
        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _content = string.Empty;

        [ObservableProperty]
        private DateTime _createdAt;

        [ObservableProperty]
        private DateTime _modifiedAt;

        [ObservableProperty]
        private bool _isFavorite;

        [ObservableProperty]
        private string _category = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _tags = new();
        
        // Propriétés calculées pour les statistiques et Discord Rich Presence
        [ObservableProperty]
        private int _wordCount;
        
        [ObservableProperty]
        private int _characterCount;
        
        [ObservableProperty]
        private DateTime _lastViewedAt;
        
        [ObservableProperty]
        private int _viewCount;

        public Note()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            ModifiedAt = DateTime.Now;
        }

        public Note(string title, string content) : this()
        {
            Title = title;
            Content = content;
            LastViewedAt = DateTime.Now;
            UpdateStatistics();
        }
        
        partial void OnContentChanged(string value)
        {
            UpdateStatistics();
        }
        
        /// <summary>
        /// Met à jour les statistiques de la note
        /// </summary>
        public void UpdateStatistics()
        {
            CharacterCount = Content.Length;
            WordCount = Regex.Matches(Content, @"\w+").Count;
        }
        
        /// <summary>
        /// Incrémente le compteur de vues et met à jour la date de dernière consultation
        /// </summary>
        public void IncrementViewCount()
        {
            ViewCount++;
            LastViewedAt = DateTime.Now;
        }
    }
}