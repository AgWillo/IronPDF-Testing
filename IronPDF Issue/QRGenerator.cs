using System;
using System.Diagnostics;
using System.Drawing;
using QRCoder;
using ECCLevel = QRCoder.QRCodeGenerator.ECCLevel;

namespace IronPDF_Issue
{
    /// <summary>
    /// Encapsulation of methods for generating QR codes
    /// </summary>
    public class QRGenerator
    {
        /// <summary>
        /// Encodes the supplied text as a QR code rendered as an SVG image
        /// </summary>
        /// <param name="text">The text to be encoded</param>
        /// <param name="qrVersion">The QR version (1 - 40), which determines how many black/white boxes
        /// appear in each dimension of the QR code. Set to -1 to automatically choose the smallest version
        /// that can encode the supplied text.</param>
        public static string GenerateSvg(string text, int qrVersion = -1)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var generator = new QRCodeGenerator())
            using (QRCodeData data = generator.CreateQrCode(text, ECCLevel.H, requestedVersion: qrVersion))
            using (var svgQRCode = new SvgQRCode(data))
            {
                var qrSvg = svgQRCode.GetGraphic(new Size(200, 200), Color.Black, Color.White, true, SvgQRCode.SizingMode.ViewBoxAttribute);
                Console.WriteLine($"\n Generate QR code: {stopwatch.Elapsed.TotalSeconds:F3}");
                return qrSvg;
            }
        }
    }
}
