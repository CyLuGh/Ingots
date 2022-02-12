namespace Ingots.Data;

public class Transaction : Operation
{
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Shop { get; set; }

    public override string ToString()
        => $"{Date:yyyy-MM-dd} {Description} {Value:N2}";
}