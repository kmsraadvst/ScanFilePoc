namespace Domain.Utilities;

public static class DocumentUtilities
{
    public const string TempRootPath = "File_Temp";
    public const string FileRootPath = "File_Valide";
    
    public static string GetTempPath(Document document) {
        var fileName =
            $"{document.DemandeAvisId}-{document.TypeCode}-{document.Id}.{document.Extension}";

        return Path.Combine(TempRootPath, fileName);
    }
}