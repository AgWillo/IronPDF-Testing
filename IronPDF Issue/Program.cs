using System;
using System.IO;
using System.Threading.Tasks;
using IronPdf;

namespace IronPDF_Issue
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            License.LicenseKey = "IRONPDF.CMMSDATAGROUP.IRO210803.9594.46112.308012-9BB148398B-ORZFRNUGHHWJH-RZRXBESXCHOO-W7GM7EREXSEH-DI5WYFQRCZ3H-CJIPSAT6HORD-E6DKJQ-LY5Q3IV5NZOREA-PROFESSIONAL.SAAS.OEM.5YR-K6D4ZP.RENEW.SUPPORT.02.AUG.2026";

            double height;
            double width;

            if (args.Length >= 5 && double.TryParse(args[1], out height) && double.TryParse(args[2], out width))
            {
                var controller = new Controller { PdfFileName = args[0] };
                float fontSize = float.Parse(args[3]);
                string[] names = args[4].Split(',');
                string result = await controller.CreateQrPdfFromAssetNamesAsync(names, height, width, fontSize);
                Console.Write(DateTime.Now.ToString("h:mm:ss tt  "));
                Console.WriteLine(result);
                return;
            }

            if (args.Length >= 2 && double.TryParse(args[0], out height) && double.TryParse(args[1], out width))
            {
                var fileName = args.Length > 2 ? args[2] : "test.pdf";
                await PrintTestPattern(height, width, fileName);
                Console.WriteLine($"Test PDF saved to {Path.GetFullPath(fileName)}.");
                return;
            }

            Console.WriteLine("\nCreateQrPDF output height width fontSize names\n");
            Console.WriteLine("  where height & width are in inches, fontSize in points, and");
            Console.WriteLine("  names is a comma-separated list of names to be QR-encoded.");
            Console.WriteLine("  If any name is 'cjkTest' (case-insensitive), it will be replaced\n"
                            + "  with a string of Chinese characters.");
        }

        private static async Task PrintTestPattern(double height, double width, string fileName)
        {
            var renderOptions = new ChromePdfRenderOptions
            {
                CustomCssUrl = "ironTest.css",
                MarginBottom = 2,
                MarginLeft = 2,
                MarginRight = 2,
                MarginTop = 2

            };
            renderOptions.SetCustomPaperSizeInInches(width, height);
            var renderer = new ChromePdfRenderer { RenderingOptions = renderOptions };
            var snippet = "<div class='absolute outerContainer'>Hello</div>";
            using (var pdfDoc = await renderer.RenderHtmlAsPdfAsync(snippet))
                pdfDoc.SaveAs(fileName);
        }
    }
}
