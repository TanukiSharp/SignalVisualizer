using System;
using System.Collections.Generic;
using System.Windows;
using SignalVisualizer.Composition;
using SignalVisualizer.Contracts;
using SignalVisualizer.ViewModels;

namespace SignalVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IEnumerable<IDataSource> dataSources;
        private IEnumerable<ISignalFilterFactory> signalFilterFactories;

        private RootViewModel rootViewModel;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            string extensionsPath = Utility.UsePath("Extensions");

            var composer = new ExtensionsComposer();
            composer.AddPathLookup(new PathLookupInfo(extensionsPath, true, "*.dll"));

            await composer.InitializeAsync();

            signalFilterFactories = await composer.ComposeAsync<ISignalFilterFactory>();
            dataSources = await composer.ComposeAsync<IDataSource>();

            rootViewModel = new RootViewModel(dataSources, signalFilterFactories);

            DataContext = rootViewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (rootViewModel != null)
                rootViewModel.Dispose();
        }
    }
}
