var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddMudServices();

builder.Services.AddSignalR();
builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri("http://localhost:5118"));

builder.Services.Configure<ProducersOptions>(builder.Configuration.GetSection("ProducersOptions"));
builder.Services.AddScoped<Producer<DocumentToScanMessage>>();

builder.Services.AddSingleton<DocumentRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
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