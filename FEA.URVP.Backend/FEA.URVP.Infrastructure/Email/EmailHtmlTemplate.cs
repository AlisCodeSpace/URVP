using System.Net;

namespace FEA.URVP.Infrastructure.Email;

internal static class EmailHtmlTemplate
{
    public static bool LooksLikeHtml(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return false;
        }

        var trimmed = body.TrimStart();
        return trimmed.StartsWith('<') && trimmed.Contains('>', StringComparison.Ordinal);
    }

    public static string Wrap(
        string title,
        string body,
        string? actionUrl,
        string? actionText)
    {
        var encodedTitle = WebUtility.HtmlEncode(title);
        var encodedBody = WebUtility.HtmlEncode(body).Replace("\n", "<br />", StringComparison.Ordinal);
        var cta = BuildCta(actionUrl, actionText);

        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>{encodedTitle}</title>
            </head>
            <body style="margin:0;padding:0;background:#f4f1ec;font-family:Georgia,'Times New Roman',serif;color:#2b2118;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f4f1ec;padding:24px 0;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="600" cellspacing="0" cellpadding="0" style="max-width:600px;width:100%;background:#ffffff;border:1px solid #e4d9c8;">
                      <tr>
                        <td style="background:#8c1d40;color:#ffffff;padding:20px 28px;font-size:18px;letter-spacing:0.04em;">
                          FEA Undergraduate Research Volunteer Program
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:28px;">
                          <h1 style="margin:0 0 16px;font-size:22px;color:#8c1d40;">{encodedTitle}</h1>
                          <p style="margin:0 0 24px;line-height:1.6;font-size:16px;">{encodedBody}</p>
                          {cta}
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:16px 28px 24px;color:#6b5c4d;font-size:12px;border-top:1px solid #e4d9c8;">
                          This is an automated message. Please do not reply.
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string BuildCta(string? actionUrl, string? actionText)
    {
        if (string.IsNullOrWhiteSpace(actionUrl) || string.IsNullOrWhiteSpace(actionText))
        {
            return string.Empty;
        }

        return $"""
            <a href="{WebUtility.HtmlEncode(actionUrl)}"
               style="display:inline-block;background:#8c1d40;color:#ffffff;text-decoration:none;padding:12px 20px;font-size:14px;">
              {WebUtility.HtmlEncode(actionText)}
            </a>
            """;
    }
}
