# Guide de développement de thèmes pour OpenNotes

Ce guide vous aidera à créer des thèmes personnalisés pour OpenNotes, permettant de modifier l'apparence de l'application selon vos préférences.

## Introduction

Le système de thèmes d'OpenNotes permet aux utilisateurs et aux développeurs de personnaliser l'apparence de l'application. Les thèmes peuvent modifier les couleurs, les polices, les icônes et d'autres éléments visuels.

## Structure d'un thème

Un thème OpenNotes est défini par un fichier JSON contenant les informations suivantes :

1. **Métadonnées** : Informations sur le thème (nom, auteur, version, etc.)
2. **Palette de couleurs** : Définition des couleurs utilisées dans l'application
3. **Styles** : Définition des styles pour les différents éléments de l'interface

## Création d'un thème

### 1. Utiliser le modèle de thème

La façon la plus simple de créer un thème est d'utiliser le modèle fourni par OpenNotes. Dans l'application, allez dans Paramètres > Thèmes > Créer un modèle de thème.

### 2. Structure du fichier theme.json

Voici un exemple de structure pour un fichier `theme.json` :

```json
{
  "Id": "com.mondomaine.montheme",
  "Name": "Mon Thème Personnalisé",
  "Description": "Un thème personnalisé pour OpenNotes",
  "Author": "Votre Nom",
  "Type": "Custom",
  "ColorPalette": {
    "Primary": "#3498db",
    "Secondary": "#2ecc71",
    "Background": "#ffffff",
    "Text": "#333333",
    "Accent": "#e74c3c",
    "Error": "#e74c3c",
    "Warning": "#f39c12",
    "Success": "#2ecc71",
    "Info": "#3498db"
  }
}
```

### 3. Personnalisation des couleurs

Les couleurs sont définies au format hexadécimal (#RRGGBB) ou avec des noms de couleurs CSS. Voici les principales couleurs que vous pouvez personnaliser :

- **Primary** : Couleur principale de l'application
- **Secondary** : Couleur secondaire
- **Background** : Couleur de fond
- **Text** : Couleur du texte
- **Accent** : Couleur d'accentuation
- **Error** : Couleur pour les erreurs
- **Warning** : Couleur pour les avertissements
- **Success** : Couleur pour les succès
- **Info** : Couleur pour les informations

## Test de votre thème

Pour tester votre thème :

1. Enregistrez votre fichier theme.json
2. Dans OpenNotes, allez dans Paramètres > Thèmes > Importer un thème
3. Sélectionnez votre fichier theme.json
4. Appliquez le thème pour voir les changements

## Distribution de votre thème

Vous pouvez distribuer votre thème de plusieurs façons :

### Option 1 : Fichier JSON

Partagez simplement le fichier theme.json avec les utilisateurs, qui pourront l'importer via le gestionnaire de thèmes.

### Option 2 : Dépôt GitHub

Créez un dépôt GitHub contenant votre thème. Les utilisateurs pourront alors l'installer directement depuis l'URL du dépôt via l'option "Télécharger depuis GitHub" dans le gestionnaire de thèmes.

Structure recommandée pour un dépôt de thème :
```
/
├── theme.json       # Définition du thème
├── preview.png      # Aperçu du thème (optionnel)
├── README.md        # Documentation
└── CHANGELOG.md     # Journal des modifications (optionnel)
```

## Mise à jour de votre thème

Pour faciliter les mises à jour de votre thème :

1. Incrémentez le numéro de version dans le fichier theme.json
2. Mettez à jour votre dépôt GitHub
3. Si vous utilisez GitHub, créez une release avec un tag correspondant à la version

Les utilisateurs pourront alors mettre à jour votre thème via le gestionnaire de thèmes d'OpenNotes.

## Bonnes pratiques

1. **Contraste** : Assurez-vous que le contraste entre le texte et l'arrière-plan est suffisant pour une bonne lisibilité
2. **Cohérence** : Maintenez une cohérence visuelle dans l'ensemble de votre thème
3. **Accessibilité** : Pensez aux utilisateurs ayant des déficiences visuelles
4. **Test** : Testez votre thème dans différentes conditions (mode jour/nuit, différentes résolutions)

## Exemples de thèmes

Vous pouvez consulter les exemples de thèmes dans le dossier `Examples` pour vous aider à démarrer.

## Ressources

- [Documentation complète d'OpenNotes](https://github.com/yourusername/opennotes/wiki)
- [Forum de la communauté](https://forum.opennotes.com)
- [Outils de création de palettes de couleurs](https://coolors.co/)

## Support

Si vous avez des questions ou des problèmes, n'hésitez pas à ouvrir une issue sur le dépôt GitHub du projet.