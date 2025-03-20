namespace Domain.Entities;

public class Document
{
    public int Id { get; set; }
    public int DemandeAvisId { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string NomFichier { get; set; } = string.Empty;
    public string Chemin { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Taille { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string StatutCode { get; set; } = string.Empty;
    public DateTime? TelechargementLe { get; set; }
}