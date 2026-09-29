using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Collections.Generic;
using System.Linq;

namespace Aurea.Services
{
    public class VoiceService : IDisposable
    {
        private SpeechRecognitionEngine recognizer;
        private SpeechSynthesizer synthesizer;
        private bool isListening = false;
        private Action<string> onTextRecognized;
        private CultureInfo cultureInfo;

        public VoiceService()
        {
            try
            {
                // Configuration pour le français
                cultureInfo = new CultureInfo("fr-FR");

                // 1. Initialisation de la reconnaissance vocale Windows
                recognizer = new SpeechRecognitionEngine(cultureInfo);

                // 2. Configuration de la grammaire
                ConfigureGrammar();

                // 3. Configuration des événements
                recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                recognizer.SpeechHypothesized += Recognizer_SpeechHypothesized;
                recognizer.SpeechRecognitionRejected += Recognizer_SpeechRecognitionRejected;
                recognizer.AudioLevelUpdated += Recognizer_AudioLevelUpdated;

                // 4. Initialisation du synthétiseur vocal
                synthesizer = new SpeechSynthesizer();

                // Chercher une voix française
                var frenchVoice = synthesizer.GetInstalledVoices(cultureInfo)
                    .FirstOrDefault(v => v.Enabled);

                if (frenchVoice != null)
                {
                    synthesizer.SelectVoice(frenchVoice.VoiceInfo.Name);
                }
                else
                {
                    // Fallback à la première voix disponible
                    var defaultVoice = synthesizer.GetInstalledVoices()
                        .FirstOrDefault(v => v.Enabled);
                    if (defaultVoice != null)
                        synthesizer.SelectVoice(defaultVoice.VoiceInfo.Name);
                }

                synthesizer.Volume = 100;
                synthesizer.Rate = 0; // Vitesse normale

                Debug.WriteLine("VoiceService initialisé avec reconnaissance vocale Windows");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur initialisation voix: {ex.Message}");
                // Si on ne peut pas initialiser avec le français, essayer avec la culture par défaut
                InitializeWithDefaultCulture();
            }
        }

        private void InitializeWithDefaultCulture()
        {
            try
            {
                recognizer = new SpeechRecognitionEngine();
                ConfigureGrammar();
                recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

                synthesizer = new SpeechSynthesizer();
                var defaultVoice = synthesizer.GetInstalledVoices()
                    .FirstOrDefault(v => v.Enabled);
                if (defaultVoice != null)
                    synthesizer.SelectVoice(defaultVoice.VoiceInfo.Name);

                Debug.WriteLine("VoiceService initialisé avec culture par défaut");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur initialisation fallback: {ex.Message}");
            }
        }

        private void ConfigureGrammar()
        {
            try
            {
                // Option 1: Grammaire par dictée (reconnaissance libre)
                recognizer.LoadGrammar(new DictationGrammar());

                // Option 2: Grammaire personnalisée pour les commandes spécifiques
                var commandsGrammar = new GrammarBuilder();
                commandsGrammar.Culture = cultureInfo;

                // Ajouter des commandes courantes
                var commandChoices = new Choices();
                commandChoices.Add("nouveau document");
                commandChoices.Add("enregistrer");
                commandChoices.Add("ouvrir");
                commandChoices.Add("imprimer");
                commandChoices.Add("annuler");
                commandChoices.Add("rétablir");
                commandChoices.Add("copier");
                commandChoices.Add("couper");
                commandChoices.Add("coller");
                commandChoices.Add("rechercher");
                commandChoices.Add("remplacer");
                commandChoices.Add("sélectionner tout");
                commandChoices.Add("gras");
                commandChoices.Add("italique");
                commandChoices.Add("souligner");
                commandChoices.Add("aligner à gauche");
                commandChoices.Add("centrer");
                commandChoices.Add("aligner à droite");
                commandChoices.Add("justifier");
                commandChoices.Add("arrêter la dictée");
                commandChoices.Add("activer la dictée");
                commandChoices.Add("lire le document");
                commandChoices.Add("arrêter la lecture");
                commandChoices.Add("bonjour");
                commandChoices.Add("salut");
                commandChoices.Add("test");
                commandChoices.Add("ok");
                commandChoices.Add("oui");
                commandChoices.Add("non");

                commandsGrammar.Append(commandChoices);
                recognizer.LoadGrammar(new Grammar(commandsGrammar));

                Debug.WriteLine("Grammaire configurée avec succès");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur configuration grammaire: {ex.Message}");
                // Charger juste la dictée si la grammaire personnalisée échoue
                try
                {
                    recognizer.LoadGrammar(new DictationGrammar());
                    Debug.WriteLine("Dictation grammar loaded as fallback");
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"Erreur chargement dictée: {ex2.Message}");
                }
            }
        }

