using System;
using System.Runtime;
using System.Windows;

namespace SignalVisualizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ProfileOptimization.SetProfileRoot(AppDomain.CurrentDomain.BaseDirectory);
            ProfileOptimization.StartProfile("Startup.Profile");

            base.OnStartup(e);
        }
    }
}
