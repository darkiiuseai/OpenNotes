# Guide de développement d'extensions pour OpenNotes

Ce guide vous aidera à créer des extensions pour OpenNotes, permettant d'ajouter de nouvelles fonctionnalités à l'application.

## Introduction

Le système d'extensions d'OpenNotes permet aux développeurs d'étendre les fonctionnalités de l'application sans modifier le code source principal. Les extensions peuvent ajouter de nouvelles fonctionnalités, modifier l'interface utilisateur, ou intégrer des services externes.

## Structure d'une extension

Une extension OpenNotes est composée des éléments suivants :

1. **Fichier DLL** : Contient le code compilé de l'extension
2. **Fichier extension.json** : Contient les métadonnées de l'extension (optionnel mais recommandé)
3. **Ressources supplémentaires** : Images, fichiers de configuration, etc. (optionnel)

## Création d'une extension

### 1. Créer un projet de bibliothèque de classes

Commencez par créer un projet de bibliothèque de classes .NET compatible avec la version utilisée par OpenNotes.

```bash
dotnet new classlib -n MonExtension
```

### 2. Ajouter les références nécessaires

Ajoutez une référence à l'assembly OpenNotes.dll ou au package NuGet correspondant.

### 3. Implémenter l'interface IExtension

Créez une classe qui implémente l'interface `IExtension` :

```csharp
using System;
using System.Threading.Tasks;
using OpenNotes.Extensions;

namespace MonExtension
{
    public class MonExtensionPrincipale : IExtension
    {
        public string Id => "com.mondomaine.monextension";
        public string Name => "Mon Extension";
        public string? Description => "Description de mon extension";
        public string? Version => "1.0.0";
        public string? Author => "Votre Nom";
        
        public async Task InitializeAsync()
        {
            // Code d'initialisation
            await Task.CompletedTask;
        }
        
        public async Task ShutdownAsync()
        {
            // Code de nettoyage
            await Task.CompletedTask;
        }
        
        // Méthodes personnalisées
    }
}
```

### 4. Créer le fichier extension.json

Créez un fichier `extension.json` qui contient les métadonnées de votre extension :

```json
{
  "Id": "com.mondomaine.monextension",
  "Name": "Mon Extension",
  "Description": "Description de mon extension",
  "Version": "1.0.0",
  "Author": "Votre Nom",
  "RepositoryUrl": "https://github.com/votrecompte/monextension"
}
```

### 5. Compiler l'extension

Compilez votre projet pour générer le fichier DLL :

```bash
dotnet build -c Release
```

## Empaquetage de l'extension

Vous pouvez distribuer votre extension de deux façons :

### Option 1 : Fichier DLL unique

Distribuez simplement le fichier DLL compilé. Les utilisateurs pourront l'installer via le gestionnaire d'extensions d'OpenNotes.

### Option 2 : Package ZIP

Créez un fichier ZIP contenant :
- Le fichier DLL compilé
- Le fichier extension.json
- Toutes les ressources supplémentaires

Cette méthode est recommandée car elle permet d'inclure des métadonnées et des ressources supplémentaires.

## Bonnes pratiques

1. **Identifiant unique** : Utilisez un format de domaine inversé pour l'ID de votre extension (ex: com.mondomaine.monextension)
2. **Gestion des erreurs** : Capturez toutes les exceptions dans vos méthodes pour éviter de faire planter l'application
3. **Nettoyage des ressources** : Libérez toutes les ressources dans la méthode ShutdownAsync
4. **Versionnage sémantique** : Utilisez le versionnage sémantique (MAJOR.MINOR.PATCH) pour votre extension
5. **Documentation** : Fournissez une documentation claire sur l'utilisation de votre extension

## Mise à jour des extensions

Pour faciliter les mises à jour de votre extension :

1. Incrémentez le numéro de version dans le code et dans le fichier extension.json
2. Ajoutez un fichier CHANGELOG.md pour documenter les changements
3. Si vous utilisez GitHub, créez une release avec un tag correspondant à la version

Les utilisateurs pourront alors mettre à jour votre extension via le gestionnaire d'extensions d'OpenNotes.

## Exemples d'extensions

Vous pouvez consulter les exemples d'extensions dans le dossier `Examples` pour vous aider à démarrer.

## Ressources
à remplir

## Support

Si vous avez des questions ou des problèmes, n'hésitez pas à ouvrir une issue sur le dépôt GitHub du projet.