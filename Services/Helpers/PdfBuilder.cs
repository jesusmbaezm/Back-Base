using System.IO.Compression;
using System.Text;

namespace Services.Helpers
{
    /// <summary>
    /// Minimal raw-PDF builder for DEMO letter-size (612 × 792 pt) documents.
    /// Coordinate origin is bottom-left; y increases upward.
    /// Fonts: 0 = Helvetica, 1 = Helvetica-Bold, 2 = Courier.
    /// Non-ASCII characters are octal-escaped automatically (WinAnsiEncoding).
    /// </summary>
    internal sealed class PdfBuilder
    {
        public const float PageW   = 612f;
        public const float PageH   = 792f;
        public const float MarginL = 40f;
        public const float MarginR = 572f;
        public const float PrintW  = 532f; // = MarginR - MarginL

        public const int Regular = 0;
        public const int Bold    = 1;
        public const int Mono    = 2;

        private readonly List<StringBuilder> _pages = [];
        private readonly List<ImageEntry> _images = [];
        private StringBuilder _cur = null!;
        private readonly float _pageW;
        private readonly float _pageH;
        private readonly float _marginL;

        /// <summary>
        /// Raw embedded image. <c>ColorSpace</c> is a PDF array/name expression
        /// (e.g. "/DeviceRGB" or "[/Indexed /DeviceRGB 255 &lt;hex&gt;]").
        /// <c>DecodeParms</c> is optional and used for PNG (predictor).
        /// <c>SMaskIndex</c> points to a sibling alpha-mask entry in <c>_images</c>, or -1.
        /// </summary>
        private sealed record ImageEntry(
            byte[] Bytes,
            int Width,
            int Height,
            int BitsPerComponent,
            string Filter,        // "/DCTDecode" or "/FlateDecode"
            string ColorSpace,
            string? DecodeParms,
            int SMaskIndex = -1);

        public PdfBuilder() : this(PageW, PageH, MarginL) { }

        public PdfBuilder(float pageW, float pageH, float marginL)
        {
            _pageW = pageW;
            _pageH = pageH;
            _marginL = marginL;
            NewPage();
        }

        public void NewPage()
        {
            _cur = new StringBuilder();
            _cur.Append("% DEMO page\n");
            _pages.Add(_cur);
        }

        // ── Graphics ─────────────────────────────────────────────────────

        public void FillRect(float x, float y, float w, float h, float gray = 0.85f)
            => _cur.Append($"q {gray:F2} g {x:F1} {y:F1} {w:F1} {h:F1} re f Q\n");

        public void StrokeRect(float x, float y, float w, float h, float lw = 0.5f, float stroke = 0f)
            => _cur.Append($"q {stroke:F2} G {lw:F2} w {x:F1} {y:F1} {w:F1} {h:F1} re S Q\n");

        public void FillStrokeRect(float x, float y, float w, float h, float gray = 0.85f, float lw = 0.5f, float stroke = 0f)
        {
            _cur.Append($"q {gray:F2} g {x:F1} {y:F1} {w:F1} {h:F1} re f Q\n");
            _cur.Append($"q {stroke:F2} G {lw:F2} w {x:F1} {y:F1} {w:F1} {h:F1} re S Q\n");
        }

        public void HLine(float x1, float x2, float y, float lw = 0.5f, float stroke = 0f)
            => _cur.Append($"q {stroke:F2} G {lw:F2} w {x1:F1} {y:F1} m {x2:F1} {y:F1} l S Q\n");

        public void VLine(float x, float y1, float y2, float lw = 0.5f, float stroke = 0f)
            => _cur.Append($"q {stroke:F2} G {lw:F2} w {x:F1} {y1:F1} m {x:F1} {y2:F1} l S Q\n");

        // ── Text ──────────────────────────────────────────────────────────

        public void Text(float x, float y, string t, int font = Regular, float size = 9f)
            => _cur.Append($"BT /F{font} {size:F1} Tf {x:F1} {y:F1} Td ({Esc(t)}) Tj ET\n");

