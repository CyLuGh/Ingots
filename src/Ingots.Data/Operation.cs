namespace Ingots.Data;

public abstract class Operation
{
    public int AccountId { get; set; }
    public long OperationId { get; set; }
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string? Description { get; set; }
    public bool IsExecuted { get; set; }

    public Account? Account { get; set; }
}