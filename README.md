# Aurea

Aurea est une application de bureau Windows en C# et WPF, conçue comme un éditeur de documents enrichi. Le projet est un prototype en cours de développement : les fonctionnalités annoncées ne sont pas toutes équivalentes à une suite bureautique finalisée.

## Fonctionnalités présentes ou commencées

- Création, ouverture et sauvegarde de documents.
- Formats pris en charge par le service : DOCX, TXT, RTF, PDF, Markdown et HTML.
- Mise en forme de texte : gras, italique, souligné, alignements, police, taille, couleur et surlignage.
- Insertion de tableaux et d’images.
- Export PDF et extraction de texte depuis les PDF.
- Assistant IA pour analyser, corriger, améliorer, résumer, traduire et reformuler du texte.
- Reconnaissance vocale et lecture vocale Windows.
- Capture caméra et enregistrement vidéo; la détection avancée des gestes reste à faire.
- Base SQLite locale pour les documents, les collaborateurs, l’historique et les sauvegardes.
- Interface composée de fenêtres et de panneaux annexes.

## Configuration de l’assistant IA

Aurea ne contient plus de clé Google Gemini dans le code source. Pour activer l’assistant, définissez `AUREA_GEMINI_API_KEY` avant de lancer l’application. Dans PowerShell, pour la session en cours :

```powershell
$env:AUREA_GEMINI_API_KEY = "votre-nouvelle-clé"
```

Ne placez pas la clé dans un fichier suivi par Git. Une clé ayant été exposée dans l’historique du dépôt doit être révoquée et remplacée dans la console du fournisseur : retirer une clé du dernier état du code ne l’efface pas des anciens commits.

Les requêtes IA transmettent le texte demandé au service externe Google. N’envoyez aucun contenu confidentiel sans avoir obtenu le consentement requis.

## Compilation et tests

Le projet cible `net10.0-windows` et requiert Windows, le SDK .NET 10, ainsi que la restauration des packages NuGet. Depuis le dossier du projet :

```powershell
dotnet restore Aurea.slnx
dotnet build Aurea.slnx --no-restore
dotnet test Aurea.slnx --no-build
```

Aucun projet de tests automatisés n’est actuellement défini dans la solution. Les tests manuels devraient couvrir au minimum la création/ouverture/sauvegarde de chaque format, les sauvegardes SQLite, les erreurs de fichiers, l’absence de caméra ou de microphone et le mode IA sans clé configurée.

## Limites et travaux restants

- Le chargement Markdown et HTML n’effectue pas encore une conversion complète vers la mise en forme WPF; le contenu peut apparaître comme texte brut.
- La conversion DOCX ne préserve qu’une partie de la mise en forme; tableaux, images et styles avancés nécessitent des tests supplémentaires.
- La gestion des PDF porte principalement sur l’extraction de texte et l’export de texte, pas sur l’édition complète de PDF.
- `ExportService.cs` et `FileRecoveryService.cs` sont des services vides ou incomplets.
- Le projet n’a pas encore de suite de tests automatisés ni de guide utilisateur complet.

## Corrections effectuées dans cette passe

- Retrait de la clé Gemini codée en dur et chargement depuis `AUREA_GEMINI_API_KEY`.
- Activation de Windows Forms, utilisé par le sélecteur de couleur, et suppression de la propriété WPF dupliquée.
- Correction du chemin de l’icône de la fenêtre et inclusion de la ressource ICO.
- Réparation de la disposition de la fenêtre principale, qui utilisait des marges fixes pouvant superposer l’éditeur et la barre d’état.
- Ajout des filtres Markdown et HTML dans les boîtes de dialogue d’ouverture et d’enregistrement.
- Création/vérification des tables SQLite à chaque démarrage et noms de sauvegardes plus uniques.
- Échappement du texte dans l’export HTML.
