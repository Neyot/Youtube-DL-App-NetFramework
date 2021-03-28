using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Diagnostics;
using Serilog;

namespace Youtube_DL_App {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            MessageBox.Show("An unhandled exception occured: " + e.Exception.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
				.CreateLogger();

            MainWindow mainWindow;

			bool consoleSwitch = false;
			bool versionSwitch = false;
			if (e.Args != null && e.Args.Length > 0) {
				foreach (string arg in e.Args) {
					if (arg.ToLower() == "--version") {
						versionSwitch = true;
					} else if (arg.ToLower() == "--console") {
						consoleSwitch = true;
					}
				}
			}

			if (versionSwitch) {
				Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
				Trace.WriteLine(version.ToString());
				string[] lines = {
					version.Major.ToString(),
					version.Minor.ToString(),
					version.Build.ToString(),
					version.Revision.ToString()
				};
				File.WriteAllLines("version.txt", lines);
				Shutdown();
			} else if (consoleSwitch) {
				mainWindow = new MainWindow(true); // show console
				mainWindow.Show();
			} else {
				mainWindow = new MainWindow(false); // do not show console
				mainWindow.Show();
			}
        }
    }
}