        public async Task StartListening(Action<string> onTextRecognizedCallback)
        {
            if (recognizer == null)
            {
                Debug.WriteLine("Reconnaisseur non initialisé.");
                return;
            }

            try
            {
                onTextRecognized = onTextRecognizedCallback;

                if (!isListening)
                {
                    // Configurer l'entrée audio (microphone par défaut)
                    recognizer.SetInputToDefaultAudioDevice();

                    // Démarrer la reconnaissance asynchrone
                    recognizer.RecognizeAsync(RecognizeMode.Multiple);
                    isListening = true;

                    await SpeakText("Reconnaissance vocale activée. Je vous écoute.");

                    Debug.WriteLine("Écoute vocale démarrée");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur démarrage écoute: {ex.Message}");
                await SpeakText("Impossible de démarrer la reconnaissance vocale. Vérifiez votre microphone.");
            }
        }

        public async Task StopListening()
        {
            if (recognizer == null) return;

            try
            {
                if (isListening)
                {
                    recognizer.RecognizeAsyncStop();
                    isListening = false;
                    await SpeakText("Reconnaissance vocale désactivée.");
                    Debug.WriteLine("Écoute vocale arrêtée");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur arrêt écoute: {ex.Message}");
            }
        }

        public async Task SpeakText(string text)
        {
            if (synthesizer == null || string.IsNullOrEmpty(text)) return;

            try
            {
                // Vérifier si le synthétiseur est en train de parler
                if (synthesizer.State == SynthesizerState.Speaking)
                {
                    synthesizer.SpeakAsyncCancelAll();
                }

                // Parler le texte de manière asynchrone
                await Task.Run(() => synthesizer.SpeakAsync(text));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur synthèse vocale: {ex.Message}");
            }
        }

        public async Task ReadDocument(string documentText)
        {
            if (!string.IsNullOrEmpty(documentText))
            {
                // Lire seulement les premiers 500 caractères pour éviter de trop parler
                string textToRead = documentText.Length > 500
                    ? documentText.Substring(0, 500) + "... [suite tronquée]"
                    : documentText;

                await SpeakText(textToRead);
            }
        }

        public void PauseSpeaking()
        {
            if (synthesizer != null && synthesizer.State == SynthesizerState.Speaking)
            {
                synthesizer.Pause();
            }
        }

        public void ResumeSpeaking()
        {
            if (synthesizer != null && synthesizer.State == SynthesizerState.Paused)
            {
                synthesizer.Resume();
            }
        }

        public void StopSpeaking()
        {
            if (synthesizer != null)
            {
                synthesizer.SpeakAsyncCancelAll();
            }
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null && e.Result.Confidence > 0.3) // Seuil de confiance réduit
            {
                string recognizedText = e.Result.Text;

                Debug.WriteLine($"Reconnu ({e.Result.Confidence:F2}): {recognizedText}");

                // Traiter les commandes spéciales
                if (ProcessVoiceCommand(recognizedText))
                {
                    return;
                }

                // Appeler le callback avec le texte reconnu
                onTextRecognized?.Invoke(recognizedText);
            }
        }

        private bool ProcessVoiceCommand(string command)
        {
            command = command.ToLower();

            switch (command)
            {
                case "arrêter la dictée":
                    StopListening().Wait();
                    return true;

                case "activer la dictée":
                    StartListening(onTextRecognized).Wait();
                    return true;

                case "lire le document":
                    Debug.WriteLine("Commande: Lire le document");
                    return true;

                case "arrêter la lecture":
                    StopSpeaking();
                    return true;

                case "test":
                    Debug.WriteLine("Commande test reçue");
                    return true;

                case "bonjour":
                case "salut":
                    SpeakText("Bonjour !").Wait();
                    return true;
            }

            return false;
        }

        private void Recognizer_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            // Texte reconnu en cours (pour affichage en temps réel)
            Debug.WriteLine($"Hypothèse: {e.Result.Text}");
        }

