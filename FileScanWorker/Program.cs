using FileScanWorker;
using FileScanWorker.RabbitMQ;
using FileScanWorker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<FileScanService>();
builder.Services.AddSingleton<SignalRClientService>();
builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri("http://localhost:5118"));
builder.Services.AddSingleton<Consumer>();

builder.Services.AddHostedService<Worker>();


var host = builder.Build();
host.Run();