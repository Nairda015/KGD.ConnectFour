using JetBrains.Annotations;

namespace ConnectFour.Extensions;

public static class ResultExtensions
{
    public static IResult Htmx(
        this IResultExtensions result,
        [LanguageInjection(InjectedLanguage.HTML)]
        string htmx)
        => Results.Ok(htmx);
}