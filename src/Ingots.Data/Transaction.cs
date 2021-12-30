namespace Ingots.Data;

public class Transaction : Operation
{
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Shop { get; set; }
}