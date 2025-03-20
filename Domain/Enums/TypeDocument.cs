namespace Domain.Enums;

public class TypeDocument
{
    public const string Projet = "Projet";
    public const string Mandat = "Mandat";
    public const string AnnexeProjet = "Annexe";

    public static readonly IEnumerable<string> All = [Projet, Mandat, AnnexeProjet];
}