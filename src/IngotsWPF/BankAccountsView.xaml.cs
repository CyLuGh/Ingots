using Ingots;
using LanguageExt;
using MaterialDesignThemes.Wpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IngotsWPF;

/// <summary>
/// Interaction logic for BankAccountsView.xaml
/// </summary>
public partial class BankAccountsView
{
    public BankAccountsView()
    {
        InitializeComponent();

        this.WhenActivated( disposables =>
        {
            this.WhenAnyValue( x => x.ViewModel )
                .WhereNotNull()
                .Do( x => PopulateFromViewModel( this , x , disposables ) )
                .Subscribe()
                .DisposeWith( disposables );
        } );
    }

    private static void PopulateFromViewModel( BankAccountsView view , BankAccountsViewModel viewModel , CompositeDisposable disposables )
    {
        view.BindCommand( viewModel ,
                vm => vm.CreateAccountCommand ,
                v => v.ButtonAddAccount )
                .DisposeWith( disposables );

        viewModel.CreateAccountInteraction
                .RegisterHandler( async ctx =>
                {
                    var aevm = new AccountEditionViewModel();
                    var res = (bool) await DialogHost.Show( new AccountEditionView { ViewModel = aevm , Width = 650 } ).ConfigureAwait( false );

                    if ( res )
                        ctx.SetOutput( Option<AccountEditionViewModel>.Some( aevm ) );
                    else
                        ctx.SetOutput( Option<AccountEditionViewModel>.None );
                } )
                .DisposeWith( disposables );
    }
}