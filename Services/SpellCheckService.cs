using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Aurea.Services
{
    public class SpellCheckService
    {
        private Dictionary<string, List<string>> customDictionary;

        public SpellCheckService()
        {
            customDictionary = new Dictionary<string, List<string>>();
            LoadDefaultDictionaries();
        }

        private void LoadDefaultDictionaries()
        {
            // Dictionnaires français et anglais de base
            customDictionary["fr"] = new List<string> { "bonjour", "merci", "aurea", "traitement" };
            customDictionary["en"] = new List<string> { "hello", "thank", "document", "processing" };
        }

        public bool CheckWord(string word, string language)
        {
            if (string.IsNullOrWhiteSpace(word))
                return true;

            word = word.ToLower().Trim();

            if (customDictionary.ContainsKey(language))
            {
                return customDictionary[language].Contains(word);
            }

            return true; // Par défaut, accepter le mot
        }

        public List<string> GetSuggestions(string word, string language)
        {
            var suggestions = new List<string>();

            if (string.IsNullOrWhiteSpace(word))
                return suggestions;

            word = word.ToLower();

            if (customDictionary.ContainsKey(language))
            {
                // Trouver les mots similaires (distance de Levenshtein simplifiée)
                foreach (var dictWord in customDictionary[language])
                {
                    if (AreSimilar(word, dictWord))
                    {
                        suggestions.Add(dictWord);
                    }
                }
            }

            return suggestions.Take(5).ToList();
        }

        private bool AreSimilar(string word1, string word2)
        {
            if (word1.Length < 3 || word2.Length < 3)
                return false;

            // Vérifier si les mots commencent pareil
            if (word1.Substring(0, Math.Min(2, word1.Length)) == word2.Substring(0, Math.Min(2, word2.Length)))
                return true;

            return false;
        }

        public void AddWordToDictionary(string word, string language)
        {
            if (!customDictionary.ContainsKey(language))
            {
                customDictionary[language] = new List<string>();
            }

            if (!customDictionary[language].Contains(word.ToLower()))
            {
                customDictionary[language].Add(word.ToLower());
            }
        }
    }
}