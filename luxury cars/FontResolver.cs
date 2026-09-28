using PdfSharp.Fonts;
using System.IO;
using System.Reflection;

namespace luxury_cars
{
    public class FontResolver : IFontResolver
    {
        public string DefaultFontName => "OpenSans";

        public byte[] GetFont(string faceName)
        {
            // Use a built-in font from PdfSharp
            return LoadFontData(faceName);
        }

        public FontResolverInfo ResolveTypeface(
            string familyName,
            bool isBold,
            bool isItalic)
        {
            // Map any font name to OpenSans
            if (isBold && isItalic)
                return new FontResolverInfo("OpenSans#bi");
            if (isBold)
                return new FontResolverInfo("OpenSans#b");
            if (isItalic)
                return new FontResolverInfo("OpenSans#i");
            return new FontResolverInfo("OpenSans#");
        }

        private static byte[] LoadFontData(string name)
        {
            // Use PdfSharp's built-in font
            var assembly = typeof(FontResolver).Assembly;
            using var stream = assembly
                .GetManifestResourceStream(name);
            if (stream == null)
                return GetFallbackFont();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        private static byte[] GetFallbackFont()
        {
            // Read a system font as fallback
            string[] fontPaths = {
                @"C:\Windows\Fonts\arial.ttf",
                @"C:\Windows\Fonts\calibri.ttf",
                @"C:\Windows\Fonts\segoeui.ttf",
                @"C:\Windows\Fonts\verdana.ttf"
            };

            foreach (var path in fontPaths)
            {
                if (File.Exists(path))
                    return File.ReadAllBytes(path);
            }
            return [];
        }
    }
}