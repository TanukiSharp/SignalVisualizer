using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using SignalVisualizer.Contracts;

namespace FourierDataSourceExtension
{
    public struct SineWaveParameters
    {
        public static readonly SineWaveParameters Identity = new SineWaveParameters(1.0, 0.0, 1.0);

        public readonly double Amplitude;
        public readonly double Phase;
        public readonly double Frequency;

        public SineWaveParameters(double amplitude, double phase, double frequency)
        {
            Amplitude = amplitude;
            Phase = phase;
            Frequency = frequency;
        }

        public double ProduceValue(double theta)
        {
            return Amplitude * Math.Sin((theta + Phase) * Frequency);
        }
    }

    [Export(typeof(IDataSource))]
    public class FourierDataSource : DataSourceBase
    {
        public override string Name { get; } = "Fourrier";
        public override uint Version { get; } = 1;
        public override Guid UniqueIdentifier { get; } = new Guid("b925976e-934f-4c73-b666-4480d514b9ca");
        public override string[] ComponentNames { get; } = new string[] { "Signal" };

        private MainWindow window;

        public FourierDataSource()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                window = new MainWindow(this, OnConfigurationChanged)
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };

                window.Closing += (ss, ee) =>
                {
                    ee.Cancel = true;
                    window.Visibility = Visibility.Collapsed;
                };
            });
        }

        public override async Task Start(CancellationToken cancellationToken)
        {
            double value;
            var array = new double[1];
            var stopwatch = Stopwatch.StartNew();

            while (cancellationToken.IsCancellationRequested == false)
            {
                double theta = stopwatch.ElapsedMilliseconds / 1000.0;

                value = 0.0;
                foreach (SineWaveParametersViewModel vm in window.SineWaveParameters)
                {
                    if (vm.IsActive)
                        value += vm.Model.ProduceValue(theta);
                }

                array[0] = value;

                NotifyNext(array);

                await Task.Delay(20);
            }
        }

        private void OnConfigurationChanged()
        {
            OnConfigurationChanged(EventArgs.Empty);
        }

        public override void Configure()
        {
            base.Configure();
            window.Visibility = Visibility.Visible;
        }

        public override void Load(XmlElement element)
        {
            window.SetSineWaves(element.ChildNodes.OfType<XmlElement>());
        }

        public override void Save(XmlWriter writer)
        {
            writer.WriteStartElement("parameters");
            foreach (SineWaveParametersViewModel vm in window.SineWaveParameters)
                vm.Save(writer);
            writer.WriteEndElement();
        }
    }
}
