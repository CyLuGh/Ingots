using Ingots.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Ingots;

public class AccountEditionViewModel : ReactiveObject, IActivatableViewModel
{
    public int AccountId { get; set; }
    [Reactive] public string? Iban { get; set; }
    [Reactive] public string? Description { get; set; }
    [Reactive] public string? Bank { get; set; }
    [Reactive] public string? Bic { get; set; }
    [Reactive] public string? Stash { get; set; }
    [Reactive] public AccountKind Kind { get; set; }
    [Reactive] public string? StartValueInput { get; set; }
    [Reactive] public bool IsDeleted { get; set; }

    public double StartValue { [ObservableAsProperty] get; }
    public bool CanSave { [ObservableAsProperty] get; }

    public ViewModelActivator Activator { get; }

    public AccountEditionViewModel()
    {
        Activator = new ViewModelActivator();

        this.WhenActivated( disposables =>
         {
             this.WhenAnyValue( x => x.StartValueInput )
                 .Select( s =>
                     {
                         if ( !string.IsNullOrWhiteSpace( s ) && double.TryParse( s , NumberStyles.Currency , CultureInfo.CurrentCulture , out var start ) )
                             return start;

                         return 0d;
                     } )
                 .ToPropertyEx( this , x => x.StartValue , scheduler: RxApp.MainThreadScheduler )
                 .DisposeWith( disposables );

             this.WhenAnyValue( x => x.Iban , x => x.Description )
                 .Select( t =>
                 {
                     var (iban, desc) = t;

                     return !string.IsNullOrWhiteSpace( iban )
                         && !string.IsNullOrWhiteSpace( desc );
                 } )
                 .ToPropertyEx( this , x => x.CanSave , scheduler: RxApp.MainThreadScheduler );
         } );
    }
}