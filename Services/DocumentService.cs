using System;
using System.IO;
using System.Linq;
using System.Windows;
using Markdig;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

// Alias WPF complets
using WpfRichTextBox = System.Windows.Controls.RichTextBox;
using WpfParagraph = System.Windows.Documents.Paragraph;
using WpfRun = System.Windows.Documents.Run;
using WpfBold = System.Windows.Documents.Bold;
using WpfItalic = System.Windows.Documents.Italic;
using WpfUnderline = System.Windows.Documents.Underline;
using WpfBlock = System.Windows.Documents.Block;
using WpfInline = System.Windows.Documents.Inline;
using WpfFlowDocument = System.Windows.Documents.FlowDocument;
using WpfTextRange = System.Windows.Documents.TextRange;
using WpfTable = System.Windows.Documents.Table;
using WpfTableRow = System.Windows.Documents.TableRow;
using WpfTableCell = System.Windows.Documents.TableCell;
using WpfTableRowGroup = System.Windows.Documents.TableRowGroup;
using WpfTableColumn = System.Windows.Documents.TableColumn;
using WpfInlineUIContainer = System.Windows.Documents.InlineUIContainer;
using WpfImage = System.Windows.Controls.Image;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfDataFormats = System.Windows.DataFormats;
using WpfBitmapImage = System.Windows.Media.Imaging.BitmapImage;

