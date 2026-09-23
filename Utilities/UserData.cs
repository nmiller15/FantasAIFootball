namespace FantasAIFootball.Utilities;

public static class UserData
{
    public static string GetAppDataLocation()
    {
        var userAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDataFolder = Path.Combine(userAppData, "FantasAIFootball");

        return appDataFolder;
    }
}
