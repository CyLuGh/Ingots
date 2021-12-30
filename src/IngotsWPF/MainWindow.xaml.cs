using Ingots;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveMarbles.ObservableEvents;
using MahApps.Metro.Controls;

namespace IngotsWPF
{
    public partial class MainWindow : IViewFor<IngotsViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = Locator.Current.GetService<IngotsViewModel>();

            this.WhenActivated( disposables =>
            {
                this.WhenAnyValue( x => x.ViewModel )
                    .WhereNotNull()
                    .Do( vm => PopulateFromViewModel( this , vm , disposables ) )
                    .Subscribe()
                    .DisposeWith( disposables );
            } );
        }

        private static void PopulateFromViewModel( MainWindow view , IngotsViewModel viewModel , CompositeDisposable disposables )
        {
            view.BankAccountsItem.Tag = viewModel.BankAccountsViewModel;

            view.OneWayBind( viewModel ,
                vm => vm.Router ,
                v => v.RoutedViewHost.Router )
                .DisposeWith( disposables );

            view.OneWayBind( viewModel ,
                vm => vm.DatabaseFilePath ,
                v => v.TextBlockPath.Text )
                .DisposeWith( disposables );

            view.HamburgerMenu.Events()
                .ItemInvoked
                .Subscribe( e =>
                {
                    viewModel.Router.Navigate.Execute( (IRoutableViewModel) ( (HamburgerMenuIconItem) e.InvokedItem ).Tag );
                } )
                .DisposeWith( disposables );
        }

        public IngotsViewModel? ViewModel { get; set; }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as IngotsViewModel; }
    }
}