        public void TextRight(float x, float y, string t, int font = Regular, float size = 9f)
        {
            var drawX = x - Approx(t, font, size);
            Text(Math.Max(_marginL, drawX), y, t, font, size);
        }

        public void TextCenter(float x1, float x2, float y, string t, int font = Regular, float size = 9f)
        {
            var tw  = Approx(t, font, size);
            var cx  = x1 + Math.Max(0f, (x2 - x1 - tw) / 2f);
            Text(cx, y, t, font, size);
        }

        /// <summary>Word-wrapped text block. Returns y below the last line drawn.</summary>
        public float Para(float x1, float x2, float y, string text, int font = Regular, float size = 8.5f, float lineH = 11f)
        {
            var maxW  = x2 - x1 - 2f;
            var words = text.Split(' ');
            var line  = new StringBuilder();
            var cy    = y;

            foreach (var word in words)
            {
                var probe = line.Length == 0 ? word : line + " " + word;
                if (Approx(probe, font, size) <= maxW)
                {
                    if (line.Length > 0) line.Append(' ');
                    line.Append(word);
                }
                else
                {
                    if (line.Length > 0) { Text(x1, cy, line.ToString(), font, size); cy -= lineH; line.Clear(); }
                    line.Append(word);
                }
            }
            if (line.Length > 0) { Text(x1, cy, line.ToString(), font, size); cy -= lineH; }
            return cy;
        }

        // ── Images ────────────────────────────────────────────────────────

        /// <summary>
        /// Registers a JPEG or PNG image (auto-detected by magic bytes) and
        /// returns its 0-based index, usable with DrawImage. Subsequent calls
        /// with byte-equal arrays return the same index (deduped by reference).
        /// </summary>
        public int RegisterImage(byte[] imageBytes)
        {
            if (imageBytes is null || imageBytes.Length < 8)
                throw new ArgumentException("Empty image data.", nameof(imageBytes));

            // PNG signature: 89 50 4E 47 0D 0A 1A 0A
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                return RegisterPngImage(imageBytes);

            // JPEG signature: FF D8 FF
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
                return RegisterJpegImage(imageBytes);

            throw new InvalidOperationException("Unsupported image format. Use JPEG or PNG.");
        }

        /// <summary>
        /// Registers a JPEG image and returns its 0-based index, usable with DrawImage.
        /// Subsequent calls with byte-equal arrays return the same index (deduped by reference).
        /// </summary>
        public int RegisterJpegImage(byte[] jpegBytes)
        {
            for (int i = 0; i < _images.Count; i++)
            {
                if (ReferenceEquals(_images[i].Bytes, jpegBytes)) return i;
            }

            var (w, h) = ReadJpegDimensions(jpegBytes);
            _images.Add(new ImageEntry(
                Bytes:            jpegBytes,
                Width:            w,
                Height:           h,
                BitsPerComponent: 8,
                Filter:           "/DCTDecode",
                ColorSpace:       "/DeviceRGB",
                DecodeParms:      null));
            return _images.Count - 1;
        }

        /// <summary>
        /// Registers a PNG image and returns its 0-based index, usable with DrawImage.
        /// Supports color types 0 (grayscale), 2 (RGB), 3 (indexed palette),
        /// 4 (grayscale + alpha) and 6 (RGB + alpha), 8 bits per component,
        /// non-interlaced. Alpha is exported as a PDF SMask.
        /// </summary>
        public int RegisterPngImage(byte[] pngBytes)
        {
            for (int i = 0; i < _images.Count; i++)
            {
                if (ReferenceEquals(_images[i].Bytes, pngBytes)) return i;
            }

            var (image, smask) = ReadPng(pngBytes);
            if (smask is null)
            {
                _images.Add(image);
                return _images.Count - 1;
            }

            int smaskIndex = _images.Count;
            _images.Add(smask);
            int imgIndex = _images.Count;
            _images.Add(image with { SMaskIndex = smaskIndex });
            return imgIndex;
        }

