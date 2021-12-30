using Ingots;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ReactiveMarbles.ObservableEvents;
using Ingots.Data;
using MaterialDesignThemes.Wpf;

namespace IngotsWPF;

public partial class AccountEditionView
{
    public AccountEditionView()
    {
        InitializeComponent();

        this.WhenActivated( disposables =>
        {
            this.WhenAnyValue( x => x.ViewModel )
                .WhereNotNull()
                .Do( vm => PopulateFromViewModel( this , vm , disposables ) )
                .Subscribe()
                .DisposeWith( disposables );
        } );
    }

    private static void PopulateFromViewModel( AccountEditionView view , AccountEditionViewModel viewModel , CompositeDisposable disposables )
    {
        view.ComboBoxKind.ItemsSource = Enum.GetValues<AccountKind>();
        view.ButtonCancel.Command = DialogHost.CloseDialogCommand;
        view.ButtonCancel.CommandParameter = false;
        view.ButtonCreate.Command = DialogHost.CloseDialogCommand;
        view.ButtonCreate.CommandParameter = true;

        view.OneWayBind( viewModel ,
            vm => vm.CanSave ,
            v => v.ButtonCreate.IsEnabled )
            .DisposeWith( disposables );

        view.OneWayBind( viewModel ,
                vm => vm.AccountId ,
                v => v.TextBlockHeader.Text ,
                id => id == 0 ? "NEW ACCOUNT" : "EDIT ACCOUNT" )
                .DisposeWith( disposables );

        view.OneWayBind( viewModel ,
            vm => vm.AccountId ,
            v => v.ButtonCreate.Content ,
            id => id == 0 ? "CREATE ACCOUNT" : "UPDATE ACCOUNT" )
            .DisposeWith( disposables );

        view.Bind( viewModel ,
            vm => vm.Iban ,
            v => v.TextBoxIban.Text )
            .DisposeWith( disposables );

        view.Bind( viewModel ,
            vm => vm.Description ,
            v => v.TextBoxDescription.Text )
            .DisposeWith( disposables );

        view.Bind( viewModel ,
            vm => vm.Bank ,
            v => v.TextBoxBank.Text )
            .DisposeWith( disposables );

        view.Bind( viewModel ,
            vm => vm.Bic ,
            v => v.TextBoxBic.Text )
            .DisposeWith( disposables );

        view.Bind( viewModel ,
            vm => vm.Stash ,
            v => v.TextBoxStash.Text )
            .DisposeWith( disposables );

        view.Bind( viewModel ,
            vm => vm.Kind ,
            v => v.ComboBoxKind.SelectedItem )
            .DisposeWith( disposables );

        view.Bind( viewModel ,
            vm => vm.StartValueInput ,
            v => v.TextBoxStartValue.Text )
            .DisposeWith( disposables );

        view.TextBoxStartValue.Events().GotFocus
            .Subscribe( _ => view.TextBoxStartValue.SelectAll() )
            .DisposeWith( disposables );

        view.TextBoxStartValue.Events().LostFocus
            .Select( _ => viewModel.StartValue.ToString( "C2" ) )
            .BindTo( view , x => x.TextBoxStartValue.Text )
            .DisposeWith( disposables );
    }
}