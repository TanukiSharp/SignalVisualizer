using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Xml;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.DefaultExtensions
{
    [Export(typeof(ISignalFilterFactory))]
    public class HighPassFilterFactory : ISignalFilterFactory
    {
        public string FilterName { get; } = "High Pass";
        public Guid UniqueIdentifier { get; } = new Guid("4edc7d26-3c48-40c1-b296-156c18c22466");

        public ISignalFilter ProduceSignalFilter()
        {
            return new HighPassFilter(this);
        }
    }

    public class HighPassFilter : SignalFilterBase, IDisposable
    {
        public HighPassFilter(ISignalFilterFactory signalFilterFactory)
            : base(signalFilterFactory)
        {
            SetSliderValue(0.85);
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
        private double previousUnfilteredValue;
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
                previousUnfilteredValue = value;
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

            previousValue = cutoff * (previousValue + value - previousUnfilteredValue);

            previousUnfilteredValue = value;

            return previousValue;
        }

        public override event EventHandler ConfigurationChanged;

        private CutoffFilterConfigWindow configWindow;

        public override void Configure()
        {
            if (configWindow == null)
            {
                configWindow = new CutoffFilterConfigWindow("High Pass Filter Configuration");

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

                configWindow.SetInitialCutoff(0.0, 1.0, 0.01, sliderValue);

                configWindow.Owner = Application.Current.MainWindow;
            }

            configWindow.Show();
        }

        public override void Load(XmlElement element)
        {
            var node = (XmlElement)element.SelectSingleNode("highpass");
            SetMode((CutoffFilterConfigWindow.FilterMode)XmlConvert.ToInt32(node.Attributes["mode"].Value));
            SetSliderValue(XmlConvert.ToDouble(node.Attributes["value"].Value));
        }

        public override void Save(XmlWriter writer)
        {
            writer.WriteStartElement("highpass");
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
