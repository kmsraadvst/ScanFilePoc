using Domain.Dtos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/scan-file", (FilePathDto filePath) =>
{
    Console.WriteLine($"scan for {filePath.path}");

    var result = new Random().Next(1, 9) <= 5;

    return TypedResults.Ok(new ResultScanDto(result));
});



app.Run();

