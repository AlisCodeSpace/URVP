using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace FEA.URVP.Application.Exports;

/// <summary>
/// Builds a single-sheet .xlsx file without third-party document libraries.
/// </summary>
public static class ExcelWorkbook
{
    public const string MimeType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static byte[] Build(
        string sheetName,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<string>> rows,
        IReadOnlyList<double>? columnWidths = null)
    {
        if (headers.Count == 0)
        {
            throw new ArgumentException("Export requires at least one column.", nameof(headers));
        }

        var safeName = NormalizeSheetName(sheetName);
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(zip, "[Content_Types].xml", ContentTypesXml);
            WriteEntry(zip, "_rels/.rels", RelsXml);
            WriteEntry(zip, "xl/workbook.xml", WorkbookXml(safeName));
            WriteEntry(zip, "xl/_rels/workbook.xml.rels", WorkbookRelsXml);
            WriteEntry(zip, "xl/styles.xml", StylesXml);
            WriteEntry(zip, "xl/worksheets/sheet1.xml", BuildSheetXml(headers, rows, columnWidths));
        }

        return stream.ToArray();
    }

    private static void WriteEntry(ZipArchive zip, string name, string xml)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(xml);
    }

    private static string BuildSheetXml(
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<string>> rows,
        IReadOnlyList<double>? columnWidths)
    {
        var xml = new StringBuilder(capacity: 1024 + ((rows.Count + 1) * headers.Count * 48));
        xml.Append("""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
              <cols>
            """);

        for (var i = 0; i < headers.Count; i++)
        {
            var width = columnWidths is not null && i < columnWidths.Count
                ? columnWidths[i]
                : 20d;
            xml.Append("<col min=\"").Append(i + 1)
                .Append("\" max=\"").Append(i + 1)
                .Append("\" width=\"").Append(width.ToString(CultureInfo.InvariantCulture))
                .Append("\" customWidth=\"1\"/>");
        }

        xml.Append("</cols><sheetData>");
        AppendRow(xml, 1, headers, header: true, columnCount: headers.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            AppendRow(xml, i + 2, rows[i], header: false, columnCount: headers.Count);
        }

        xml.Append("</sheetData></worksheet>");
        return xml.ToString();
    }

    private static void AppendRow(
        StringBuilder xml,
        int rowNumber,
        IReadOnlyList<string> values,
        bool header,
        int columnCount)
    {
        xml.Append("<row r=\"").Append(rowNumber).Append("\">");
        for (var i = 0; i < columnCount; i++)
        {
            var value = i < values.Count ? values[i] : "";
            xml.Append("<c r=\"").Append(ColumnName(i)).Append(rowNumber).Append('"');
            if (header)
            {
                xml.Append(" s=\"1\"");
            }

            xml.Append(" t=\"inlineStr\"><is><t xml:space=\"preserve\">")
                .Append(XmlEscape(value))
                .Append("</t></is></c>");
        }

        xml.Append("</row>");
    }

    internal static string ColumnName(int zeroBasedIndex)
    {
        var dividend = zeroBasedIndex + 1;
        var name = "";
        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            name = (char)('A' + modulo) + name;
            dividend = (dividend - 1) / 26;
        }

        return name;
    }

    private static string XmlEscape(string? value)
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

    private static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        var buffer = new StringBuilder(value.Length);
        var pendingSpace = false;
        foreach (var ch in value)
        {
            if (ch is '\r' or '\n')
            {
                pendingSpace = buffer.Length > 0;
                continue;
            }

            if (char.IsControl(ch) && ch is not '\t')
            {
                continue;
            }

            if (pendingSpace)
            {
                buffer.Append(' ');
                pendingSpace = false;
            }

            buffer.Append(ch);
        }

        return buffer.ToString();
    }

    private static string NormalizeSheetName(string name)
    {
        var buffer = new StringBuilder(name.Length);
        foreach (var ch in name)
        {
            if (ch is ':' or '\\' or '/' or '?' or '*' or '[' or ']')
            {
                continue;
            }

            buffer.Append(ch);
        }

        var cleaned = buffer.ToString().Trim();
        if (cleaned.Length == 0)
        {
            cleaned = "Sheet1";
        }

        return cleaned.Length > 31 ? cleaned[..31] : cleaned;
    }

    private static string WorkbookXml(string sheetName) =>
        $"""
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
          <sheets>
            <sheet name="{XmlEscape(sheetName)}" sheetId="1" r:id="rId1"/>
          </sheets>
        </workbook>
        """;

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
          <fills count="2">
            <fill><patternFill patternType="none"/></fill>
            <fill><patternFill patternType="gray125"/></fill>
          </fills>
          <borders count="1">
            <border><left/><right/><top/><bottom/><diagonal/></border>
          </borders>
          <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
          <cellXfs count="2">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
            <xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/>
          </cellXfs>
          <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
        </styleSheet>
        """;
}
