using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ConnectFour.Extensions;

public static class ResultExtensions
{
    public static IResult Htmx(
        this IResultExtensions result,
        [LanguageInjection(InjectedLanguage.HTML)]
        string htmx)
        => Results.Content(htmx, "text/html");
}