        private void Recognizer_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Debug.WriteLine($"Rejeté: {e.Result?.Text ?? "Inconnu"}");
        }

        private void Recognizer_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
        {
            // Niveau audio en temps réel
            Debug.WriteLine($"Niveau audio: {e.AudioLevel}");
        }

        public void EnableDebugLogging(bool enable)
        {
            if (enable)
            {
                // Les événements sont déjà connectés au débogage
                Debug.WriteLine("Logs de débogage vocaux activés");
            }
        }

        public void UpdateGrammar(List<string> customWords)
        {
            if (recognizer == null || customWords == null) return;

            try
            {
                // Arrêter temporairement la reconnaissance
                bool wasListening = isListening;
                if (wasListening)
                {
                    recognizer.RecognizeAsyncStop();
                }

                // Recharger les grammaires avec les nouveaux mots
                recognizer.UnloadAllGrammars();
                ConfigureGrammar();

                // Ajouter une grammaire personnalisée
                var customGrammar = new GrammarBuilder();
                var customChoices = new Choices();

                foreach (var word in customWords)
                {
                    customChoices.Add(word);
                }

                customGrammar.Append(customChoices);
                recognizer.LoadGrammar(new Grammar(customGrammar));

                // Redémarrer la reconnaissance si elle était active
                if (wasListening)
                {
                    recognizer.RecognizeAsync(RecognizeMode.Multiple);
                }

                Debug.WriteLine($"Grammaire mise à jour avec {customWords.Count} mots");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur mise à jour grammaire: {ex.Message}");
            }
        }

        public List<string> GetAvailableVoices()
        {
            var voices = new List<string>();

            if (synthesizer != null)
            {
                foreach (var voice in synthesizer.GetInstalledVoices())
                {
                    if (voice.Enabled)
                    {
                        voices.Add(voice.VoiceInfo.Name);
                    }
                }
            }

            return voices;
        }

        public void SetVoice(string voiceName)
        {
            if (synthesizer != null)
            {
                try
                {
                    synthesizer.SelectVoice(voiceName);
                    Debug.WriteLine($"Voix définie sur: {voiceName}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur définition voix: {ex.Message}");
                }
            }
        }

        public void SetSpeechRate(int rate)
        {
            // Rate: -10 (très lent) à 10 (très rapide)
            if (synthesizer != null)
            {
                rate = Math.Clamp(rate, -10, 10);
                synthesizer.Rate = rate;
            }
        }

        public void SetSpeechVolume(int volume)
        {
            // Volume: 0 à 100
            if (synthesizer != null)
            {
                volume = Math.Clamp(volume, 0, 100);
                synthesizer.Volume = volume;
            }
        }

        public void SetContinuousDictation(bool enabled)
        {
            // Réinitialiser avec la grammaire appropriée
            if (recognizer != null)
            {
                try
                {
                    bool wasListening = isListening;
                    if (wasListening)
                    {
                        recognizer.RecognizeAsyncStop();
                    }

                    recognizer.UnloadAllGrammars();

                    if (enabled)
                    {
                        // Mode dictée uniquement
                        recognizer.LoadGrammar(new DictationGrammar());
                        Debug.WriteLine("Mode dictée continue activé");
                    }
                    else
                    {
                        // Mode commandes uniquement
                        ConfigureGrammar();
                        Debug.WriteLine("Mode commandes activé");
                    }

                    if (wasListening)
                    {
                        recognizer.RecognizeAsync(RecognizeMode.Multiple);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur changement mode dictée: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            try
            {
                StopListening().Wait(); // Attendre l'arrêt de l'écoute
                StopSpeaking();

                if (recognizer != null)
                {
                    recognizer.SpeechRecognized -= Recognizer_SpeechRecognized;
                    recognizer.SpeechHypothesized -= Recognizer_SpeechHypothesized;
                    recognizer.SpeechRecognitionRejected -= Recognizer_SpeechRecognitionRejected;
                    recognizer.AudioLevelUpdated -= Recognizer_AudioLevelUpdated;
                    recognizer.Dispose();
                    recognizer = null;
                }

                if (synthesizer != null)
                {
                    synthesizer.Dispose();
                    synthesizer = null;
                }

                Debug.WriteLine("VoiceService disposé");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors du dispose: {ex.Message}");
            }
        }

        // Méthode utilitaire pour vérifier si la reconnaissance vocale est disponible
        public static bool IsVoiceRecognitionAvailable()
        {
            try
            {
                using (var testRecognizer = new SpeechRecognitionEngine())
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // Méthode pour tester rapidement la reconnaissance
        public async Task<string> QuickTest()
        {
            try
            {
                string result = "";
                var tcs = new TaskCompletionSource<bool>();

                EventHandler<SpeechRecognizedEventArgs> handler = null;
                handler = (s, e) =>
                {
                    result = e.Result.Text;
                    tcs.TrySetResult(true);
                };

                recognizer.SpeechRecognized += handler;

                recognizer.SetInputToDefaultAudioDevice();
                recognizer.RecognizeAsync(RecognizeMode.Single);

                // Timeout après 5 secondes
                var timeoutTask = Task.Delay(5000);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                recognizer.SpeechRecognized -= handler;
                recognizer.RecognizeAsyncStop();

                if (completedTask == timeoutTask)
                {
                    return "TIMEOUT: Aucune parole détectée";
                }

                return $"SUCCÈS: '{result}'";
            }
            catch (Exception ex)
            {
                return $"ERREUR: {ex.Message}";
            }
        }
    }
}