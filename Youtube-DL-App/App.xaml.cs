using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Youtube_DL_App {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            MessageBox.Show("An unhandled exception occured: " + e.Exception.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            MainWindow mainWindow;
            if (e.Args != null && e.Args.Length > 0 && e.Args[0].ToLower() == "console")
                mainWindow = new MainWindow(true); // show console
            else
                mainWindow = new MainWindow(false); // do not show console
            mainWindow.Show();
        }
    }
}
