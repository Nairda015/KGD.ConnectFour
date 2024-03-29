using System.Security.Claims;
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
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ClaimsPrincipal>(s => s.GetService<IHttpContextAccessor>()!.HttpContext!.User);
builder.Services.AddAuthentication()
    .AddCookie("xd", x => { x.Cookie.Name = "user_id"; });

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

app.UseAuthentication();

app.UseStaticFiles();

app.MapHub<GameHub>("/game-hub");
app.MapHub<WsHubTest>("/ws-hub");
app.MapHub<LobbyHub>("/lobby-hub");

app.UseAntiforgery();

app.Use(async (context, next) =>
{
    if (!context.User.Identity!.IsAuthenticated && context.Request.Path.Value != "/login")
    {
        context.Response.Redirect("/login");
        return;
    }

    await next(context);
});

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapEndpoints<Program>();

app.MapGet("refresh-board", () => new RazorComponentResult(typeof(NewBoard)));
app.MapGet("subscribe/{gameId}", async (
    GameHub gameHub,
    GameId gameId,
    ClaimsPrincipal user,
    GamesContext gamesContext,
    CancellationToken ct) =>
{
    if (!gamesContext.Exist(gameId)) return Results.Redirect("/");
    var playerId = user.GetPlayerId();
    await gameHub.AddSpectator(gameId, playerId, ct);
    return Results.Ok();
});

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Connect Four"); });

app.Run();