        /// <summary>Draws a previously registered image scaled into the given box.</summary>
        public void DrawImage(int imageIndex, float x, float y, float w, float h)
        {
            if (imageIndex < 0 || imageIndex >= _images.Count)
                throw new ArgumentOutOfRangeException(nameof(imageIndex));
            // PDF image transform: w 0 0 h x y cm  → scales 1×1 image space to w×h at (x,y)
            _cur.Append($"q {w:F2} 0 0 {h:F2} {x:F2} {y:F2} cm /Im{imageIndex} Do Q\n");
        }

        private static (ImageEntry Image, ImageEntry? SMask) ReadPng(byte[] data)
        {
            // PNG signature already validated by caller (8 bytes).
            if (data.Length < 8 + 25)
                throw new InvalidOperationException("Truncated PNG file.");

            int width = 0, height = 0, bitDepth = 0, colorType = -1, interlace = 0;
            byte[]? palette = null;
            using var idat = new MemoryStream();
            bool sawIhdr = false, sawIend = false;

            int p = 8;
            while (p + 8 <= data.Length)
            {
                int len = (data[p] << 24) | (data[p + 1] << 16) | (data[p + 2] << 8) | data[p + 3];
                p += 4;
                string type = $"{(char)data[p]}{(char)data[p + 1]}{(char)data[p + 2]}{(char)data[p + 3]}";
                p += 4;
                if (p + len + 4 > data.Length)
                    throw new InvalidOperationException("PNG chunk extends past end of file.");

                switch (type)
                {
                    case "IHDR":
                        if (len < 13) throw new InvalidOperationException("PNG IHDR too short.");
                        width     = (data[p] << 24) | (data[p + 1] << 16) | (data[p + 2] << 8) | data[p + 3];
                        height    = (data[p + 4] << 24) | (data[p + 5] << 16) | (data[p + 6] << 8) | data[p + 7];
                        bitDepth  = data[p + 8];
                        colorType = data[p + 9];
                        interlace = data[p + 12];
                        sawIhdr   = true;
                        break;
                    case "PLTE":
                        palette = new byte[len];
                        Array.Copy(data, p, palette, 0, len);
                        break;
                    case "IDAT":
                        idat.Write(data, p, len);
                        break;
                    case "IEND":
                        sawIend = true;
                        break;
                }

                p += len + 4; // skip data + CRC
                if (sawIend) break;
            }

            if (!sawIhdr) throw new InvalidOperationException("PNG missing IHDR chunk.");
            if (interlace != 0)
                throw new NotSupportedException("Interlaced PNGs are not supported. Save the PNG without interlace.");
            if (bitDepth != 8)
                throw new NotSupportedException($"PNG bit depth {bitDepth} not supported. Use 8 bits per channel.");

            var idatBytes = idat.ToArray();
            if (idatBytes.Length == 0)
                throw new InvalidOperationException("PNG missing IDAT data.");

            // Color types without alpha: pass IDAT through with FlateDecode + PNG predictor.
            if (colorType == 0 || colorType == 2 || colorType == 3)
            {
                int colors;
                string colorSpace;
                switch (colorType)
                {
                    case 0:
                        colors = 1; colorSpace = "/DeviceGray";
                        break;
                    case 2:
                        colors = 3; colorSpace = "/DeviceRGB";
                        break;
                    default: // 3
                        if (palette is null || palette.Length == 0 || palette.Length % 3 != 0)
                            throw new InvalidOperationException("Indexed PNG missing or invalid PLTE chunk.");
                        colors = 1;
                        int paletteEntries = palette.Length / 3;
                        var hex = new StringBuilder(palette.Length * 2 + 2);
                        hex.Append('<');
                        foreach (var b in palette) hex.Append(b.ToString("X2"));
                        hex.Append('>');
                        colorSpace = $"[/Indexed /DeviceRGB {paletteEntries - 1} {hex}]";
                        break;
                }

                var dp = $"/DecodeParms << /Predictor 15 /Colors {colors} /BitsPerComponent 8 /Columns {width} >>";
                return (new ImageEntry(
                    Bytes: idatBytes, Width: width, Height: height, BitsPerComponent: 8,
                    Filter: "/FlateDecode", ColorSpace: colorSpace, DecodeParms: dp), null);
            }

            // Color types 4 (gray + alpha) and 6 (RGBA): decode pixels, split alpha into SMask.
            if (colorType != 4 && colorType != 6)
                throw new NotSupportedException($"Unsupported PNG color type {colorType}.");

            int bpp = colorType == 6 ? 4 : 2;        // bytes per pixel
            int colorBpp = bpp - 1;                  // RGB or Gray bytes per pixel
            byte[] pixels = DecodePngFilteredScanlines(idatBytes, width, height, bpp);

            // Split into color plane and alpha plane.
            int pixelCount = width * height;
            byte[] colorPlane = new byte[pixelCount * colorBpp];
            byte[] alphaPlane = new byte[pixelCount];
            for (int i = 0; i < pixelCount; i++)
            {
                int src = i * bpp;
                int dst = i * colorBpp;
                for (int c = 0; c < colorBpp; c++)
                    colorPlane[dst + c] = pixels[src + c];
                alphaPlane[i] = pixels[src + colorBpp];
            }

            byte[] colorDeflated = ZlibCompress(colorPlane);
            byte[] alphaDeflated = ZlibCompress(alphaPlane);

            string mainColorSpace = colorType == 6 ? "/DeviceRGB" : "/DeviceGray";

            var imageEntry = new ImageEntry(
                Bytes: colorDeflated, Width: width, Height: height, BitsPerComponent: 8,
                Filter: "/FlateDecode", ColorSpace: mainColorSpace, DecodeParms: null);

            var smaskEntry = new ImageEntry(
                Bytes: alphaDeflated, Width: width, Height: height, BitsPerComponent: 8,
                Filter: "/FlateDecode", ColorSpace: "/DeviceGray", DecodeParms: null);

            return (imageEntry, smaskEntry);
        }

