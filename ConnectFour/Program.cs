using ConnectFour.Components;
using ConnectFour.Hubs;
using ConnectFour.Persistance;
using MiWrap;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSignalR();
builder.Services.AddHostedService<BackgroundPublisher>();
builder.Services.AddSingleton<WsHubTest>();

builder.Services.AddSingleton<InMemoryGamesState>();
builder.Services.RegisterHandlers<Program>();


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
app.MapHub<BoardHub>("/board-hub");
app.MapHub<WsHubTest>("/ws-hub");


app.MapEndpoints<Program>();


app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Upskill"); });

app.Run();