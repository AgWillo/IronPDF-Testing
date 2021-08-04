using System;
using System.Threading.Tasks;
using IronPdf;

namespace IronPDF_Issue
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IronPdf.Logging.Logger.LogFilePath = "IronPDF.log";
            License.LicenseKey = "IRONPDF.CMMSDATAGROUP.IRO210803.9594.46112.308012-9BB148398B-ORZFRNUGHHWJH-RZRXBESXCHOO-W7GM7EREXSEH-DI5WYFQRCZ3H-CJIPSAT6HORD-E6DKJQ-LY5Q3IV5NZOREA-PROFESSIONAL.SAAS.OEM.5YR-K6D4ZP.RENEW.SUPPORT.02.AUG.2026";

            if (args.Length == 0)
            {
                Console.WriteLine("CreateQrPDF output height width fontSize names");
                Console.WriteLine("  where height & width are in inches, fontSize in points,");
                Console.WriteLine("  and names is a comma-separated list of names to be QR-encoded.");
                Console.WriteLine("  If any name is 'cjkTest' (case-insensitive), it will be replaced\n"
                                + "  with a string of Chinese characters.");
                return;
            }

            var controller = new Controller { PdfFileName = args[0] };
            double height = double.Parse(args[1]);
            double width = double.Parse(args[2]);
            float fontSize = float.Parse(args[3]);
            string[] names = args[4].Split(',');
            string result = await controller.CreateQrPdfFromAssetNamesAsync(names, height, width, fontSize);
            Console.WriteLine(result);
        }
    }
}
