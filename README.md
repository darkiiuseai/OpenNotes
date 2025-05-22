# OpenNotes - Application de Prise de Notes Avancée

OpenNotes est une application de prise de notes moderne et extensible, inspirée par LibreOffice et conçue pour être personnalisable.

## Fonctionnalités principales

- **Éditeur Markdown** : Créez et modifiez des notes avec prise en charge complète du Markdown
- **Organisation par catégories et tags** : Classez vos notes efficacement
- **Statistiques détaillées** : Suivez votre productivité avec des statistiques sur vos notes
- **Système de thèmes** : Personnalisez l'apparence de l'application
- **Système d'extensions** : Étendez les fonctionnalités selon vos besoins
- **Mises à jour automatiques** : Restez à jour avec les dernières fonctionnalités

## Système de thèmes

Personnalisez l'apparence d'OpenNotes selon vos préférences.

### Thèmes intégrés
- **Clair (Défaut)** : Thème lumineux et épuré
- **Sombre** : Thème sombre pour réduire la fatigue oculaire
- **Système** : S'adapte au thème de votre système d'exploitation
- **Translucide Mica** : Thème dégradé translucide avec effet Mica

### Création de thèmes personnalisés

1. Dans les paramètres, accédez à l'onglet "Thèmes"
2. Cliquez sur "Créer un modèle de thème"
3. Personnalisez les couleurs et les styles
4. Enregistrez et appliquez votre thème

## Système d'extensions

OpenNotes dispose d'un système d'extensions puissant qui vous permet d'ajouter de nouvelles fonctionnalités selon vos besoins.

### Extensions intégrées

#### Exportation Avancée
Exportez vos notes dans différents formats :
- PDF
- HTML
- Markdown
- Texte brut
- RTF
- DOCX

#### Organisateur Automatique
Organise et catégorise automatiquement vos notes en fonction de leur contenu :
- Détection de catégories
- Suggestion de tags
- Génération de titres

#### Modèles de Notes
Utilisez des modèles prédéfinis pour différents types de notes :
- Compte-rendu de réunion
- Plan de projet
- Journal quotidien
- Recette de cuisine
- Notes de cours

#### Synchronisation Cloud
Synchronisez vos notes avec différents services cloud :
- Google Drive
- OneDrive
- Dropbox

### Création d'extensions personnalisées

1. Dans les paramètres, accédez à l'onglet "Extensions"
2. Cliquez sur "Créer un modèle d'extension"
3. Enregistrez le modèle et développez votre extension
4. Importez l'extension dans l'application

## Système de mises à jour

OpenNotes vérifie automatiquement les mises à jour au démarrage. Vous pouvez également :

- Vérifier manuellement les mises à jour depuis l'écran d'accueil
- Installer les mises à jour en un clic
- Vérifier les mises à jour des thèmes et extensions installés

## Présentation

OpenNotes est une application de prise de notes moderne et puissante développée avec Avalonia UI. Elle offre une interface utilisateur élégante et intuitive ainsi que de nombreuses fonctionnalités avancées pour gérer efficacement vos notes.

## Fonctionnalités

- **Édition de texte riche** : Créez et modifiez des notes avec un éditeur de texte complet
- **Organisation par catégories** : Classez vos notes par catégories pour une meilleure organisation
- **Système de tags** : Ajoutez des tags à vos notes pour faciliter la recherche et le filtrage
- **Recherche avancée** : Recherchez rapidement dans toutes vos notes par titre, contenu, catégorie ou tag
- **Favoris** : Marquez vos notes importantes comme favorites pour y accéder plus rapidement
- **Statistiques** : Visualisez des statistiques sur votre collection de notes
- **Sauvegarde automatique** : Vos notes sont automatiquement sauvegardées dans un fichier JSON

## Architecture

L'application est construite selon le modèle MVVM (Model-View-ViewModel) avec les composants suivants :

- **Models** : Définition des objets de données (Note)
- **ViewModels** : Logique de présentation et de gestion des données
- **Views** : Interface utilisateur en XAML
- **Services** : Gestion de la persistance des données et des mises à jour

## Utilisation

### Créer une nouvelle note

1. Cliquez sur le bouton "Nouvelle note" dans la barre d'outils
2. Donnez un titre à votre note
3. Ajoutez du contenu dans l'éditeur
4. Assignez une catégorie et des tags si nécessaire

### Rechercher des notes

1. Entrez votre terme de recherche dans la barre de recherche
2. Appuyez sur Entrée ou cliquez sur l'icône de recherche
3. Les résultats s'afficheront dans la liste des notes

### Afficher les statistiques

Cliquez sur le bouton "Statistiques" dans la barre d'outils pour afficher ou masquer le panneau de statistiques.

## Développement

### Prérequis

- .NET 7.0 ou supérieur
- Avalonia UI 11.0.0 ou supérieur
- CommunityToolkit.Mvvm 8.2.0 ou supérieur

### Compilation

```bash
dotnet build
```

### Exécution

```bash
dotnet run
```

## Licence

Ce projet est sous licence MIT.