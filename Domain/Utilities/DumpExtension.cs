using System.Text.Json;

namespace Domain.Utilities;

public static class DumpExtension
{
    public static string Dump(this object obj) {
        return JsonSerializer.Serialize(obj);
    }
}