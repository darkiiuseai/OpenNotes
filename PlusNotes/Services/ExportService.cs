using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlusNotes.Models;

namespace PlusNotes.Services
{
    public class ExportService
    {
        public async Task ExportNoteAsync(Note note)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Exporter la note",
                InitialFileName = SanitizeFileName(note.Title) + ".md",
                Filters = new()
                {
                    new() { Name = "Markdown", Extensions = new() { "md" } },
                    new() { Name = "Texte", Extensions = new() { "txt" } },
                    new() { Name = "HTML", Extensions = new() { "html" } }
                }
            };

            var filePath = await dialog.ShowAsync(App.MainWindow);
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var content = extension switch
            {
                ".html" => ConvertToHtml(note),
                ".md" => ConvertToMarkdown(note),
                _ => ConvertToText(note)
            };

            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
        }

        private string ConvertToMarkdown(Note note)
        {
            var sb = new StringBuilder();
            
            // Titre
            sb.AppendLine($"# {note.Title}");
            sb.AppendLine();
            
            // Métadonnées
            sb.AppendLine($"*Créée le: {note.CreatedAt:dd/MM/yyyy HH:mm}*");
            sb.AppendLine($"*Modifiée le: {note.ModifiedAt:dd/MM/yyyy HH:mm}*");
            
            if (!string.IsNullOrEmpty(note.Category))
            {
                sb.AppendLine($"*Catégorie: {note.Category}*");
            }
            
            if (note.Tags.Count > 0)
            {
                sb.AppendLine($"*Tags: {string.Join(", ", note.Tags)}*");
            }
            
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
            
            // Contenu
            sb.AppendLine(note.Content);
            
            return sb.ToString();
        }

        private string ConvertToHtml(Note note)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine($"<title>{EscapeHtml(note.Title)}</title>");
            sb.AppendLine("<meta charset=\"utf-8\">");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; max-width: 800px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine("h1 { color: #333; }");
            sb.AppendLine(".metadata { color: #666; font-style: italic; margin-bottom: 20px; }");
            sb.AppendLine(".tag { background-color: #e0e0e0; padding: 2px 6px; border-radius: 3px; margin-right: 5px; }");
            sb.AppendLine(".content { border-top: 1px solid #ddd; padding-top: 20px; white-space: pre-wrap; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            
            // Titre
            sb.AppendLine($"<h1>{EscapeHtml(note.Title)}</h1>");
            
            // Métadonnées
            sb.AppendLine("<div class=\"metadata\">");
            sb.AppendLine($"<div>Créée le: {note.CreatedAt:dd/MM/yyyy HH:mm}</div>");
            sb.AppendLine($"<div>Modifiée le: {note.ModifiedAt:dd/MM/yyyy HH:mm}</div>");
            
            if (!string.IsNullOrEmpty(note.Category))
            {
                sb.AppendLine($"<div>Catégorie: {EscapeHtml(note.Category)}</div>");
            }
            
            if (note.Tags.Count > 0)
            {
                sb.Append("<div>Tags: ");
                foreach (var tag in note.Tags)
                {
                    sb.Append($"<span class=\"tag\">{EscapeHtml(tag)}</span>");
                }
                sb.AppendLine("</div>");
            }
            
            sb.AppendLine("</div>");
            
            // Contenu
            sb.AppendLine($"<div class=\"content\">{EscapeHtml(note.Content).Replace("\n", "<br>")}</div>");
            
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            return sb.ToString();
        }

        private string ConvertToText(Note note)
        {
            var sb = new StringBuilder();
            
            // Titre
            sb.AppendLine(note.Title);
            sb.AppendLine(new string('=', note.Title.Length));
            sb.AppendLine();
            
            // Métadonnées
            sb.AppendLine($"Créée le: {note.CreatedAt:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Modifiée le: {note.ModifiedAt:dd/MM/yyyy HH:mm}");
            
            if (!string.IsNullOrEmpty(note.Category))
            {
                sb.AppendLine($"Catégorie: {note.Category}");
            }
            
            if (note.Tags.Count > 0)
            {
                sb.AppendLine($"Tags: {string.Join(", ", note.Tags)}");
            }
            
            sb.AppendLine();
            sb.AppendLine(new string('-', 80));
            sb.AppendLine();
            
            // Contenu
            sb.AppendLine(note.Content);
            
            return sb.ToString();
        }

        /// <summary>
        /// Exporte une collection de notes au format texte
        /// </summary>
        /// <param name="notes">Les notes à exporter</param>
        /// <param name="filePath">Le chemin du fichier de destination</param>
        /// <returns>Task représentant l'opération asynchrone</returns>
        public async Task ExportNotesAsTextAsync(IEnumerable<Note> notes, string filePath)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            var content = new StringBuilder();
            var notesList = notes.ToList();

            content.AppendLine($"EXPORT PLUSNOTES - {DateTime.Now:dd/MM/yyyy HH:mm}");
            content.AppendLine($"Nombre de notes: {notesList.Count}");
            content.AppendLine(new string('-', 50));
            content.AppendLine();

            foreach (var note in notesList)
            {
                content.AppendLine($"TITRE: {note.Title}");
                content.AppendLine($"Date: {note.ModifiedAt:dd/MM/yyyy HH:mm}");
                
                if (!string.IsNullOrWhiteSpace(note.Category))
                    content.AppendLine($"Catégorie: {note.Category}");
                
                if (note.Tags.Any())
                    content.AppendLine($"Tags: {string.Join(", ", note.Tags)}");
                
                content.AppendLine();
                content.AppendLine(note.Content);
                content.AppendLine();
                content.AppendLine(new string('-', 50));
                content.AppendLine();
            }

            await File.WriteAllTextAsync(filePath, content.ToString());
        }

        /// <summary>
        /// Exporte une note au format HTML
        /// </summary>
        /// <param name="note">La note à exporter</param>
        /// <param name="filePath">Le chemin du fichier de destination</param>
        /// <returns>Task représentant l'opération asynchrone</returns>
        public async Task ExportNoteAsHtmlAsync(Note note, string filePath)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"fr\">");
            html.AppendLine("<head>");
            html.AppendLine("  <meta charset=\"UTF-8\">");
            html.AppendLine($"  <title>{EscapeHtml(note.Title)}</title>");
            html.AppendLine("  <style>");
            html.AppendLine("    body { font-family: Arial, sans-serif; line-height: 1.6; max-width: 800px; margin: 0 auto; padding: 20px; }");
            html.AppendLine("    h1 { color: #333; }");
            html.AppendLine("    .meta { color: #666; font-size: 0.9em; margin-bottom: 20px; }");
            html.AppendLine("    .tag { background-color: #f0f0f0; padding: 2px 6px; border-radius: 3px; margin-right: 5px; font-size: 0.8em; }");
            html.AppendLine("    .content { border-top: 1px solid #eee; padding-top: 20px; white-space: pre-wrap; }");
            html.AppendLine("  </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"  <h1>{EscapeHtml(note.Title)}</h1>");
            html.AppendLine("  <div class=\"meta\">");
            html.AppendLine($"    <div>Créé le: {note.CreatedAt:dd/MM/yyyy HH:mm}</div>");
            html.AppendLine($"    <div>Modifié le: {note.ModifiedAt:dd/MM/yyyy HH:mm}</div>");
            
            if (!string.IsNullOrWhiteSpace(note.Category))
                html.AppendLine($"    <div>Catégorie: {EscapeHtml(note.Category)}</div>");
            
            if (note.Tags.Any())
            {
                html.AppendLine("    <div style=\"margin-top: 10px;\">Tags: ");
                foreach (var tag in note.Tags)
                {
                    html.AppendLine($"      <span class=\"tag\">{EscapeHtml(tag)}</span>");
                }
                html.AppendLine("    </div>");
            }
            
            html.AppendLine("  </div>");
            html.AppendLine($"  <div class=\"content\">{EscapeHtml(note.Content)}</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            await File.WriteAllTextAsync(filePath, html.ToString());
        }

        /// <summary>
        /// Échappe les caractères spéciaux HTML
        /// </summary>
        private string EscapeHtml(string text)
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }
    }
}