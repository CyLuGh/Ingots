using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ingots.Data
{
    public class IngotsContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public string Path { get; }

        public IngotsContext() : this( "ingots.db" )
        {
        }

        public IngotsContext( string path )
        {
            Path = path;
            Database.Migrate();
        }

        protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = Path
            };
            var connection = new SqliteConnection( $"{connectionStringBuilder}" );
            optionsBuilder.UseSqlite( connection );
        }

        protected override void OnModelCreating( ModelBuilder modelBuilder )
        {
            modelBuilder.Entity<Account>().ToTable( "Accounts" );
            modelBuilder.Entity<Account>().HasKey( a => a.AccountId );
            modelBuilder.Entity<Account>().Property( a => a.Iban ).IsRequired();
            modelBuilder.Entity<Account>().Property( a => a.Bank ).IsRequired();
            modelBuilder.Entity<Account>().Property( a => a.Description ).IsRequired();
            modelBuilder.Entity<Account>().Property( a => a.Stash ).HasDefaultValue( "None" ).IsRequired();
            modelBuilder.Entity<Account>().Property( a => a.IsDeleted ).HasDefaultValue( false ).IsRequired();
            modelBuilder.Entity<Account>().Property( a => a.StartValue ).HasDefaultValue( 0 ).IsRequired();
            modelBuilder.Entity<Account>().Property( a => a.Kind ).HasDefaultValue( AccountKind.Checking ).IsRequired();

            modelBuilder.Entity<Transaction>().ToTable( "Transactions" );
            modelBuilder.Entity<Transaction>().HasKey( t => t.OperationId );
            modelBuilder.Entity<Transaction>().Property( t => t.Value ).IsRequired();
            modelBuilder.Entity<Transaction>().Property( t => t.Description ).IsRequired().HasDefaultValue( string.Empty );
            modelBuilder.Entity<Transaction>().Property( t => t.Date ).IsRequired();
            // see https://github.com/dotnet/efcore/issues/20110#issuecomment-597368911
            modelBuilder.Entity<Transaction>().Property( t => t.IsExecuted ).HasDefaultValue( true ).IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<Transaction>().Property( t => t.AccountId ).IsRequired();
            modelBuilder.Entity<Transaction>().Property( t => t.Category ).HasDefaultValue( "None" ).IsRequired();
            modelBuilder.Entity<Transaction>().Property( t => t.SubCategory ).HasDefaultValue( "None" ).IsRequired();
            modelBuilder.Entity<Transaction>().Property( t => t.Shop ).HasDefaultValue( "None" ).IsRequired();
            modelBuilder.Entity<Transaction>().HasOne( t => t.Account ).WithMany().HasForeignKey( t => t.AccountId );

            modelBuilder.Entity<Transfer>().ToTable( "Transfers" );
            modelBuilder.Entity<Transfer>().HasKey( t => t.OperationId );
            modelBuilder.Entity<Transfer>().Property( t => t.Value ).IsRequired();
            modelBuilder.Entity<Transfer>().Property( t => t.Description ).IsRequired().HasDefaultValue( string.Empty );
            modelBuilder.Entity<Transfer>().Property( t => t.Date ).IsRequired();
            modelBuilder.Entity<Transfer>().Property( t => t.IsExecuted ).HasDefaultValue( true ).IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<Transfer>().Property( t => t.AccountId ).IsRequired();
            modelBuilder.Entity<Transfer>().Property( t => t.TargetAccountId ).IsRequired();
            modelBuilder.Entity<Transfer>().HasOne( t => t.Account ).WithMany().HasForeignKey( t => t.AccountId );
            modelBuilder.Entity<Transfer>().HasOne( t => t.TargetAccount ).WithMany().HasForeignKey( t => t.TargetAccountId );
        }
    }
}