using System.Threading.Channels;
using ConnectFour.Components;
using ConnectFour.Components.Shared.Board;
using ConnectFour.Examples.WebSocket;
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

//channels
builder.Services.AddSingleton(Channel.CreateUnbounded<LobbyUpdateToken>());
builder.Services.AddHostedService<LobbyUpdateConsumer>();

//ws test
builder.Services.AddHostedService<BackgroundPublisher>();
builder.Services.AddSingleton<WsHubTest>();

//hubs
builder.Services.AddTransient<GameHub>();
builder.Services.AddTransient<LobbyHub>();

builder.Services.AddSingleton<PlayersContext>();
builder.Services.AddSingleton<GamesContext>();

builder.Services.RegisterHandlers<Program>();

//razor to string rendering 
builder.Services.AddTransient<HtmlRenderer>();
builder.Services.AddTransient<BlazorRenderer>();


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

app.MapHub<GameHub>("/game-hub");
app.MapHub<WsHubTest>("/ws-hub");
app.MapHub<LobbyHub>("/lobby-hub");

app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapEndpoints<Program>();


//TODO: This is hack because there is no support for htmx headers in signalr
app.MapGet("game-url/{gameId}", (HttpContext ctx, string gameId) =>
{
    ctx.Response.Headers.Append("HX-Push-Url", $"game/{gameId}");
    return Results.Ok();
});

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


app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Connect Four"); });

app.Run();