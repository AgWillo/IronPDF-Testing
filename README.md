# IronPDF-UTF-16 Testing

This repository contains a Visual Studio solution that demonstrates the UTF-16 problem with IronPDF. Note that it relies on two NuGet packages: IronPdf.EAP and QRCoder, for generating QR codes. It's a command-line app; after building it, open a command prompt at …\bin\Debug\netcoreapp3.1 and enter the following:

`CreateQrPDF qr.pdf 3 3 12 fred,cjktest`

(Entering just `CreateQrPDF` will display a summary of the command-line parameters.) This will generate a two-page PDF in the same folder as the executable. The PDF will display the Chinese text correctly (and truncated, which is by design). To demonstrate the UTF-16 issue, modify the line of code in Controller.cs that inserts the `<meta>` tag (just search for `<meta>`) and remove the concatenation. After rebuilding and running the above command with the modified code, you should see a PDF with the Chinese text mangled.

Per Iron Software, the problem arises because Chromium's encoding-detection logic examines "only" the first 65K chracters. The large SVG in the test foils that logic, necessitating an explicit declaration of the encoding, provided here by a `<meta charset ... />` tag.
