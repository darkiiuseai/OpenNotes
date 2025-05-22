using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;

namespace OpenNotes.Themes
{
    /// <summary>
    /// Thème dégradé translucide avec effet Mica pour OpenNotes
    /// </summary>
    public class TranslucentMicaTheme
    {
        public static readonly ThemeInfo ThemeInfo = new ThemeInfo
        {
            Id = "com.opennotes.theme.translucent_mica",
            Name = "Translucide Mica",
            Description = "Thème dégradé translucide avec effet Mica pour OpenNotes",
            Author = "OpenNotes Team",
            Type = ThemeType.Custom,
            IsBuiltIn = true,
            ColorPalette = new Dictionary<string, string>
            {
                { "Primary", "#3498db" },
                { "Secondary", "#2ecc71" },
                { "Background", "#ffffff80" }, // Translucide
                { "BackgroundBlur", "Mica" }, // Effet Mica
                { "Text", "#333333" },
                { "Accent", "#e74c3c" },
                { "Gradient1", "#3498db" }, // Couleur de début du dégradé
                { "Gradient2", "#9b59b6" }, // Couleur de fin du dégradé
                { "GradientAngle", "45" } // Angle du dégradé en degrés
            },
            LastUpdated = DateTime.Now
        };

        /// <summary>
        /// Enregistre le thème dans le gestionnaire de thèmes
        /// </summary>
        /// <param name="themeManager">Le gestionnaire de thèmes</param>
        public static void Register(ThemeManager themeManager)
        {
            if (themeManager == null) throw new ArgumentNullException(nameof(themeManager));
            
            // Vérifier si le thème existe déjà
            var existingTheme = themeManager.AvailableThemes.FirstOrDefault(t => t.Id == ThemeInfo.Id);
            if (existingTheme == null)
            {
                // Ajouter le thème à la liste des thèmes disponibles
                themeManager.AvailableThemes.Add(ThemeInfo);
            }
        }
    }
}