        private static byte[] DecodePngFilteredScanlines(byte[] idatBytes, int width, int height, int bpp)
        {
            int scanlineLen = width * bpp;
            byte[] result = new byte[scanlineLen * height];

            using var msIn = new MemoryStream(idatBytes);
            using var inflate = new ZLibStream(msIn, CompressionMode.Decompress);

            byte[] prevRow = new byte[scanlineLen];
            byte[] curRow  = new byte[scanlineLen];

            for (int y = 0; y < height; y++)
            {
                int filter = inflate.ReadByte();
                if (filter < 0) throw new InvalidOperationException("PNG IDAT ended before all scanlines were read.");

                int read = 0;
                while (read < scanlineLen)
                {
                    int n = inflate.Read(curRow, read, scanlineLen - read);
                    if (n <= 0) throw new InvalidOperationException("PNG IDAT ended mid-scanline.");
                    read += n;
                }

                // Defilter in place into curRow.
                switch (filter)
                {
                    case 0: // None
                        break;
                    case 1: // Sub: x = data + a (left)
                        for (int i = bpp; i < scanlineLen; i++)
                            curRow[i] = (byte)(curRow[i] + curRow[i - bpp]);
                        break;
                    case 2: // Up: x = data + b (above)
                        for (int i = 0; i < scanlineLen; i++)
                            curRow[i] = (byte)(curRow[i] + prevRow[i]);
                        break;
                    case 3: // Average: x = data + floor((a + b) / 2)
                        for (int i = 0; i < scanlineLen; i++)
                        {
                            int a = i < bpp ? 0 : curRow[i - bpp];
                            int b = prevRow[i];
                            curRow[i] = (byte)(curRow[i] + ((a + b) >> 1));
                        }
                        break;
                    case 4: // Paeth
                        for (int i = 0; i < scanlineLen; i++)
                        {
                            int a = i < bpp ? 0 : curRow[i - bpp];
                            int b = prevRow[i];
                            int c = i < bpp ? 0 : prevRow[i - bpp];
                            curRow[i] = (byte)(curRow[i] + PaethPredictor(a, b, c));
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown PNG filter type {filter} at row {y}.");
                }

                Buffer.BlockCopy(curRow, 0, result, y * scanlineLen, scanlineLen);
                (prevRow, curRow) = (curRow, prevRow);
            }

            return result;
        }

        private static int PaethPredictor(int a, int b, int c)
        {
            int p  = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);
            if (pa <= pb && pa <= pc) return a;
            if (pb <= pc) return b;
            return c;
        }

