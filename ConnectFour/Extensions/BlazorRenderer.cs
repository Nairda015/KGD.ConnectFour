using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ConnectFour.Extensions;

public class BlazorRenderer(HtmlRenderer htmlRenderer)
{
    // Renders a component T which doesn't require any parameters
    public Task<string> RenderComponent<T>() where T : IComponent
        => RenderComponent<T>(ParameterView.Empty);

    // Renders a component T using the provided dictionary of parameters
    public Task<string> RenderComponent<T>(Dictionary<string, object?> dictionary) where T : IComponent
        => RenderComponent<T>(ParameterView.FromDictionary(dictionary));

    public Task<string> RenderComponent<T>(ParameterView parameters) where T : IComponent
    {
        // Use the default dispatcher to invoke actions in the context of the 
        // static HTML renderer and return as a string
        return htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await htmlRenderer.RenderComponentAsync<T>(parameters);
            return output.ToHtmlString();
        });
    }
}