namespace ScanFilePoc.Repositories;

public class DocumentRepository
{
    private const string ConnectionString0 =
        "Server=W2k22-Devel-T01; Database=FileScanDB; User ID=filescanuser; Password=filescan123.; Encrypt=False;";

    // Local CDE
    private const string ConnectionString =
        "Server=localhost; Database=ScanFileDB; User ID=sa; Password=huk@r2Xmen99; Encrypt=False;";

    // Local Maison
    private const string ConnectionString1 =
        "Server=localhost; Database=FileScanDB; User ID=sa; Password=Huk@r99_Dba; Encrypt=False;";


    public async Task<Document?> GetByIdAsync(int id)
    {
        await using var connection = new SqlConnection(ConnectionString);

        const string sql = """
                           SELECT * 
                           FROM Document 
                           WHERE Id = @id
                           """;

        return await connection.QuerySingleOrDefaultAsync<Document>(sql, new { id });
    }

    public async Task<IEnumerable<Document>> GetAllAsync(string typeCode, int demandeAvisId)
    {
        await using var connection = new SqlConnection(ConnectionString);

        const string sql = """
                           SELECT * 
                           FROM Document
                           WHERE DemandeAvisId = @demandeAvisId 
                             AND TypeCode = @typeCode
                           """;

        return await connection.QueryAsync<Document>(sql, new { demandeAvisId, typeCode });
    }

    public async Task<int> CreateAsync(Document document)
    {
        await using var connection = new SqlConnection(ConnectionString);

        const string sql = """
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

        const string sql = """
                           DELETE FROM Document 
                                  WHERE Id = @id
                           """;

        return await connection.ExecuteAsync(sql, new { id });
    }
}