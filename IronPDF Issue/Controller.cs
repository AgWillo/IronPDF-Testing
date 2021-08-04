using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IronPdf;

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
            string noBreakName = string.Join(wordJoiner, name.TextElements());
            double qrHeight = height - (fontSize * 1.05);

            return $@"
            <div class='qr-label'>
                <div id='qrcontainer' class='qr-container' style='font-size:{fontSize}pt; height:{height}in; width:{width}in;'>
                    <div id='qrwrapper' class='qr-wrapper' style='height: {qrHeight}pt;'>
                        {QRGenerator.GenerateSvg(name)}
                    </div>
                    <div class='asset-name'>
                        <div>{noBreakName}</div>
                    </div>
                </div>
            </div>";
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
                    MarginTop = 0
                };
                renderOptions.SetCustomPaperSizeInInches(width, height);

                var stopwatch = new Stopwatch();
                TimeSpan firstPageTime = default;
                TimeSpan secondPageRender = default;
                TimeSpan secondPageAppend = default;
                var snippets = assetNames.Select(name =>
                {
                    if (name.Equals("cjkTest", StringComparison.OrdinalIgnoreCase)) name = "获得宽恕总是比授权更容易永远不要把无能充分解释的恶意归咎于恶意。";
                    // Inserting byte-order mark before HTML snippet as temporary workaround for IronPDF UTF-16 issue
                    return new { name, html = '\ufeff' + GetHtmlQrSnippet(name, height, width, fontSize) };
                });

                var renderer = new ChromePdfRenderer { RenderingOptions = renderOptions };
                stopwatch.Start();
                foreach (var snippet in snippets)
                {
                    try
                    {
                        if (qrLabelDoc == null)
                        {
                            qrLabelDoc = await renderer.RenderHtmlAsPdfAsync(snippet.html);
                            if (qrLabelDoc.PageCount > 1) qrLabelDoc.RemovePages(1, qrLabelDoc.PageCount - 1);
                            firstPageTime = stopwatch.Elapsed;
                        }
                        else
                            using (PdfDocument oneLabel = await renderer.RenderHtmlAsPdfAsync(snippet.html))
                            {
                                secondPageRender = stopwatch.Elapsed;
                                if (oneLabel.PageCount > 1) oneLabel.RemovePages(1, oneLabel.PageCount - 1);
                                qrLabelDoc.AppendPdf(oneLabel);
                                secondPageAppend = stopwatch.Elapsed;
                            }

                        labelCount++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"PDF creation failed for {snippet.name}. [{e.Message}]");
                    }
                }
                stopwatch.Stop();
                Console.WriteLine($" First page render: {firstPageTime.TotalSeconds:F2}");
                Console.WriteLine($"Second page render: {(secondPageRender - firstPageTime).TotalSeconds:F2}");
                Console.WriteLine($"Second page append: {(secondPageAppend - secondPageRender).TotalSeconds:F2}");

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

            statusMessage = $"Created {labelCount} label(s).";

            return statusMessage;
        }
    }
}
