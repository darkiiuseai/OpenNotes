using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        }
    }
}