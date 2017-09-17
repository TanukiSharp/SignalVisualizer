using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Xml;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.DefaultExtensions
{
    [Export(typeof(ISignalFilterFactory))]
    public class LowPassFilterFactory : ISignalFilterFactory
    {
        public string FilterName { get; } = "Low Pass";
        public Guid UniqueIdentifier { get; } = new Guid("20d6a92d-037e-4814-94b2-0cac85b04365");

        public ISignalFilter ProduceSignalFilter()
        {
            return new LowPassFilter(this);
        }
    }

    public class LowPassFilter : SignalFilterBase, IDisposable
    {
        public LowPassFilter(ISignalFilterFactory signalFilterFactory)
            : base(signalFilterFactory)
        {
            SetSliderValue(0.05);
        }

        public void Dispose()
        {
            if (configWindow != null)
            {
                configWindow.Close();
                configWindow = null;
            }
        }

        private CutoffFilterConfigWindow.FilterMode mode;
        private double sliderValue;
        private double previousValue;
        private double previousTime = -1.0;

        private void SetMode(CutoffFilterConfigWindow.FilterMode mode)
        {
            if (this.mode != mode)
            {
                this.mode = mode;
                ConfigurationChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetSliderValue(double sliderValue)
        {
            if (this.sliderValue != sliderValue)
            {
                this.sliderValue = sliderValue;
                ConfigurationChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public override double ProcessValue(double time, double value)
        {
            if (previousTime < 0.0)
            {
                previousValue = value;
                previousTime = time;
                return value;
            }

            double cutoff;

            if (mode == CutoffFilterConfigWindow.FilterMode.Alpha)
                cutoff = sliderValue;
            else // if (mode == CutoffFilterConfigWindow.FilterMode.RC)
            {
                double dt = time - previousTime;
                previousTime = time;
                cutoff = sliderValue / (sliderValue + dt);
            }

            previousValue = previousValue + cutoff * (value - previousValue);

            return previousValue;
        }

        public override event EventHandler ConfigurationChanged;

        private CutoffFilterConfigWindow configWindow;

        public override void Configure()
        {
            if (configWindow == null)
            {
                configWindow = new CutoffFilterConfigWindow(SignalFilterFactory.FilterName);

                EventHandler closedHandler = null;
                closedHandler = (s, e) =>
                {
                    configWindow.ModeChanged -= ConfigWindow_ModeChanged;
                    configWindow.SliderValueChanged -= ConfigWindow_SliderValueChanged;
                    configWindow.Closed -= closedHandler;
                    configWindow = null;
                };

                configWindow.ModeChanged += ConfigWindow_ModeChanged;
                configWindow.SliderValueChanged += ConfigWindow_SliderValueChanged;
                configWindow.Closed += closedHandler;

                configWindow.SetInitialCutoff(0.0, 0.5, 0.005, sliderValue);
                configWindow.Owner = Application.Current.MainWindow;
            }

            configWindow.Show();
        }

        public override void Load(XmlElement element)
        {
            var node = (XmlElement)element.SelectSingleNode("lowpass");
            SetMode((CutoffFilterConfigWindow.FilterMode)XmlConvert.ToInt32(node.Attributes["mode"].Value));
            SetSliderValue(XmlConvert.ToDouble(node.Attributes["value"].Value));
        }

        public override void Save(XmlWriter writer)
        {
            writer.WriteStartElement("lowpass");
            writer.WriteAttributeString("mode", XmlConvert.ToString((int)mode));
            writer.WriteAttributeString("value", XmlConvert.ToString(sliderValue));
            writer.WriteEndElement();
        }

        private void ConfigWindow_ModeChanged(object sender, EventArgs e)
        {
            SetMode(configWindow.Mode);
        }

        private void ConfigWindow_SliderValueChanged(object sender, EventArgs e)
        {
            SetSliderValue(configWindow.SliderValue);
        }
    }
}
