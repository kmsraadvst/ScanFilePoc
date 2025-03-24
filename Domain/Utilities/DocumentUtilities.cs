namespace Domain.Utilities;

public static class DocumentUtilities
{
    private const string RootPath = "File_Temp";
    
    public static string GetPath(Document document) {
        var fileName =
            $"{document.DemandeAvisId}-{document.TypeCode}-{document.Id}.{document.Extension}";

        return Path.Combine(RootPath, fileName);
    }
}