using CsvHelper;
using CsvHelper.Configuration;
using Ingots.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ingots;

public static class CsvImporter
{
    private static readonly Regex _sWhitespace = new Regex( @"\s+" );

    private static string ReplaceWhitespace( string input , string replacement )
    {
        return _sWhitespace.Replace( input , replacement );
    }

    public static IEnumerable<Operation> ParseFile( string path , Account currentAccount , Account[] accounts )
    {
        if ( !File.Exists( path ) )
            yield break;

        var csvParsers = CsvConfig.GetConfigs()
            .ToDictionary( x => x.Bank );

        if ( csvParsers.TryGetValue( currentAccount.Bank , out var parser ) )
        {
            var config = new CsvConfiguration( CultureInfo.InvariantCulture )
            {
                Delimiter = parser.Delimiter
            };

            using var reader = new StreamReader( path );
            using var csv = new CsvReader( reader , config );

            csv.Read();
            csv.ReadHeader();

            while ( csv.Read() )
            {
                var date = DateTime.Parse( csv.GetField<string>( parser.DateIndex ) ,
                    string.IsNullOrEmpty( parser.DateCulture ) ?
                    CultureInfo.InvariantCulture : new CultureInfo( parser.DateCulture ) );

                var value = double.Parse( csv.GetField<string>( parser.ValueIndex )
                        .Replace( parser.ThousandSeparator , "" )
                        .Replace( " " , "" )
                        .Replace( "+" , "" ) ,
                    string.IsNullOrEmpty( parser.ValueCulture ) ?
                    CultureInfo.InvariantCulture : new CultureInfo( parser.ValueCulture ) );

                var accountIban = parser.AccountIndex != -1 ?
                    csv.GetField<string>( parser.AccountIndex ) : string.Empty;

                var targetIban = parser.TargetIndex != -1 ?
                    csv.GetField<string>( parser.TargetIndex ) : string.Empty;

                var description = csv.GetField<string>( parser.DescriptionIndex );

                if ( string.IsNullOrEmpty( targetIban ) )
                {
                    var pattern = @"([A-Z]{2})\d+";
                    var match = Regex.Match( description , pattern );
                    if ( match.Success )
                        targetIban = match.Value;
                }

                targetIban = ReplaceWhitespace( targetIban , string.Empty );

                var targetAccount = Array.Find( accounts , a => a.Iban.Equals( targetIban ) );

                if ( targetAccount != null )
                {
                    var transfer = new Transfer
                    {
                        Account = currentAccount ,
                        TargetAccount = targetAccount ,
                        Date = date ,
                        Value = value ,
                        Description = description ,
                        IsExecuted = true
                    };

                    yield return transfer;
                }
                else
                {
                    var transaction = new Transaction
                    {
                        Account = currentAccount ,
                        Date = date ,
                        Value = value ,
                        Description = description ,
                        IsExecuted = true
                    };

                    yield return transaction;
                }
            }
        }
    }
}