namespace FileScanWorker.Services;

public class CheckExtensionService
{
    public async Task<bool> CheckAsync(Document document)
    {
        await Task.Delay(600);

        var isExtensionValid = new Random().Next(1, 9) != 5;
        return isExtensionValid;
    }
}