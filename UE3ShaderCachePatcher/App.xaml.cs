using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using BespokeFusion;
using Microsoft.Extensions.DependencyInjection;

#if DEBUG
using ShowMeTheXAML;
#endif

namespace UE3ShaderCachePatcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        private BindingListener _listener;

        private void OnDispatcherUnhandledException(
            object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version?.ToString(4);
            var error = e.Exception.Message;
            var msg = "" +
                      "Unhandled exception!\n\n" +
                      "Please report this error at: https://github.com/tuokri/UE3ShaderCachePatcher/issues\n\n" +
                      $"App Version: {ver}\n\n" +
                      "Error details:\n" +
                      $"{error}";

            MaterialMessageBox.ShowError(msg, "Error");

            e.Handled = true;
            Shutdown();
        }

        public App()
        {
            _listener = new BindingListener(
                TraceOptions.Callstack
                | TraceOptions.DateTime
                | TraceOptions.LogicalOperationStack
                | TraceOptions.ProcessId
                | TraceOptions.ThreadId
                | TraceOptions.Timestamp);

            Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ObjectDataModel>();
            services.AddSingleton<ObjectViewModel>();
            services.AddSingleton<MainWindow>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
#if DEBUG
            XamlDisplay.Init();
#endif

            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }
    }
}
