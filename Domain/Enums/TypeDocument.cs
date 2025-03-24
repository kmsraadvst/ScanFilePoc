namespace Domain.Enums;

public class TypeDocument
{
    public const string Projet = "Projet";
    public const string Mandat = "Mandat";
    public const string AnnexeProjet = "Annexe";

    public static readonly IEnumerable<string> All = [Projet, Mandat, AnnexeProjet];
    
    public static string GetEventHub(string type) => type switch
    {
        _ when type == Projet => HubEvents.RefreshProjet,
        _ when type == Mandat => HubEvents.RefreshMandat,
        _ when type == AnnexeProjet => HubEvents.RefreshAnnexe,
        _ => throw new ArgumentOutOfRangeException()
    };
}