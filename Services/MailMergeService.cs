using RichTextBox = System.Windows.Controls.RichTextBox;
using System;
using System.Data;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Aurea.Services
{
    public class MailMergeService
    {
        public void CreateMailMerge(RichTextBox editor, DataTable data)
        {
            if (data == null || data.Rows.Count == 0)
            {
                throw new ArgumentException("Aucune donnée fournie pour le publipostage");
            }

            var originalContent = GetDocumentContent(editor);
            var flowDoc = new FlowDocument();

            // Pour chaque ligne du DataTable, créer un document
            foreach (DataRow row in data.Rows)
            {
                string content = originalContent;

                // Remplacer les champs de fusion
                foreach (DataColumn column in data.Columns)
                {
                    string placeholder = $"<<{column.ColumnName}>>";
                    string value = row[column].ToString();
                    content = content.Replace(placeholder, value);
                }

                // Ajouter le contenu fusionné au document
                var paragraph = new Paragraph(new Run(content));
                paragraph.Margin = new System.Windows.Thickness(0, 0, 0, 20);
                flowDoc.Blocks.Add(paragraph);

                // Ajouter un saut de page (sauf pour le dernier)
                if (data.Rows.IndexOf(row) < data.Rows.Count - 1)
                {
                    var pageBreak = new Paragraph(new Run("\f")); // Form feed = saut de page
                    flowDoc.Blocks.Add(pageBreak);
                }
            }

            editor.Document = flowDoc;
        }

        public DataTable CreateSampleData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Nom");
            dt.Columns.Add("Prenom");
            dt.Columns.Add("Email");
            dt.Columns.Add("Adresse");

            dt.Rows.Add("Dupont", "Jean", "jean.dupont@email.com", "1 rue de Paris");
            dt.Rows.Add("Martin", "Marie", "marie.martin@email.com", "2 avenue Victor Hugo");
            dt.Rows.Add("Bernard", "Paul", "paul.bernard@email.com", "3 boulevard Haussmann");

            return dt;
        }

        private string GetDocumentContent(RichTextBox editor)
        {
            var range = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
            return range.Text;
        }

        public List<string> DetectMergeFields(RichTextBox editor)
        {
            var fields = new List<string>();
            string content = GetDocumentContent(editor);

            // Détecter les champs entre << et >>
            var matches = System.Text.RegularExpressions.Regex.Matches(content, @"<<(\w+)>>");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (!fields.Contains(match.Groups[1].Value))
                {
                    fields.Add(match.Groups[1].Value);
                }
            }

            return fields;
        }
    }
}