// Alias OpenXml
using DocxParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using DocxRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using DocxBold = DocumentFormat.OpenXml.Wordprocessing.Bold;
using DocxItalic = DocumentFormat.OpenXml.Wordprocessing.Italic;
using DocxUnderline = DocumentFormat.OpenXml.Wordprocessing.Underline;
using DocxTable = DocumentFormat.OpenXml.Wordprocessing.Table;
using DocxTableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using DocxTableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using DocxText = DocumentFormat.OpenXml.Wordprocessing.Text;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Aurea.Services
{
    public class DocumentService
    {
        public void LoadDocument(string filePath, WpfRichTextBox richTextBox)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".docx": LoadDocx(filePath, richTextBox); break;
                case ".txt": LoadText(filePath, richTextBox); break;
                case ".rtf": LoadRtf(filePath, richTextBox); break;
                case ".pdf": LoadPdf(filePath, richTextBox); break;
                case ".md": LoadMarkdown(filePath, richTextBox); break;
                case ".html":
                case ".htm": LoadHtml(filePath, richTextBox); break;
                default: throw new NotSupportedException($"Format non supporté : {extension}");
            }
        }

        public void SaveDocument(string filePath, WpfRichTextBox richTextBox)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".docx": SaveDocx(filePath, richTextBox); break;
                case ".txt": SaveText(filePath, richTextBox); break;
                case ".rtf": SaveRtf(filePath, richTextBox); break;
                case ".pdf": ExportToPDF(richTextBox, filePath); break;
                case ".md": SaveMarkdown(filePath, richTextBox); break;
                case ".html":
                case ".htm": SaveHtml(filePath, richTextBox); break;
                default: throw new NotSupportedException($"Format non supporté : {extension}");
            }
        }

        private void LoadDocx(string filePath, WpfRichTextBox richTextBox)
        {
            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;
                    var flowDoc = new WpfFlowDocument();

                    foreach (var element in body.Elements())
                    {
                        if (element is DocxParagraph para)
                        {
                            var wpfPara = new WpfParagraph();
                            foreach (var run in para.Elements<DocxRun>())
                            {
                                var wpfRun = new WpfRun(run.InnerText);
                                var runProps = run.RunProperties;
                                if (runProps != null)
                                {
                                    if (runProps.Bold != null) wpfRun.FontWeight = FontWeights.Bold;
                                    if (runProps.Italic != null) wpfRun.FontStyle = FontStyles.Italic;
                                    if (runProps.Underline != null) wpfRun.TextDecorations = TextDecorations.Underline;
                                }
                                wpfPara.Inlines.Add(wpfRun);
                            }
                            flowDoc.Blocks.Add(wpfPara);
                        }
                        else if (element is DocxTable table)
                        {
                            ProcessTable(table, flowDoc);
                        }
                    }
                    richTextBox.Document = flowDoc;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur DOCX : {ex.Message}");
            }
        }

        private void LoadText(string filePath, WpfRichTextBox richTextBox)
        {
            string text = File.ReadAllText(filePath);
            richTextBox.Document = new WpfFlowDocument(new WpfParagraph(new WpfRun(text)));
        }

        private void LoadRtf(string filePath, WpfRichTextBox richTextBox)
        {
            var range = new WpfTextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                range.Load(stream, WpfDataFormats.Rtf);
            }
        }

        private void LoadPdf(string filePath, WpfRichTextBox richTextBox)
        {
            string text = ExtractTextFromPDF(filePath);
            richTextBox.Document = new WpfFlowDocument(new WpfParagraph(new WpfRun(text)));
        }

        private void LoadMarkdown(string filePath, WpfRichTextBox richTextBox)
        {
            string markdown = File.ReadAllText(filePath);
            string html = Markdown.ToHtml(markdown);
            richTextBox.Document = new WpfFlowDocument(new WpfParagraph(new WpfRun(markdown)));
        }

        private void LoadHtml(string filePath, WpfRichTextBox richTextBox)
        {
            string html = File.ReadAllText(filePath);
            richTextBox.Document = new WpfFlowDocument(new WpfParagraph(new WpfRun(html)));
        }

        private void SaveDocx(string filePath, WpfRichTextBox richTextBox)
        {
            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(
                    filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    foreach (WpfBlock block in richTextBox.Document.Blocks)
                    {
                        if (block is WpfParagraph wpfPara)
                        {
                            var docxPara = new DocxParagraph();
                            foreach (WpfInline inline in wpfPara.Inlines)
                            {
                                if (inline is WpfRun wpfRun)
                                {
                                    var docxRun = new DocxRun();
                                    var runProps = new RunProperties();

                                    if (wpfRun.FontWeight == FontWeights.Bold)
                                        runProps.Append(new DocxBold());
                                    if (wpfRun.FontStyle == FontStyles.Italic)
                                        runProps.Append(new DocxItalic());
                                    if (wpfRun.TextDecorations == TextDecorations.Underline)
                                        runProps.Append(new DocxUnderline { Val = UnderlineValues.Single });

                                    docxRun.Append(runProps);
                                    docxRun.Append(new DocxText(wpfRun.Text));
                                    docxPara.Append(docxRun);
                                }
                            }
                            body.Append(docxPara);
                        }
                    }
                    mainPart.Document.Save();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur sauvegarde DOCX : {ex.Message}");
            }
        }

        private void SaveText(string filePath, WpfRichTextBox richTextBox)
        {
            File.WriteAllText(filePath, GetDocumentText(richTextBox));
        }

        private void SaveRtf(string filePath, WpfRichTextBox richTextBox)
        {
            var range = new WpfTextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                range.Save(stream, WpfDataFormats.Rtf);
            }
        }

        private void SaveMarkdown(string filePath, WpfRichTextBox richTextBox)
        {
            File.WriteAllText(filePath, GetDocumentText(richTextBox));
        }

        private void SaveHtml(string filePath, WpfRichTextBox richTextBox)
        {
            File.WriteAllText(filePath, ConvertToHtml(richTextBox));
        }

        public void ExportToPDF(WpfRichTextBox richTextBox, string filePath)
        {
            try
            {
                using (var writer = new PdfWriter(filePath))
                using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                using (var document = new iText.Layout.Document(pdf))
                {
                    document.Add(new iText.Layout.Element.Paragraph(GetDocumentText(richTextBox)));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur export PDF : {ex.Message}");
            }
        }

        public string ExtractTextFromPDF(string filePath)
        {
            try
            {
                using (var pdfReader = new PdfReader(filePath))
                using (var pdfDoc = new iText.Kernel.Pdf.PdfDocument(pdfReader))
                {
                    string text = "";
                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                    {
                        text += PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), new SimpleTextExtractionStrategy());
                    }
                    return text;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur extraction PDF : {ex.Message}");
            }
        }

        public void GenerateTableOfContents(WpfRichTextBox richTextBox)
        {
            var flowDoc = richTextBox.Document;
            var tocPara = new WpfParagraph(new WpfRun("TABLE DES MATIÈRES"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };

            flowDoc.Blocks.InsertBefore(flowDoc.Blocks.FirstBlock, tocPara);

            int headingNumber = 1;
            foreach (WpfBlock block in flowDoc.Blocks)
            {
                if (block is WpfParagraph para && para != tocPara)
                {
                    if (para.FontSize > 14 || para.FontWeight == FontWeights.Bold)
                    {
                        string headingText = new WpfTextRange(para.ContentStart, para.ContentEnd).Text;
                        var tocEntry = new WpfParagraph(new WpfRun($"{headingNumber}. {headingText}"))
                        {
                            Margin = new Thickness(20, 2, 0, 2)
                        };
                        flowDoc.Blocks.InsertAfter(tocPara, tocEntry);
                        headingNumber++;
                    }
                }
            }
        }

        public void InsertTable(WpfRichTextBox richTextBox, int rows, int columns)
        {
            var table = new WpfTable
            {
                CellSpacing = 0,
                BorderBrush = WpfBrushes.Black,
                BorderThickness = new Thickness(1)
            };

            for (int i = 0; i < columns; i++)
                table.Columns.Add(new WpfTableColumn());

            var rowGroup = new WpfTableRowGroup();
            for (int r = 0; r < rows; r++)
            {
                var row = new WpfTableRow();
                for (int c = 0; c < columns; c++)
                {
                    var cell = new WpfTableCell(new WpfParagraph(new WpfRun("")))
                    {
                        BorderBrush = WpfBrushes.Gray,
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(5)
                    };
                    row.Cells.Add(cell);
                }
                rowGroup.Rows.Add(row);
            }
            table.RowGroups.Add(rowGroup);
            richTextBox.Document.Blocks.Add(table);
        }

        public void InsertImage(WpfRichTextBox richTextBox, string imagePath)
        {
            try
            {
                var bitmap = new WpfBitmapImage(new Uri(imagePath));
                var image = new WpfImage
                {
                    Source = bitmap,
                    MaxWidth = 400,
                    MaxHeight = 400
                };
                var container = new WpfInlineUIContainer(image);
                var para = new WpfParagraph(container);
                richTextBox.Document.Blocks.Add(para);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur insertion image : {ex.Message}");
            }
        }

        public string GetDocumentText(WpfRichTextBox richTextBox)
        {
            var range = new WpfTextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            return range.Text;
        }

        private string ConvertToHtml(WpfRichTextBox richTextBox)
        {
            string html = "<!DOCTYPE html><html><head><meta charset='utf-8'></head><body>";
            foreach (WpfBlock block in richTextBox.Document.Blocks)
            {
                if (block is WpfParagraph para)
                {
                    html += "<p>";
                    foreach (WpfInline inline in para.Inlines)
                    {
                        if (inline is WpfRun run)
                        {
                            string text = System.Net.WebUtility.HtmlEncode(run.Text);
                            if (run.FontWeight == FontWeights.Bold) text = $"<strong>{text}</strong>";
                            if (run.FontStyle == FontStyles.Italic) text = $"<em>{text}</em>";
                            if (run.TextDecorations == TextDecorations.Underline) text = $"<u>{text}</u>";
                            html += text;
                        }
                    }
                    html += "</p>";
                }
            }
            html += "</body></html>";
            return html;
        }

        private void ProcessTable(DocxTable docxTable, WpfFlowDocument flowDoc)
        {
            var wpfTable = new WpfTable
            {
                CellSpacing = 0,
                BorderBrush = WpfBrushes.Black,
                BorderThickness = new Thickness(1)
            };

            var rowGroup = new WpfTableRowGroup();
            foreach (var docxRow in docxTable.Elements<DocxTableRow>())
            {
                var wpfRow = new WpfTableRow();
                foreach (var docxCell in docxRow.Elements<DocxTableCell>())
                {
                    var wpfCell = new WpfTableCell(new WpfParagraph(new WpfRun(docxCell.InnerText)))
                    {
                        BorderBrush = WpfBrushes.Gray,
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(5)
                    };
                    wpfRow.Cells.Add(wpfCell);
                }
                rowGroup.Rows.Add(wpfRow);
            }
            wpfTable.RowGroups.Add(rowGroup);
            flowDoc.Blocks.Add(wpfTable);
        }
    }
}
