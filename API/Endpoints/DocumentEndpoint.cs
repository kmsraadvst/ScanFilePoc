namespace ScanFilePoc.Endpoints;

public static class DocumentEndpoint
{
   public static WebApplication MapDocument(this WebApplication app)
   {
      app.MapGet("/document/{id:int}", async (int id, DocumentRepository repo) =>
      
         await repo.GetByIdAsync(id) is Document document
            ? Results.Ok(document)
            : Results.NotFound()
      );
      
      app.MapGet("/document", async (DocumentRepository repo) =>
      
         await repo.GetAllAsync()
      );
      
      app.MapPost("/Document", async (Document document, DocumentRepository repo) =>
      {
         var id = await repo.CreateAsync(document);

         var documentCreated = await repo.GetByIdAsync(id);

         return Results.Created($"/document/{id}", documentCreated);
      });
      
      app.MapDelete("/document/{id:int}", async (int id, DocumentRepository repo) =>
         
          await repo.DeleteAsync(id)
      );


      return app;
   }
}