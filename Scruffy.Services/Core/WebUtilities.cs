using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Scruffy.Services.Core;

/// <summary>
/// Web utilities
/// </summary>
public static class WebUtilities
{
    /// <summary>
    /// Converting html to plain text
    /// </summary>
    /// <param name="html">HTML</param>
    /// <returns>Plain text</returns>
    public static string HtmlToPlainText(string html)
    {
        var text = WebUtility.HtmlDecode(html);

        text = new Regex(@"(>|$)(\W|\n|\r)+<", RegexOptions.Multiline).Replace(text, " ><");
        text = new Regex(@"<(br|BR)\s{0,1}\/{0,1}>", RegexOptions.Multiline).Replace(text, Environment.NewLine);
        text = new Regex(@"<[^>]*(>|$)", RegexOptions.Multiline).Replace(text, string.Empty);

        return text;
    }
}