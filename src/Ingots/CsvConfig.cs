using System.Text.Json;

namespace Ingots;

public class CsvConfig
{
    public string Bank { get; set; } = string.Empty;
    public int DateIndex { get; set; }
    public int ValueIndex { get; set; }
    public int DescriptionIndex { get; set; }
    public int AccountIndex { get; set; }
    public int TargetIndex { get; set; }
    public string? DateCulture { get; set; }
    public string? ValueCulture { get; set; }
    public string Delimiter { get; set; } = ";";
    public string ThousandSeparator { get; set; } = ".";
    public bool ParseTargetInCommunication { get; set; } = true;

    public static CsvConfig[] GetConfigs()
    {
        var filePath = Path.Combine( AppContext.BaseDirectory , "res" , "csvConfig.json" );

        if ( File.Exists( filePath ) )
        {
            var jsonString = File.ReadAllText( filePath );
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<CsvConfig[]>( jsonString , options );
        }

        return Array.Empty<CsvConfig>();
    }
}