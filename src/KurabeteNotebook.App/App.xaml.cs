using System.Windows;
using KurabeteNotebook.Data;

namespace KurabeteNotebook.App;

public partial class App : Application
{
    public static Microsoft.Data.Sqlite.SqliteConnection DbConnection { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DbConnection = DatabaseInitializer.CreateConnection();
        DatabaseInitializer.Initialize(DbConnection);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        DbConnection?.Dispose();
        base.OnExit(e);
    }
}
