using BlazorServerUI.Components;
using BlazorServerUI.Hubs;
using BlazorServerUI.RabbitMQ;
using Domain.Contracts;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddMudServices();

builder.Services.AddSignalR();

var p_documentToScan = await ProducerFactory.CreateProducerAsync<DocumentToScanMessage>();
builder.Services.AddSingleton(_ => p_documentToScan);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapHub<MyHub>("/my-hub");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();