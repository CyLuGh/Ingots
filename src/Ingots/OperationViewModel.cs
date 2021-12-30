using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Ingots;

public abstract class OperationViewModel : ReactiveObject, IActivatableViewModel, ICloneable
{
    [Reactive] public int AccountId { get; set; }
    [Reactive] public long OperationId { get; set; }
    [Reactive] public DateTime Date { get; set; }
    [Reactive] public double Value { get; set; }
    [Reactive] public string Description { get; set; }
    [Reactive] public bool IsExecuted { get; set; }
    [Reactive] public bool IsDerived { get; set; }

    [Reactive] public string ImportReference { get; set; }
    [Reactive] public bool IsSelectedForImport { get; set; }

    public ViewModelActivator Activator { get; }

    protected OperationViewModel()
    {
        Activator = new ViewModelActivator();
    }

    public abstract object Clone();
}