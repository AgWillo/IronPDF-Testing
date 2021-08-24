using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronPdf;
using IronPdf.Rendering;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace IronPDF_Issue
{
    public class Controller
    {
        public string PdfFileName { get; set; }

        private string GetHtmlQrSnippet(string name, double height, double width, float fontSize)
        {
            // Many CJK characters are words unto themselves; the zero-width word-joiner (aka no-break) character
            // prevents line breaks between words.
            const string wordJoiner = "\u2060";
            //const string wordJoiner = "&NoBreak;";
            //const string wordJoiner = "&#8288;";
            //const string wordJoiner = "";
            string noBreakName = string.Join(wordJoiner, name.TextElements());

            // Label margin; matches @page-margin variable in qr-label.less
            const double pageMargin = .125;

            // The asset name's line-height is sized at 1.05 in CSS to ensure that underscores and descenders aren't chopped off.
            // The calculation here ensures that the height of the QR region is calculated with that line height in mind.
            double qrHeight = height - (2 * pageMargin) - (fontSize * 1.05 / 72); // fontSize is in points; dividing by 72 converts to inches.

            var html =  $@"<meta charset='utf-8'/>
                      <div class='qr-label col-md-10'>
                        <div id='qrcontainer' class='qr-container' style='font-size:{fontSize}pt; height:{height}in; width:{width}in;'>
                          <div id='qrwrapper' class='qr-wrapper' style='height: {qrHeight}in;'>
                            {QRGenerator.GenerateSvg(name)}
                          </div>
                          <div class='asset-name'>
                            <div>{noBreakName}</div>
                          </div>
                        </div>
                    </div>".Replace("\"", "'");
            return html;
        }

        /// <summary>
        /// Creates a PDF with QR codes for the supplied names
        /// </summary>
        /// <param name="assetNames">The asset names for which QR-code labels will be created</param>
        /// <param name="height">The PDF page height in inches</param>
        /// <param name="width">The PDF page width in inches</param>
        /// <param name="fontSize">Size of the font in which the asset name will be displayed in points</param>
        public async Task<string> CreateQrPdfFromAssetNamesAsync(IEnumerable<string> assetNames, double height, double width, float fontSize)
        {
            PdfDocument qrLabelDoc = null;
            string statusMessage = string.Empty;
            int labelCount = 0;
            try
            {
                var renderOptions = new ChromePdfRenderOptions
                {
                    CustomCssUrl = "qr-label.css",
                    MarginBottom = 0,
                    MarginLeft = 0,
                    MarginRight = 0,
                    MarginTop = 0,
                    CssMediaType = PdfCssMediaType.Print
                };
                renderOptions.SetCustomPaperSizeInInches(width, height);

                var stopwatch = new Stopwatch();
                var snippets = assetNames.Select(name =>
                {
                    if (name.Equals("cjkTest", StringComparison.OrdinalIgnoreCase)) name = "获得宽恕总是比授权更容易永远不要把无能充分解释的恶意归咎于恶意。";
                    // Inserting byte-order mark before HTML snippet as temporary workaround for IronPDF UTF-16 issue
                    return new { name, html = GetHtmlQrSnippet(name, height, width, fontSize) };
                    //return new { name, html = GetHtmlQrSnippet(name, height, width, fontSize) };
                });

                Action<PdfDocument> truncateAfterFirstPage = doc =>
                {
                    //if (doc.PageCount > 1) doc.RemovePages(1, doc.PageCount - 1);
                };
                var renderer = new ChromePdfRenderer { RenderingOptions = renderOptions };
                TimeSpan elapsedSoFar = TimeSpan.Zero;
                stopwatch.Start();
                foreach (var snippet in snippets)
                {
                    try
                    {
                        if (qrLabelDoc == null)
                        {
                            qrLabelDoc = await renderer.RenderHtmlAsPdfAsync(snippet.html);
                            truncateAfterFirstPage(qrLabelDoc);
                            elapsedSoFar = stopwatch.Elapsed;
                            Console.WriteLine($"First page render: {elapsedSoFar.TotalSeconds:F3}");
                        }
                        else
                            using (PdfDocument oneLabel = await renderer.RenderHtmlAsPdfAsync(snippet.html))
                            {
                                TimeSpan nextPageRender = stopwatch.Elapsed;
                                Console.WriteLine($" Next page render: {(nextPageRender - elapsedSoFar).TotalSeconds:F3}");
                                truncateAfterFirstPage(oneLabel);
                                qrLabelDoc.AppendPdf(oneLabel);
                                TimeSpan nextPageAppend = stopwatch.Elapsed;
                                Console.WriteLine($" Next page append: {(nextPageAppend - nextPageRender).TotalSeconds:F3}");
                                elapsedSoFar = nextPageAppend;
                            }

                        labelCount++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"PDF creation failed for {snippet.name}. [{e.Message}]");
                    }
                }
                stopwatch.Stop();
                Console.WriteLine($"            Total: {stopwatch.Elapsed.TotalSeconds:F2} seconds");

                qrLabelDoc?.SaveAs(PdfFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unknown error. [{e.Message}]");
            }
            finally
            {
                qrLabelDoc?.Dispose();
            }

            statusMessage = $"Saved {labelCount} label(s) to {Path.GetFullPath(PdfFileName)}.";

            return statusMessage;
        }
    }
}
