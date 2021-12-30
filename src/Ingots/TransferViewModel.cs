using ReactiveUI.Fody.Helpers;

namespace Ingots;

public class TransferViewModel : OperationViewModel
{
    [Reactive] public int TargetAccountId { get; set; }

    internal TransferViewModel OppositeTransfer()
    {
        var opposite = (TransferViewModel) Clone();
        opposite.Value = Value * -1;
        opposite.AccountId = TargetAccountId;
        opposite.TargetAccountId = AccountId;
        opposite.IsDerived = true;
        return opposite;
    }

    public override object Clone() // Don't use MemberWiseClone as it messes with DynamicData
        => new TransferViewModel
        {
            OperationId = OperationId ,
            AccountId = AccountId ,
            Date = Date ,
            Value = Value ,
            Description = Description ,
            IsExecuted = IsExecuted ,
            TargetAccountId = TargetAccountId ,
            IsDerived = IsDerived
        };
}