        private static byte[] ZlibCompress(byte[] data)
        {
            using var ms = new MemoryStream();
            using (var z = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
            {
                z.Write(data, 0, data.Length);
            }
            return ms.ToArray();
        }

        private static (int Width, int Height) ReadJpegDimensions(byte[] data)
        {
            // Scan for SOFn markers (0xFFC0..0xFFCF except DHT 0xFFC4, JPG 0xFFC8, DAC 0xFFCC)
            if (data.Length < 4 || data[0] != 0xFF || data[1] != 0xD8)
                throw new InvalidOperationException("Not a JPEG file.");

            int i = 2;
            while (i + 9 < data.Length)
            {
                if (data[i] != 0xFF) { i++; continue; }
                while (i < data.Length && data[i] == 0xFF) i++; // skip fill bytes
                if (i >= data.Length) break;
                byte marker = data[i++];

                // Markers without length payload
                if (marker == 0xD8 || marker == 0xD9 || (marker >= 0xD0 && marker <= 0xD7)) continue;
                if (i + 1 >= data.Length) break;
                int segLen = (data[i] << 8) | data[i + 1];

                bool isSof = marker >= 0xC0 && marker <= 0xCF
                             && marker != 0xC4 && marker != 0xC8 && marker != 0xCC;
                if (isSof)
                {
                    // Layout: length(2) precision(1) height(2) width(2) ...
                    int height = (data[i + 3] << 8) | data[i + 4];
                    int width  = (data[i + 5] << 8) | data[i + 6];
                    return (width, height);
                }
                i += segLen;
            }
            throw new InvalidOperationException("JPEG SOF marker not found.");
        }

        // ── Build ─────────────────────────────────────────────────────────

        public byte[] Build()
        {
            var sb     = new StringBuilder();
            var offset = 0;

            // All strings appended to sb via A() must be Latin-1 (1 byte/char).
            void A(string s) { sb.Append(s); offset += s.Length; }
            void AB(byte[] bytes)
            {
                foreach (var b in bytes) sb.Append((char)b);
                offset += bytes.Length;
            }

            int n         = _pages.Count;
            int m         = _images.Count;
            // Object layout: 1=Catalog, 2=Pages, 3-5=Fonts, 6..(5+m)=Images, then pages/content pairs.
            int firstPageObj = 6 + m;
            int totalObjs = 5 + m + n * 2;
            var offsets   = new int[totalObjs + 1]; // 1-indexed

            A("%PDF-1.4\n");

            // obj 1 – Catalog
            offsets[1] = offset;
            A("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

            // obj 2 – Pages tree
            offsets[2] = offset;
            var kids = string.Join(" ", Enumerable.Range(0, n).Select(i => $"{firstPageObj + i * 2} 0 R"));
            A($"2 0 obj\n<< /Type /Pages /Kids [ {kids} ] /Count {n} >>\nendobj\n");

            // obj 3 – Helvetica
            offsets[3] = offset;
            A("3 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n");

            // obj 4 – Helvetica-Bold
            offsets[4] = offset;
            A("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n");

            // obj 5 – Courier
            offsets[5] = offset;
            A("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Courier /Encoding /WinAnsiEncoding >>\nendobj\n");

            // obj 6..(5+m) – image XObjects (JPEG via DCTDecode, PNG via FlateDecode + optional SMask)
            for (int k = 0; k < m; k++)
            {
                int imgObj = 6 + k;
                var img    = _images[k];
                offsets[imgObj] = offset;
                A($"{imgObj} 0 obj\n");
                A($"<< /Type /XObject /Subtype /Image /Filter {img.Filter} ");
                A($"/Width {img.Width} /Height {img.Height} /BitsPerComponent {img.BitsPerComponent} /ColorSpace {img.ColorSpace} ");
                if (!string.IsNullOrEmpty(img.DecodeParms)) A(img.DecodeParms + " ");
                if (img.SMaskIndex >= 0) A($"/SMask {6 + img.SMaskIndex} 0 R ");
                A($"/Length {img.Bytes.Length} >>\nstream\n");
                AB(img.Bytes);
                A("\nendstream\nendobj\n");
            }

            // XObject resource entry shared by all pages
            string xobjEntry = m == 0
                ? ""
                : "/XObject << " + string.Join(" ", Enumerable.Range(0, m).Select(k => $"/Im{k} {6 + k} 0 R")) + " >> ";

            // Page + content stream pairs
            for (int i = 0; i < n; i++)
            {
                int pageId = firstPageObj + i * 2;
                int contId = pageId + 1;

                var content = _pages[i].ToString();
                if (!content.EndsWith("\n")) content += "\n";
                var contentLen = content.Length; // all ASCII → length == bytes

                offsets[pageId] = offset;
                A($"{pageId} 0 obj\n");
                A($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {_pageW:F0} {_pageH:F0}] ");
                A("/Resources << /Font << /F0 3 0 R /F1 4 0 R /F2 5 0 R >> ");
                A(xobjEntry);
                A(">> ");
                A($"/Contents {contId} 0 R >>\nendobj\n");

                offsets[contId] = offset;
                A($"{contId} 0 obj\n<< /Length {contentLen} >>\nstream\n");
                A(content);
                A("endstream\nendobj\n");
            }

            // xref table – each entry must be exactly 20 bytes: NNNNNNNNNN GGGGG n \n
            var xrefOffset = offset;
            A("xref\n");
            A($"0 {totalObjs + 1}\n");
            A("0000000000 65535 f \n");
            for (int i = 1; i <= totalObjs; i++)
                A($"{offsets[i]:D10} 00000 n \n");

            A("trailer\n");
            A($"<< /Size {totalObjs + 1} /Root 1 0 R >>\n");
            A("startxref\n");
            A($"{xrefOffset}\n");
            A("%%EOF");

            return Encoding.Latin1.GetBytes(sb.ToString());
        }

        // ── Private helpers ───────────────────────────────────────────────

        /// <summary>Approximate rendered width of text in points.</summary>
        internal static float Approx(string t, int font, float size) =>
            t.Length * size * (font == Mono ? 0.60f : font == Bold ? 0.58f : 0.52f);

        /// <summary>Escape a string for use inside a PDF string literal.</summary>
        internal static string Esc(string v)
        {
            var sb = new StringBuilder(v.Length + 8);
            foreach (var c in v)
            {
                switch (c)
                {
                    case '(' or ')' or '\\':
                        sb.Append('\\').Append(c);
                        break;
                    default:
                        if      (c < 32)   sb.Append(' ');
                        else if (c <= 127) sb.Append(c);
                        else if (c <= 255) sb.Append('\\').Append(Convert.ToString(c, 8).PadLeft(3, '0'));
                        // chars > 255 are omitted (outside Latin-1)
                        break;
                }
            }
            return sb.ToString();
        }
        public void TextStrikeThrough(float x, float y, string t, int font = Regular, float size = 9f)
        {
            // Dibujar el texto normal
            Text(x, y, t, font, size);

            if (!string.IsNullOrEmpty(t))
            {
                // Calcular el ancho del texto
                float textWidth = Approx(t, font, size);

                // Posición de la línea (a la mitad de la altura de la letra)
                float lineY = y + (size * 0.35f);

                // Dibujar la línea de tachado
                _cur.Append($"q 0 G 0.8 w {x:F1} {lineY:F1} m {x + textWidth:F1} {lineY:F1} l S Q\n");
            }
        }
    }
}
