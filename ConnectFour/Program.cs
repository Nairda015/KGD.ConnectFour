using ConnectFour.Components;
using ConnectFour.Persistance;
using MiWrap;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

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


// var antiforgery = app.Services.GetRequiredService<IAntiforgery>();
//
// app.Use((context, next) =>
// {
//     var requestPath = context.Request.Path.Value;
//
//     if (string.Equals(requestPath, "/", StringComparison.OrdinalIgnoreCase)
//         || string.Equals(requestPath, "/index.html", StringComparison.OrdinalIgnoreCase))
//     {
//         var tokenSet = antiforgery.GetAndStoreTokens(context);
//         context.Response.Cookies.Append("XSRF-TOKEN", tokenSet.RequestToken!,
//             new CookieOptions { HttpOnly = false });
//     }
//
//     return next(context);
// });

app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapEndpoints<Program>();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Upskill"); });

app.Run();