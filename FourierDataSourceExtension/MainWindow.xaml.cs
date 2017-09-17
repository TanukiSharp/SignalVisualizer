using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace FourierDataSourceExtension
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly FourierDataSource dataSource;
        private readonly Action configChanged;
        private readonly ObservableCollection<SineWaveParametersViewModel> sineWaveParameters = new ObservableCollection<SineWaveParametersViewModel>();

        public ReadOnlyObservableCollection<SineWaveParametersViewModel> SineWaveParameters { get; }

        public MainWindow(FourierDataSource dataSource, Action configChanged)
        {
            InitializeComponent();

            this.dataSource = dataSource;
            this.configChanged = configChanged;

            DataContext = this;

            SineWaveParameters = new ReadOnlyObservableCollection<SineWaveParametersViewModel>(sineWaveParameters);
            AddCommand = new AnonymousCommand(OnAdd);
        }

        public void SetSineWaves(IEnumerable<XmlElement> elements)
        {
            sineWaveParameters.Clear();

            foreach (XmlElement element in elements)
                AddSineWaveParameters(element);
        }

        public ICommand AddCommand { get; }

        private void OnAdd(object ignore)
        {
            AddSineWaveParameters(null);
            configChanged();
        }

        private void AddSineWaveParameters(XmlElement element)
        {
            SineWaveParametersViewModel viewModel = null;

            Action remove = () =>
            {
                sineWaveParameters.Remove(viewModel);
                configChanged();
            };

            viewModel = new SineWaveParametersViewModel(element, configChanged, remove);

            sineWaveParameters.Add(viewModel);
        }
    }
}
