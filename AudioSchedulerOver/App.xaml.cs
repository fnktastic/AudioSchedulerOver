using AudioSchedulerOver.Helper;
using AudioSchedulerOver.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AudioSchedulerOver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.InitLogger();//инициализация - требуется один раз в начале
            Logger.Log.Info(string.Format("Application Startup"));
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Exception.Message, e.Exception.StackTrace, e.Exception.Data));
        }
    }
}
