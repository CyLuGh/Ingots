namespace Ingots.Data;

public class Account
{
    public int AccountId { get; set; }
    public string Bank { get; set; } = string.Empty;
    public string Bic { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Stash { get; set; } = string.Empty;
    public double StartValue { get; set; }
    public bool IsDeleted { get; set; }
    public AccountKind Kind { get; set; }
}