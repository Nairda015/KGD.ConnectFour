using ConnectFour.Components;
using ConnectFour.Components.Shared;
using ConnectFour.Extensions;
using ConnectFour.Hubs;
using ConnectFour.Models;
using ConnectFour.Persistence;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using MiWrap;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

//ws test
builder.Services.AddHostedService<BackgroundPublisher>();
builder.Services.AddSingleton<WsHubTest>();

//hubs
builder.Services.AddScoped<GameHub>();
builder.Services.AddScoped<LobbyHub>();


builder.Services.AddSingleton<PlayersContext>();
builder.Services.AddSingleton<GamesContext>();

builder.Services.RegisterHandlers<Program>();

//razor to string rendering 
builder.Services.AddScoped<HtmlRenderer>();
builder.Services.AddScoped<BlazorRenderer>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.OrderActionsBy(x => x.HttpMethod); });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapHub<GameHub>("/game-hub");
app.MapHub<WsHubTest>("/ws-hub");
app.MapHub<LobbyHub>("/lobby-hub");

app.MapEndpoints<Program>();


app.MapGet("game-url/{gameId}", (HttpContext ctx, string gameId) =>
{
    ctx.Response.Headers.Add("HX-Push-Url", $"game/{gameId}");
    return Results.Ok();
});

app.MapGet("click", async() =>
{
    await Task.Delay(800);
    return Results.Ok(Guid.NewGuid().ToString());
});


app.MapGet("in-game-buttons", () => new RazorComponentResult(typeof(InGameButtons)));
app.MapGet("new-game-buttons", () => new RazorComponentResult(typeof(NewGameButtons)));
app.MapGet("score/{playerId}", (PlayerId playerId, PlayersContext ctx) => ctx.GetPlayerScore(playerId).ToString());

app.MapGet("game/{gameId}", (HttpContext ctx, GamesContext db, GameId gameId) =>
{
    //TODO: create separate endpoint for game search
    if (db.TryGetGameState(gameId, out var log) && !log!.IsComplete)
    {
        //replace target and refresh board 
        //if game is on going subscribe to group (how to handle dropped subscription?)
        return Results.Ok();
    }

    //ctx.Response.Headers.Add("HX-Push-Url", "/");
    return Results.Redirect("/");
});

app.MapGet("refresh-board", () => new RazorComponentResult(typeof(Board)));


app.MapGet("test-multi", async (BlazorRenderer renderer) =>
{
    var result = await renderer.RenderComponent<MultiSwap>();
    return Results.Extensions.Htmx(result);
});


app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Upskill"); });

app.Run();