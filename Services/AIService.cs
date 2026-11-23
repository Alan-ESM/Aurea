using Google.GenAI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Aurea.Services
{
    public sealed class AIService
    {
        private readonly Client? client;

        public AIService()
        {
            string? apiKey = Environment.GetEnvironmentVariable("AUREA_GEMINI_API_KEY");
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client = new Client(apiKey: apiKey);
            }
        }

        public async Task<string> GetResponse(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "Saisissez un texte ou une question.";

            if (client == null)
            {
                return "L’assistant IA n’est pas configuré. Définissez la variable d’environnement AUREA_GEMINI_API_KEY puis redémarrez Aurea.";
            }

            try
            {
                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-2.0-flash",
                    contents: userMessage);

                string? responseText = response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
                return string.IsNullOrWhiteSpace(responseText)
                    ? "Désolé, je n’ai pas pu générer de réponse."
                    : responseText;
            }
            catch (Exception ex)
            {
                return $"Erreur du service IA : {ex.Message}";
            }
        }

        public Task<string> AnalyzeDocument(string content) =>
            GetResponse($"Analyse ce document et donne-moi un résumé et des suggestions d’amélioration :\n\n{content}");

        public Task<string> ImproveText(string text) =>
            GetResponse($"Améliore ce texte en corrigeant les fautes et en améliorant le style :\n\n{text}");

        public Task<string> CorrectText(string text) =>
            GetResponse($"Corrige uniquement les fautes d’orthographe et de grammaire dans ce texte :\n\n{text}");

        public Task<string> Summarize(string text) =>
            GetResponse($"Fais un résumé concis de ce texte :\n\n{text}");

        public Task<string> Translate(string text, string targetLanguage) =>
            GetResponse($"Traduis ce texte en {targetLanguage} :\n\n{text}");

        public Task<string> Rewrite(string text, string style) =>
            GetResponse($"Réécris ce texte dans un style {style} :\n\n{text}");
    }
}
