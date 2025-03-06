namespace Domain.Entities;

public class Document
{
    public int Id { get; set; }
    public int DemandeAvisId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string StatutCode { get; set; } = "EnCours";
    public string Addresse { get; set; } = string.Empty;
}