
using Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();



app.MapGet("/document/{id:int}", (int id) => {
    Console.WriteLine($"worker calls me Document Id[{id}]");
    
    return new Document { Id = id };
});

app.MapPut("/document", (Document document) => {
    Console.WriteLine("document reçu pour update");
    Console.WriteLine($"Id: {document.Id}, Statut: {document.StatutCode}, address: {document.Addresse}");

    return Results.NoContent();
});

app.Run();

