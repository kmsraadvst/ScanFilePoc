
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<DocumentRepository>();

builder.Services.AddSingleton<SignalRClientService>();
builder.Services.AddSingleton<FileScanService>();
builder.Services.AddSingleton<CheckExtensionService>();
builder.Services.AddSingleton<ProcessMessageService>();

builder.Services.AddSingleton<Consumer>();

builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri("http://localhost:5118"));

// builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri("http://localhost:5118"),(serviceProvider, client) =>
// {
//     var settings = serviceProvider.GetService<Settings>();
//         
//     client.BaseAddress = new Uri(settings!.EprolexApiBaseAddress);
//     client.DefaultRequestHeaders.Add("Utilisateur-Id", "2");
// });



builder.Services.AddHttpClient("scan", client => client.BaseAddress = new Uri("http://localhost:5063"));

builder.Services.AddHostedService<Worker>();


var host = builder.Build();
host.Run();