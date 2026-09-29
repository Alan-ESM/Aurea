using RichTextBox = System.Windows.Controls.RichTextBox;
using Brushes = System.Windows.Media.Brushes;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Aurea.Services
{
    public class SearchResult
    {
        public TextPointer Start { get; set; }
        public TextPointer End { get; set; }
        public string Text { get; set; }
    }

    public class SearchService
    {
        private List<SearchResult> searchResults;
        private int currentResultIndex = -1;

        public SearchService()
        {
            searchResults = new List<SearchResult>();
        }

        public List<SearchResult> FindAll(RichTextBox editor, string searchText, bool caseSensitive = false)
        {
            searchResults.Clear();
            currentResultIndex = -1;

            if (string.IsNullOrEmpty(searchText))
                return searchResults;

            TextPointer position = editor.Document.ContentStart;

            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);
                    string textToSearch = caseSensitive ? textRun : textRun.ToLower();
                    string searchTextToUse = caseSensitive ? searchText : searchText.ToLower();

                    int index = textToSearch.IndexOf(searchTextToUse);
                    while (index >= 0)
                    {
                        TextPointer start = position.GetPositionAtOffset(index);
                        TextPointer end = start.GetPositionAtOffset(searchText.Length);

                        searchResults.Add(new SearchResult
                        {
                            Start = start,
                            End = end,
                            Text = searchText
                        });

                        index = textToSearch.IndexOf(searchTextToUse, index + 1);
                    }
                }

                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }

            return searchResults;
        }

        public void HighlightResults(RichTextBox editor)
        {
            foreach (var result in searchResults)
            {
                var range = new TextRange(result.Start, result.End);
                range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
            }
        }

        public void ClearHighlights(RichTextBox editor)
        {
            var range = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
        }

        public bool FindNext(RichTextBox editor)
        {
            if (searchResults.Count == 0)
                return false;

            currentResultIndex = (currentResultIndex + 1) % searchResults.Count;
            var result = searchResults[currentResultIndex];

            editor.Selection.Select(result.Start, result.End);
            editor.Focus();

            return true;
        }

        public bool FindPrevious(RichTextBox editor)
        {
            if (searchResults.Count == 0)
                return false;

            currentResultIndex = currentResultIndex <= 0 ? searchResults.Count - 1 : currentResultIndex - 1;
            var result = searchResults[currentResultIndex];

            editor.Selection.Select(result.Start, result.End);
            editor.Focus();

            return true;
        }

        public int ReplaceAll(RichTextBox editor, string searchText, string replaceText, bool caseSensitive = false)
        {
            FindAll(editor, searchText, caseSensitive);
            int count = searchResults.Count;

            // Remplacer en commençant par la fin pour ne pas décaler les positions
            for (int i = searchResults.Count - 1; i >= 0; i--)
            {
                var result = searchResults[i];
                var range = new TextRange(result.Start, result.End);
                range.Text = replaceText;
            }

            searchResults.Clear();
            return count;
        }

        public int GetResultCount()
        {
            return searchResults.Count;
        }

        public int GetCurrentResultIndex()
        {
            return currentResultIndex;
        }
    }
}