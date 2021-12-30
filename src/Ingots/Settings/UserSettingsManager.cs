using Newtonsoft.Json;

namespace Ingots.Settings;

internal static class UserSettingsManager
{
    public static void Save( UserSettings settings )
    {
        var directory = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ) , "Ingot6" );
        if ( !Directory.Exists( directory ) )
            Directory.CreateDirectory( directory );

        var serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        var file = Path.Combine( directory , "UserSettings.json" );

        using var sw = new StreamWriter( file );
        using var writer = new JsonTextWriter( sw );
        serializer.Serialize( writer , settings );
    }

    public static UserSettings? Load()
    {
        var file = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ) , "Ingot6" , "UserSettings.json" );

        if ( File.Exists( file ) )
        {
            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            using var sr = new StreamReader( file );
            using var reader = new JsonTextReader( sr );
            return serializer.Deserialize<UserSettings>( reader );
        }

        return default;
    }
}