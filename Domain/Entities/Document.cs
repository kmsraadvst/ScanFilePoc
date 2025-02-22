namespace Domain.Entities;

public class Document
{
    public int Id { get; set; }
    public string StatutCode { get; set; } = "EnCours";
    public string Addresse { get; set; } = string.Empty;
}