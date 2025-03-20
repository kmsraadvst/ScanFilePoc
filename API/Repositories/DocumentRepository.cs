namespace ScanFilePoc.Repositories;

public class DocumentRepository
{
    private const string ConnectionString =
        "Server=W2k22-Devel-T01; Database=FileScanDB; User ID=filescanuser; Password=filescan123.; Encrypt=False;";


    public async Task<Document?> GetByIdAsync(int id)
    {
        await using var connection = new SqlConnection(ConnectionString);

        var sql = """
                  SELECT * FROM Document WHERE Id = @id
                  """;

        return await connection.QuerySingleOrDefaultAsync<Document>(sql, new { id });

    }
    
    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);

        var sql = """
                  SELECT * FROM Document
                  """;

        return await connection.QueryAsync<Document>(sql);

    }
    
    public async Task<int> CreateAsync(Document document)
    {
        await using var connection = new SqlConnection(ConnectionString);

        var sql = """
                  INSERT INTO Document (
                      DemandeAvisId
                      ,Titre
                      ,NomFichier
                      ,Chemin
                      ,Extension
                      ,ContentType
                      ,Taille
                      ,TypeCode
                      ,StatutCode
                      ,TelechargementLe
                  )
                  output inserted.Id
                  values (
                      @DemandeAvisId
                      ,@Titre
                      ,@NomFichier
                      ,@Chemin
                      ,@Extension
                      ,@ContentType
                      ,@Taille
                      ,@TypeCode
                      ,@StatutCode
                      ,GetDate()
                  )
                  """;

        var id = await connection.ExecuteScalarAsync<int>(sql, document);

        return id;
    }
    
    public async Task<int> DeleteAsync(int id)
    {
        await using var connection = new SqlConnection(ConnectionString);

        var sql = """
                  DELETE FROM Document WHERE Id = @id
                  """;

        return await connection.ExecuteAsync(sql, new { id });

    }
}