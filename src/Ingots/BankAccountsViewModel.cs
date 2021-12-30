using DynamicData;
using Ingots.Data;
using LanguageExt;
using Mapster;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingots
{
    public class BankAccountsViewModel : ReactiveObject, IActivatableViewModel, IRoutableViewModel
    {
        private readonly IngotsViewModel _ingotsViewModel;

        //private ReadOnlyObservableCollection<BankViewModel> _banks;
        //public ReadOnlyObservableCollection<BankViewModel> Banks => _banks;

        private ReadOnlyObservableCollection<OperationViewModel> _operations;
        public ReadOnlyObservableCollection<OperationViewModel> Operations => _operations;

        [Reactive] public AccountViewModel SelectedAccount { get; set; }
        [Reactive] public OperationViewModel SelectedOperation { get; set; }
        [Reactive] public string SearchInput { get; set; }

        public ReactiveCommand<System.Reactive.Unit , Option<AccountEditionViewModel>> CreateAccountCommand { get; private set; }
        public ReactiveCommand<AccountEditionViewModel , Option<AccountViewModel>> AddAccountCommand { get; private set; }

        public Interaction<System.Reactive.Unit , Option<AccountEditionViewModel>> CreateAccountInteraction { get; }
            = new Interaction<System.Reactive.Unit , Option<AccountEditionViewModel>>( RxApp.MainThreadScheduler );

        public ViewModelActivator Activator { get; }

        public string UrlPathSegment => "BankAccounts";

        public IScreen HostScreen { get; }

        public BankAccountsViewModel( IngotsViewModel? ingotsViewModel = null , IScreen? screen = null )
        {
            _ingotsViewModel = ingotsViewModel
                ?? Locator.Current.GetService<IngotsViewModel>();

            HostScreen = screen ?? Locator.Current.GetService<IScreen>();

            Activator = new ViewModelActivator();

            CreateAccountInteraction.RegisterHandler( ctx => ctx.SetOutput( Option<AccountEditionViewModel>.None ) );

            InitializeCommands( this );

            this.WhenActivated( disposables =>
            {
                var operationsCache = _ingotsViewModel.TransactionsCache.Connect()
                    .Transform( t => t as OperationViewModel )
                    .Or( _ingotsViewModel.TransfersCache.Connect()
                        .AutoRefresh( x => x.IsExecuted )
                        .TransformMany( t => new[]
                         {
                            t as OperationViewModel,
                            t.OppositeTransfer() as OperationViewModel
                        } ,
                        t => (t.OperationId, t.IsDerived, typeof( TransferViewModel )) ) )
                    .AsObservableCache()
                    .DisposeWith( disposables );
            } );

            CreateAccountCommand
                .Subscribe( opt =>
                    opt.Some( aevm =>
                    {
                        Observable.Return( aevm )
                            .InvokeCommand( AddAccountCommand );
                    } )
                    .None( () => { } ) );

            AddAccountCommand
                .Subscribe( opt =>
                     opt.Some( avm => _ingotsViewModel.AccountsCache.AddOrUpdate( avm ) )
                     .None( () => { } ) );
        }

        private static void InitializeCommands( BankAccountsViewModel @this )
        {
            @this.CreateAccountCommand = ReactiveCommand.CreateFromObservable(
                () => @this.CreateAccountInteraction.Handle( System.Reactive.Unit.Default ) );

            @this.AddAccountCommand = ReactiveCommand.CreateFromObservable( ( AccountEditionViewModel aevm )
                 => Observable.Start( () =>
                    @this._ingotsViewModel.Context
                     .Some( ctx =>
                     {
                         var acc = aevm.Adapt<Account>();
                         var added = ctx.Accounts.Add( acc );
                         ctx.SaveChanges();

                         var avm = added.Entity.Adapt<AccountViewModel>();

                         return Option<AccountViewModel>.Some( avm );
                     } )
                     .None( () => Option<AccountViewModel>.None )
                  ) );
        }
    }
}