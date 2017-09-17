using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Xml;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.DefaultExtensions
{
    [Export(typeof(ISignalFilterFactory))]
    public class HysteresisFilterFactory : ISignalFilterFactory
    {
        public string FilterName { get; } = "Hysteresis";
        public Guid UniqueIdentifier { get; } = new Guid("23777bb4-5984-4cd0-b99e-1edee9dab927");

        public ISignalFilter ProduceSignalFilter()
        {
            return new HysteresisFilter(this);
        }
    }

    public class HysteresisFilter : SignalFilterBase, IDisposable
    {
        public HysteresisFilter(ISignalFilterFactory signalFilterFactory)
            : base(signalFilterFactory)
        {
        }

        public void Dispose()
        {
            if (configWindow != null)
            {
                configWindow.Close();
                configWindow = null;
            }
        }

        internal double maxValue;
        internal double highTrigger;
        internal double lowTrigger;
        internal double minValue;

        internal void UpdateSettings(double maxValue, double highTrigger, double lowTrigger, double minValue)
        {
            this.maxValue = maxValue;
            this.highTrigger = highTrigger;
            this.lowTrigger = lowTrigger;
            this.minValue = minValue;

            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }

        private double outputSignalValue;

        public override double ProcessValue(double time, double value)
        {
            if (value >= highTrigger)
                outputSignalValue = maxValue;
            else if (value <= lowTrigger)
                outputSignalValue = minValue;

            return outputSignalValue;
        }

        public override event EventHandler ConfigurationChanged;

        private HysteresisFilterConfigWindow configWindow;

        public override void Configure()
        {
            if (configWindow == null)
            {
                configWindow = new HysteresisFilterConfigWindow(this);

                EventHandler closedHandler = null;
                closedHandler = (s, e) =>
                {
                    configWindow.Closed -= closedHandler;
                    configWindow = null;
                };

                configWindow.Closed += closedHandler;

                configWindow.Owner = Application.Current.MainWindow;
            }

            configWindow.Show();
        }

        public override void Load(XmlElement element)
        {
            var node = (XmlElement)element.SelectSingleNode("hysteresis");

            maxValue = XmlConvert.ToDouble(node.Attributes["maxValue"].Value);
            highTrigger = XmlConvert.ToDouble(node.Attributes["highTrigger"].Value);
            lowTrigger = XmlConvert.ToDouble(node.Attributes["lowTrigger"].Value);
            minValue = XmlConvert.ToDouble(node.Attributes["minValue"].Value);

            if (configWindow != null)
                configWindow.InitializeValues(this);
        }

        public override void Save(XmlWriter writer)
        {
            writer.WriteStartElement("hysteresis");
            writer.WriteAttributeString("maxValue", XmlConvert.ToString(maxValue));
            writer.WriteAttributeString("highTrigger", XmlConvert.ToString(highTrigger));
            writer.WriteAttributeString("lowTrigger", XmlConvert.ToString(lowTrigger));
            writer.WriteAttributeString("minValue", XmlConvert.ToString(minValue));
            writer.WriteEndElement();
        }
    }
}
