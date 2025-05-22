using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OpenNotes.Models;

namespace OpenNotes.Services
{
    public class NoteService
    {
        private readonly string _dataFolderPath;
        private readonly string _notesFilePath;

        public NoteService()
        {
            _dataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenNotes");
            _notesFilePath = Path.Combine(_dataFolderPath, "notes.json");
            
            // Créer le dossier de données s'il n'existe pas
            if (!Directory.Exists(_dataFolderPath))
            {
                Directory.CreateDirectory(_dataFolderPath);
            }
        }

        public async Task<ObservableCollection<Note>> LoadNotesAsync()
        {
            if (!File.Exists(_notesFilePath))
            {
                return new ObservableCollection<Note>();
            }

            try
            {
                var json = await File.ReadAllTextAsync(_notesFilePath);
                var notes = JsonSerializer.Deserialize<List<Note>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return notes != null 
                    ? new ObservableCollection<Note>(notes) 
                    : new ObservableCollection<Note>();
            }
            catch (Exception ex)
            {
                // En cas d'erreur, retourner une collection vide
                Console.WriteLine($"Erreur lors du chargement des notes: {ex.Message}");
                return new ObservableCollection<Note>();
            }
        }

        public async Task SaveNotesAsync(ObservableCollection<Note> notes)
        {
            try
            {
                var json = JsonSerializer.Serialize(notes, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                await File.WriteAllTextAsync(_notesFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde des notes: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Note>> SearchNotesAsync(string searchTerm, ObservableCollection<Note> notes)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return notes;
            }

            searchTerm = searchTerm.ToLowerInvariant();
            
            return notes.Where(n =>
                n.Title.ToLowerInvariant().Contains(searchTerm) ||
                n.Content.ToLowerInvariant().Contains(searchTerm) ||
                n.Tags.Any(t => t.ToLowerInvariant().Contains(searchTerm)) ||
                n.Category.ToLowerInvariant().Contains(searchTerm));
        }
    }
}