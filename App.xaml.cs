using System.IO;
using System.Windows;
using System.Windows.Threading;
using Ynost.Services;
using Ynost.View;
namespace Ynost
{
    public partial class App : Application
    {
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string dump = e.Exception.ToString();
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            File.WriteAllText(Path.Combine(desktop, "ui-crash.txt"), dump);

            MessageBox.Show(dump[..Math.Min(500, dump.Length)], "UI-crash");
            e.Handled = true;
        }

        public static DatabaseService Db { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            const string conn =
                "Host=91.192.168.52;" +
                "Port=5432;" +
                "Database=ynost_db;" +
                "Username=teacher_app;" +
                "Password=T_pass;" +
                "Timeout=10;Command Timeout=30;" +
                "Ssl Mode=Prefer;";          // Require + Trust... если сервер принуждает SSL

            Db = new DatabaseService(conn);

            base.OnStartup(e);
        }
    }
}
