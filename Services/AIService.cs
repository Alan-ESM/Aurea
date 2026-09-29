using Google.GenAI;
using Google.GenAI.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Aurea.Services
{
    public class AIService : IDisposable
    {
        private const string GEMINI_API_KEY = "AIzaSyAlwd1a6bxgCrFJYKELDQ_sqXfDv3-kp_E";
        private readonly Client client;
        private readonly HttpClient httpClient;

        public AIService()
        {
            // Initialiser avec Google.GenAI
            client = new Client(apiKey: GEMINI_API_KEY);
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> GetResponse(string userMessage)
        {
            try
            {
                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-2.0-flash",
                    contents: userMessage
                );

                if (response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text != null)
                {
                    return response.Candidates[0].Content.Parts[0].Text;
                }

                return "Désolé, je n'ai pas pu générer de réponse.";
            }
            catch (Exception ex)
            {
                return $"Erreur : {ex.Message}";
            }
        }

        public async Task<string> AnalyzeDocument(string content)
        {
            try
            {
                string prompt = $"Analyse ce document et donne-moi un résumé et des suggestions d'amélioration:\n\n{content}";
                return await GetResponse(prompt);
            }
            catch (Exception ex)
            {
                return $"Erreur analyse: {ex.Message}";
            }
        }

        public async Task<string> ImproveText(string text)
        {
            string prompt = $"Améliore ce texte en corrigeant les fautes et en améliorant le style:\n\n{text}";
            return await GetResponse(prompt);
        }

        public async Task<string> CorrectText(string text)
        {
            string prompt = $"Corrige uniquement les fautes d'orthographe et de grammaire dans ce texte:\n\n{text}";
            return await GetResponse(prompt);
        }

        public async Task<string> Summarize(string text)
        {
            string prompt = $"Fais un résumé concis de ce texte:\n\n{text}";
            return await GetResponse(prompt);
        }

        public async Task<string> Translate(string text, string targetLanguage)
        {
            string prompt = $"Traduis ce texte en {targetLanguage}:\n\n{text}";
            return await GetResponse(prompt);
        }

        public async Task<string> Rewrite(string text, string style)
        {
            string prompt = $"Réécris ce texte dans un style {style}:\n\n{text}";
            return await GetResponse(prompt);
        }

        // Méthode pour les appels de secours (si Google.GenAI échoue)
        private async Task<string> GetResponseFallback(string userMessage)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = userMessage }
                            }
                        }
                    }
                };

                string jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={GEMINI_API_KEY}";
                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(responseText);

                    var candidates = jsonResponse["candidates"];
                    if (candidates != null && candidates.HasValues)
                    {
                        string aiResponse = candidates[0]["content"]["parts"][0]["text"].ToString();
                        return aiResponse;
                    }
                }

                return "Désolé, je n'ai pas pu générer de réponse. Veuillez réessayer.";
            }
            catch (Exception ex)
            {
                return $"Erreur : {ex.Message}";
            }
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}