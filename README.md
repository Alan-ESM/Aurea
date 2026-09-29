Aurea
En quoi consiste le projet ?
Aurea est une application de bureau Windows développée en C# / WPF. Elle ressemble à un éditeur de documents enrichi, avec une interface graphique et plusieurs services annexes.

Fonctionnalités présentes dans le code
•	Création, ouverture et sauvegarde de documents.
•	Formats prévus : DOCX, TXT, RTF, PDF, Markdown et HTML.
•	Mise en forme : gras, italique, souligné, alignements, police, taille, couleur et surlignage.
•	Insertion de tableaux et d’images.
•	Export PDF.
•	Extraction de texte depuis des PDF.
•	Assistant IA pour analyser, corriger, améliorer, résumer, traduire et réécrire du texte.
•	Reconnaissance vocale et lecture vocale.
•	Capture caméra et détection de gestes.
•	Base SQLite locale pour les documents, collaborateurs, historiques et sauvegardes.
•	Publipostage, recherche, styles et récupération de fichiers prévus par des services dédiés.
•	Interface comportant plusieurs panneaux et fenêtres de configuration.

Ce qui semble déjà réalisé
Le projet est le plus avancé et le plus volumineux de l’archive : il comporte une vraie séparation en Models, Services, Database et Views. La fenêtre principale relie déjà les services de document, IA, caméra, voix et base de données. La gestion de plusieurs formats et l’interface graphique sont largement commencées.

Ce qui manque ou doit être corrigé
Priorité critique — sécurité :

•	Services/AIService.cs contient une clé API Google Gemini en clair dans le code source. Il faut immédiatement révoquer/renouveler cette clé, supprimer la valeur du dépôt, puis la charger via une variable d’environnement, un gestionnaire de secrets ou une configuration locale non versionnée.
•	La méthode de secours utilise également cette même clé dans une URL. Elle doit suivre la même règle de sécurité.

Fonctionnalités incomplètes :

•	CameraPanel.xaml.cs contient un TODO pour une détection avancée des gestes.
•	La détection de gestes n’est donc pas complète, même si la caméra et une partie du traitement existent.
•	ExportService.cs et FileRecoveryService.cs sont très courts par rapport aux fonctionnalités annoncées : ils doivent être vérifiés pour confirmer qu’ils sont réellement utilisés.
•	Le chargement Markdown convertit le Markdown en HTML mais affiche finalement le texte Markdown brut dans l’éditeur ; le rendu enrichi n’est pas réellement appliqué.
•	Le chargement HTML affiche également le HTML comme texte brut. Il manque un véritable convertisseur HTML vers le modèle WPF.
•	La conversion DOCX ne semble préserver qu’une partie de la mise en forme ; tableaux, images, styles avancés et autres éléments doivent être testés.
•	La gestion PDF est surtout une extraction de texte et un export de texte ; ce n’est pas encore un éditeur PDF complet.
•	Il faut vérifier les cas d’erreur : fichier verrouillé, chemin invalide, document vide, permission refusée, caméra absente, microphone absent et absence de connexion Internet.
•	Les appels IA envoient le contenu du document à un service externe : il faut informer l’utilisateur, prévoir un consentement et documenter la confidentialité.

Qualité et livraison :

•	Le projet cible net10.0-windows, ce qui impose un environnement .NET 10 récent et limite la portabilité.
•	Le fichier .csproj déclare deux fois UseWPF et indique UseWindowsForms=false, alors que le code utilise System.Windows.Forms.ColorDialog. La configuration doit être vérifiée et probablement ajustée.
•	Il faut ajouter des tests automatisés pour les formats, la base de données, les sauvegardes et les services.
•	Il manque une documentation utilisateur et une procédure d’installation.

Niveau estimé
Prototype avancé / pré-version, mais pas encore prêt pour une distribution publique à cause de la clé API exposée, de la dépendance à Windows et des fonctionnalités de conversion encore partielles.
