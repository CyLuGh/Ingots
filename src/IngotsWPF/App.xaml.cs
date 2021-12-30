using Ingots;
using ReactiveUI;
using Splat;
using System.Windows;

namespace IngotsWPF;

public partial class App : Application
{
    public App()
    {
        Locator.CurrentMutable.RegisterConstant( new IngotsViewModel() );

        Locator.CurrentMutable.RegisterConstant( new BankAccountsView() ,
            typeof( IViewFor<BankAccountsViewModel> ) );
    }
}