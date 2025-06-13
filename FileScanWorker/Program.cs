
using Domain.Contracts.Notifications;
using Domain.Settings;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddSingleton(
    builder.Configuration.GetSection("RabbitMq").Get<RabbitMqConsumerSettings>()!
);

builder.Services.AddSingleton<DocumentRepository>();

builder.Services.AddSingleton<SignalRClientService<DocumentUpdatedNotification>>();
builder.Services.AddSingleton<FileScanService>();
builder.Services.AddSingleton<CheckExtensionService>();
builder.Services.AddSingleton<ProcessMessageService>();

builder.Services.AddSingleton<Consumer>();

builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri("http://localhost:5118"));

builder.Services.AddHttpClient("scan", client => client.BaseAddress = new Uri("http://localhost:5063"));

builder.Services.AddHostedService<Worker>();


var host = builder.Build();
host.Run();