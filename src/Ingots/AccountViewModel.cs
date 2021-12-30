using Ingots.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Ingots;

public class AccountViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }

    [Reactive] public int AccountId { get; set; }
    [Reactive] public string Bank { get; set; }
    [Reactive] public string Iban { get; set; }
    [Reactive] public string Bic { get; set; }
    [Reactive] public string Description { get; set; }
    [Reactive] public string Stash { get; set; }
    [Reactive] public double StartValue { get; set; }
    [Reactive] public bool IsDeleted { get; set; }
    [Reactive] public AccountKind Kind { get; set; }

    [Reactive] public double OperationsValue { get; set; }

    public double CurrentValue { [ObservableAsProperty]get; }

    public AccountViewModel()
    {
        Activator = new ViewModelActivator();

        this.WhenActivated( disposables =>
        {
            this.WhenAnyValue( o => o.StartValue )
                .CombineLatest( this.WhenAnyValue( o => o.OperationsValue ) ,
                ( sv , ops ) => sv + ops )
                .Throttle( TimeSpan.FromMilliseconds( 100 ) )
                .ToPropertyEx( this , x => x.CurrentValue , scheduler: RxApp.MainThreadScheduler )
                .DisposeWith( disposables );
        } );
    }
}