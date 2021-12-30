using DynamicData;
using Ingots.Data;
using Ingots.Settings;
using LanguageExt;
using Mapster;
using MoreLinq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingots;

public class IngotsViewModel : ReactiveObject, IActivatableViewModel
{
    [Reactive] internal UserSettings? Settings { get; set; }
    [Reactive] internal Option<IngotsContext> Context { get; set; }

    public string DatabaseFilePath { [ObservableAsProperty] get; }

    public ReactiveCommand<System.Reactive.Unit , string> PickDbFileCommand { get; private set; }
    public ReactiveCommand<string , System.Reactive.Unit> LoadContextCommand { get; private set; }

    public Interaction<System.Reactive.Unit , string> PickDbFileInteraction { get; }
        = new Interaction<System.Reactive.Unit , string>( RxApp.MainThreadScheduler );

    public ViewModelActivator Activator { get; }
    public RoutingState Router { get; }
    public BankAccountsViewModel BankAccountsViewModel { get; }

    internal SourceCache<AccountViewModel , int> AccountsCache { get; }
            = new SourceCache<AccountViewModel , int>( a => a.AccountId );

    internal SourceCache<TransactionViewModel , (long, bool, Type)> TransactionsCache { get; }
        = new SourceCache<TransactionViewModel , (long, bool, Type)>( t => (t.OperationId, t.IsDerived, typeof( TransactionViewModel )) );

    internal SourceCache<TransferViewModel , (long, bool, Type)> TransfersCache { get; }
        = new SourceCache<TransferViewModel , (long, bool, Type)>( t => (t.OperationId, t.IsDerived, typeof( TransferViewModel )) );

    public IngotsViewModel()
    {
        Activator = new ViewModelActivator();
        Router = new RoutingState();
        BankAccountsViewModel = new BankAccountsViewModel( this );

        InitializeCommands( this );

        this.WhenAnyValue( x => x.DatabaseFilePath )
            .Where( p => !string.IsNullOrWhiteSpace( p ) )
            .InvokeCommand( LoadContextCommand );

        this.WhenActivated( disposables =>
        {
            this.WhenAnyValue( x => x.Context )
                .Subscribe( context =>
                {
                    context.Some( ctx =>
                    {
                        AccountsCache.Clear();
                        TransactionsCache.Clear();
                        TransfersCache.Clear();

                        AccountsCache.AddOrUpdate( ctx.Accounts.ProjectToType<AccountViewModel>() );
                        TransactionsCache.AddOrUpdate( ctx.Transactions.ProjectToType<TransactionViewModel>() );
                        TransfersCache.AddOrUpdate( ctx.Transfers.ProjectToType<TransferViewModel>() );
                    } );
                } )
                .DisposeWith( disposables );

            SetAccountsUpdaters( TransactionsCache , t => t.AccountId ).DisposeWith( disposables );
            SetAccountsUpdaters( TransfersCache , t => t.AccountId ).DisposeWith( disposables );
            SetAccountsUpdaters( TransfersCache , t => t.TargetAccountId , multiplier: -1 ).DisposeWith( disposables );

            Observable.Merge(
                this.WhenAnyValue( x => x.Settings )
                    .Where( x => x == null || string.IsNullOrWhiteSpace( x.DbPath ) )
                    .Select( _ => Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ) , "ingots.db" ) ) ,
                this.WhenAnyValue( x => x.Settings )
                    .Where( x => x != null && !string.IsNullOrWhiteSpace( x.DbPath ) )
                    .Select( u => u.DbPath ) ,
                PickDbFileCommand.Where( path => !string.IsNullOrWhiteSpace( path ) )
                )
                .ToPropertyEx( this , x => x.DatabaseFilePath , scheduler: RxApp.MainThreadScheduler )
                .DisposeWith( disposables );

            Observable.Return( System.Reactive.Unit.Default )
                .Throttle( TimeSpan.FromMilliseconds( 20 ) )
                .Do( _ =>
                {
                    Settings = UserSettingsManager.Load();
                } )
                .Subscribe()
                .DisposeWith( disposables );
        } );
    }

    private static void InitializeCommands( IngotsViewModel @this )
    {
        @this.PickDbFileCommand = ReactiveCommand.CreateFromObservable( () =>
            @this.PickDbFileInteraction.Handle( System.Reactive.Unit.Default ) );

        @this.LoadContextCommand = ReactiveCommand.CreateFromObservable( ( string path ) =>
            Observable.Start( () =>
            {
                @this.Context.Some( ctx => ctx.Dispose() );
                @this.Context = Option<IngotsContext>.Some( new IngotsContext( path ) );
            } ) );
    }

    private IDisposable SetAccountsUpdaters<T>( SourceCache<T , (long, bool, Type)> cache , Func<T , long> selector , int multiplier = 1 ) where T : OperationViewModel
            => cache.Connect()
                .AutoRefresh( x => x.IsExecuted )
                .Subscribe( changes =>
                {
                    changes.Where( x => x.Reason == ChangeReason.Add ).Select( x => x.Current ).ForEach( change =>
                    {
                        var account = AccountsCache.Items.FirstOrDefault( o => o.AccountId == selector( change ) );
                        if ( account != null && change.IsExecuted )
                            account.OperationsValue += change.Value * multiplier;
                    } );

                    changes.Where( x => x.Reason == ChangeReason.Remove ).Select( x => x.Current ).ForEach( change =>
                    {
                        var account = AccountsCache.Items.FirstOrDefault( o => o.AccountId == selector( change ) );
                        if ( account != null && change.IsExecuted )
                            account.OperationsValue -= change.Value * multiplier;
                    } );

                    changes.Where( x => x.Reason == ChangeReason.Update ).ForEach( change =>
                    {
                        var account = AccountsCache.Items.FirstOrDefault( o => o.AccountId == selector( change.Previous.Value ) );
                        if ( account != null && change.Previous.Value.IsExecuted )
                            account.OperationsValue -= change.Previous.Value.Value * multiplier;
                        account = AccountsCache.Items.FirstOrDefault( o => o.AccountId == selector( change.Current ) );
                        if ( account != null && change.Current.IsExecuted )
                            account.OperationsValue += change.Current.Value * multiplier;
                    } );

                    changes.Where( x => x.Reason == ChangeReason.Refresh ).Select( x => x.Current ).ForEach( change =>
                    {
                        var account = AccountsCache.Items.FirstOrDefault( o => o.AccountId == selector( change ) );
                        if ( account != null )
                            account.OperationsValue += change.Value * ( change.IsExecuted ? 1 : -1 ) * multiplier;
                    } );
                } );
}