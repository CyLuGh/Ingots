using ReactiveUI.Fody.Helpers;

namespace Ingots;

public class TransactionViewModel : OperationViewModel
{
    [Reactive] public string? Category { get; set; }
    [Reactive] public string? SubCategory { get; set; }
    [Reactive] public string? Shop { get; set; }

    public override object Clone() // Don't use MemberWiseClone as it messes with DynamicData
        => new TransactionViewModel
        {
            OperationId = OperationId ,
            AccountId = AccountId ,
            Date = Date ,
            Value = Value ,
            Description = Description ,
            IsExecuted = IsExecuted ,
            Category = Category ,
            SubCategory = SubCategory ,
            Shop = Shop
        };
}