namespace Ingots.Data;

public class Transfer : Operation
{
    public int TargetAccountId { get; set; }
    public Account? TargetAccount { get; set; }
}