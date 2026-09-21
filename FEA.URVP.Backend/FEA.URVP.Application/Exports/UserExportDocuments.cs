using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace FEA.URVP.Application.Exports;

public sealed record UserExportRow(string UserName, string Email, string Role);

/// <summary>
/// Builds admin user exports without third-party document libraries.
/// </summary>
public static class UserExportDocuments
{
    public static byte[] ToExcel(IReadOnlyList<UserExportRow> rows)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(zip, "[Content_Types].xml", ContentTypesXml);
            WriteEntry(zip, "_rels/.rels", RelsXml);
            WriteEntry(zip, "xl/workbook.xml", WorkbookXml);
            WriteEntry(zip, "xl/_rels/workbook.xml.rels", WorkbookRelsXml);
            WriteEntry(zip, "xl/styles.xml", StylesXml);
            WriteEntry(zip, "xl/worksheets/sheet1.xml", BuildSheetXml(rows));
        }

        return stream.ToArray();
    }

    public static byte[] ToPdf(IReadOnlyList<UserExportRow> rows) => UserExportPdf.Build(rows);

    private static void WriteEntry(ZipArchive zip, string name, string xml)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(xml);
    }

    private static string BuildSheetXml(IReadOnlyList<UserExportRow> rows)
    {
        var xml = new StringBuilder(capacity: 1024 + (rows.Count * 160));
        xml.Append("""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
              <cols>
                <col min="1" max="1" width="22" customWidth="1"/>
                <col min="2" max="2" width="36" customWidth="1"/>
                <col min="3" max="3" width="14" customWidth="1"/>
              </cols>
              <sheetData>
            """);

        AppendRow(xml, 1, ["Username", "Email", "Role"], header: true);
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            AppendRow(xml, i + 2, [row.UserName, row.Email, row.Role], header: false);
        }

        xml.Append("</sheetData></worksheet>");
        return xml.ToString();
    }

    private static void AppendRow(StringBuilder xml, int rowNumber, IReadOnlyList<string> values, bool header)
    {
        xml.Append("<row r=\"").Append(rowNumber).Append("\">");
        for (var i = 0; i < values.Count; i++)
        {
            var cellRef = $"{(char)('A' + i)}{rowNumber}";
            xml.Append("<c r=\"").Append(cellRef).Append('"');
            if (header)
            {
                xml.Append(" s=\"1\"");
            }

            xml.Append(" t=\"inlineStr\"><is><t xml:space=\"preserve\">")
                .Append(XmlEscape(values[i]))
                .Append("</t></is></c>");
        }

        xml.Append("</row>");
    }

    private static string XmlEscape(string value)
    {
        var cleaned = Sanitize(value);
        return new StringBuilder(cleaned.Length)
            .Append(cleaned)
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .ToString();
    }

    internal static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        var buffer = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (char.IsControl(ch) && ch is not '\t')
            {
                continue;
            }

            buffer.Append(ch);
        }

        return buffer.ToString();
    }

    private const string ContentTypesXml = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
          <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
          <Default Extension="xml" ContentType="application/xml"/>
          <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
          <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
          <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
        </Types>
        """;

    private const string RelsXml = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
        </Relationships>
        """;

    private const string WorkbookXml = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
          <sheets>
            <sheet name="Users" sheetId="1" r:id="rId1"/>
          </sheets>
        </workbook>
        """;

    private const string WorkbookRelsXml = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
          <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
        </Relationships>
        """;

    private const string StylesXml = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
          <fonts count="2">
            <font><sz val="11"/><name val="Calibri"/></font>
            <font><b/><sz val="11"/><name val="Calibri"/></font>
          </fonts>
          <fills count="1"><fill><patternFill patternType="none"/></fill></fills>
          <borders count="1"><border/></borders>
          <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
          <cellXfs count="2">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
            <xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/>
          </cellXfs>
        </styleSheet>
        """;
}

internal static class UserExportPdf
{
    private const float PageWidth = 612f;
    private const float PageHeight = 792f;
    private const float Margin = 40f;
    private const float RowHeight = 18f;
    private const float HeaderBand = 22f;
    private const float FontSize = 9f;
    private const float TitleSize = 14f;

    public static byte[] Build(IReadOnlyList<UserExportRow> rows)
    {
        var generated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);
        var pages = Paginate(rows);
        var objects = new List<byte[]>();

        objects.Add(Obj("<< /Type /Catalog /Pages 2 0 R >>"));

        var pageObjectNumbers = new int[pages.Count];
        var firstPageObject = 4;
        for (var i = 0; i < pages.Count; i++)
        {
            pageObjectNumbers[i] = firstPageObject + (i * 2);
        }

        var kids = string.Join(" ", pageObjectNumbers.Select(n => $"{n} 0 R"));
        objects.Add(Obj($"<< /Type /Pages /Count {pages.Count} /Kids [{kids}] >>"));
        objects.Add(Obj("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"));

        for (var i = 0; i < pages.Count; i++)
        {
            var pageObject = pageObjectNumbers[i];
            var contentObject = pageObject + 1;
            var content = BuildPageStream(pages[i], i + 1, pages.Count, generated);
            objects.Add(Obj(
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth.ToString(CultureInfo.InvariantCulture)} {PageHeight.ToString(CultureInfo.InvariantCulture)}] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentObject} 0 R >>"));
            objects.Add(StreamObj(content));
        }

        return Assemble(objects);
    }

    private sealed record PdfPage(IReadOnlyList<UserExportRow> Rows);

    private static List<PdfPage> Paginate(IReadOnlyList<UserExportRow> rows)
    {
        var pages = new List<PdfPage>();
        var y = StartY();
        var current = new List<UserExportRow>();
        var firstOnPage = true;

        void Flush()
        {
            pages.Add(new PdfPage(current));
            current = [];
            y = StartY();
            firstOnPage = true;
        }

        foreach (var row in rows)
        {
            var needed = firstOnPage ? HeaderBand + RowHeight : RowHeight;
            if (y - needed < Margin + 28)
            {
                Flush();
            }

            if (firstOnPage)
            {
                y -= HeaderBand;
                firstOnPage = false;
            }

            current.Add(row);
            y -= RowHeight;
        }

        if (current.Count > 0 || pages.Count == 0)
        {
            pages.Add(new PdfPage(current));
        }

        return pages;
    }

    private static float StartY() => PageHeight - Margin - 36f;

    private static byte[] BuildPageStream(
        PdfPage page,
        int pageNumber,
        int pageCount,
        string generated)
    {
        var sb = new StringBuilder();
        sb.Append("0.2 w\n");

        WriteText(sb, TitleSize, Margin, PageHeight - Margin - 8, "URVP users");
        WriteText(sb, 8, Margin, PageHeight - Margin - 22, $"Username, email, and role · {generated}");

        var columns = new (float X, float Width, string Title)[]
        {
            (Margin, 130, "Username"),
            (Margin + 130, 290, "Email"),
            (Margin + 420, 112, "Role"),
        };
        var tableRight = columns[^1].X + columns[^1].Width;
        var y = StartY();

        DrawRowBackground(sb, y, HeaderBand, header: true);
        DrawHorizontal(sb, y, tableRight);
        DrawHorizontal(sb, y - HeaderBand, tableRight);
        foreach (var col in columns)
        {
            DrawVertical(sb, col.X, y, y - HeaderBand);
        }

        DrawVertical(sb, tableRight, y, y - HeaderBand);

        for (var i = 0; i < columns.Length; i++)
        {
            WriteText(sb, FontSize, columns[i].X + 4, y - 14, columns[i].Title);
        }

        y -= HeaderBand;

        foreach (var row in page.Rows)
        {
            DrawHorizontal(sb, y - RowHeight, tableRight);
            foreach (var col in columns)
            {
                DrawVertical(sb, col.X, y, y - RowHeight);
            }

            DrawVertical(sb, tableRight, y, y - RowHeight);
            WriteText(sb, FontSize, columns[0].X + 4, y - 12, Fit(row.UserName, columns[0].Width - 8));
            WriteText(sb, FontSize, columns[1].X + 4, y - 12, Fit(row.Email, columns[1].Width - 8));
            WriteText(sb, FontSize, columns[2].X + 4, y - 12, Fit(row.Role, columns[2].Width - 8));
            y -= RowHeight;
        }

        WriteText(
            sb,
            8,
            Margin,
            Margin - 8,
            $"Page {pageNumber} of {pageCount} · {page.Rows.Count} row{(page.Rows.Count == 1 ? "" : "s")} on this page");

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    private static void DrawRowBackground(StringBuilder sb, float top, float height, bool header)
    {
        if (!header)
        {
            return;
        }

        sb.Append("0.92 g ")
            .Append(Margin.ToString(CultureInfo.InvariantCulture)).Append(' ')
            .Append((top - height).ToString(CultureInfo.InvariantCulture)).Append(' ')
            .Append((PageWidth - Margin - Margin).ToString(CultureInfo.InvariantCulture)).Append(' ')
            .Append(height.ToString(CultureInfo.InvariantCulture))
            .Append(" re f 0 g\n");
    }

    private static void DrawHorizontal(StringBuilder sb, float y, float right)
    {
        sb.Append(Margin.ToString(CultureInfo.InvariantCulture)).Append(' ')
            .Append(y.ToString(CultureInfo.InvariantCulture)).Append(" m ")
            .Append(right.ToString(CultureInfo.InvariantCulture)).Append(' ')
            .Append(y.ToString(CultureInfo.InvariantCulture)).Append(" l S\n");
    }

    private static void DrawVertical(StringBuilder sb, float x, float top, float bottom)
    {
        sb.Append(x.ToString(CultureInfo.InvariantCulture)).Append(' ')
            .Append(top.ToString(CultureInfo.InvariantCulture)).Append(" m ")
            .Append(x.ToString(CultureInfo.InvariantCulture)).Append(' ')
            .Append(bottom.ToString(CultureInfo.InvariantCulture)).Append(" l S\n");
    }

    private static void WriteText(StringBuilder sb, float size, float x, float y, string text)
    {
        sb.Append("BT /F1 ").Append(size.ToString(CultureInfo.InvariantCulture)).Append(" Tf ")
            .Append(x.ToString(CultureInfo.InvariantCulture)).Append(' ')
            .Append(y.ToString(CultureInfo.InvariantCulture)).Append(" Td (")
            .Append(PdfEscape(text))
            .Append(") Tj ET\n");
    }

    private static string Fit(string value, float width)
    {
        var sanitized = UserExportDocuments.Sanitize(value);
        var maxChars = Math.Max(1, (int)Math.Floor(width / (FontSize * 0.5f)));
        return sanitized.Length <= maxChars ? sanitized : sanitized[..Math.Max(0, maxChars - 3)] + "...";
    }

    private static string PdfEscape(string value)
    {
        var sanitized = UserExportDocuments.Sanitize(value);
        var sb = new StringBuilder(sanitized.Length);
        foreach (var ch in sanitized)
        {
            if (ch is '(' or ')' or '\\')
            {
                sb.Append('\\').Append(ch);
                continue;
            }

            if (ch > 127)
            {
                sb.Append('?');
                continue;
            }

            sb.Append(ch);
        }

        return sb.ToString();
    }

    private static byte[] Obj(string body) => Encoding.ASCII.GetBytes(body + "\n");

    private static byte[] StreamObj(byte[] content)
    {
        var header = Encoding.ASCII.GetBytes($"<< /Length {content.Length} >>\nstream\n");
        var footer = Encoding.ASCII.GetBytes("\nendstream\n");
        var result = new byte[header.Length + content.Length + footer.Length];
        Buffer.BlockCopy(header, 0, result, 0, header.Length);
        Buffer.BlockCopy(content, 0, result, header.Length, content.Length);
        Buffer.BlockCopy(footer, 0, result, header.Length + content.Length, footer.Length);
        return result;
    }

    private static byte[] Assemble(List<byte[]> bodies)
    {
        using var stream = new MemoryStream();
        stream.Write("%PDF-1.4\n"u8);

        var offsets = new long[bodies.Count];
        for (var i = 0; i < bodies.Count; i++)
        {
            offsets[i] = stream.Position;
            var number = Encoding.ASCII.GetBytes($"{i + 1} 0 obj\n");
            stream.Write(number);
            stream.Write(bodies[i]);
            stream.Write("endobj\n"u8);
        }

        var xref = stream.Position;
        var xrefBuilder = new StringBuilder();
        xrefBuilder.Append("xref\n0 ").Append(bodies.Count + 1).Append('\n');
        xrefBuilder.Append("0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            xrefBuilder.Append(offset.ToString("0000000000", CultureInfo.InvariantCulture))
                .Append(" 00000 n \n");
        }

        xrefBuilder.Append("trailer\n<< /Size ").Append(bodies.Count + 1)
            .Append(" /Root 1 0 R >>\nstartxref\n")
            .Append(xref)
            .Append("\n%%EOF\n");

        stream.Write(Encoding.ASCII.GetBytes(xrefBuilder.ToString()));
        return stream.ToArray();
    }
}
