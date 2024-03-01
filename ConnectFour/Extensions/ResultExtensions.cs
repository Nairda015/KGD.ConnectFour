using System.Net.Mime;
using System.Text;
using JetBrains.Annotations;

namespace ConnectFour.Extensions;

public static class ResultsExtensions
{
    public static IResult Html(this IResultExtensions resultExtensions, [LanguageInjection(InjectedLanguage.HTML)] string html)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);

        return new HtmlResult(html);
    }
}

public class HtmlResult : IResult
{
    private readonly string _html;

    public HtmlResult(string html)
    {
        _html = html;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = MediaTypeNames.Text.Html;
        httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_html);
        return httpContext.Response.WriteAsync(_html);
    }
}
