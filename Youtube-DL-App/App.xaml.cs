﻿using System;
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
    }
}