
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<DocumentRepository>();
builder.Services.AddSingleton<FileScanService>();
builder.Services.AddSingleton<SignalRClientService>();
builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri("http://localhost:5118"));
builder.Services.AddHttpClient("scan", client => client.BaseAddress = new Uri("http://localhost:5063"));
builder.Services.AddSingleton<Consumer>();

builder.Services.AddHostedService<Worker>();


var host = builder.Build();
host.Run();