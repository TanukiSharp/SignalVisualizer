using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using SignalVisualizer.Contracts;

namespace FourierDataSourceExtension
{
    public class SineWaveParametersViewModel : ConfigurationViewModelBase
    {
        public SineWaveParameters Model { get; private set; }

        private bool isActive = true;
        public bool IsActive
        {
            get { return isActive; }
            set { SetValue(ref isActive, value); }
        }

        private string amplitude = "0";
        public string Amplitude
        {
            get { return amplitude; }
            set
            {
                if (SetValue(ref amplitude, value))
                    UpdateModel();
            }
        }

        private string phase = "0";
        public string Phase
        {
            get { return phase; }
            set
            {
                if (SetValue(ref phase, value))
                    UpdateModel();
            }
        }

        private string frequency = "0";
        public string Frequency
        {
            get { return frequency; }
            set
            {
                if (SetValue(ref frequency, value))
                    UpdateModel();
            }
        }

        public ICommand RemoveCommand { get; }

        public SineWaveParametersViewModel(Action configChanged, Action remove)
            : this(null, configChanged, remove)
        {
        }

        public SineWaveParametersViewModel(XmlElement xmlElement, Action configChanged, Action remove)
            : base(configChanged)
        {
            if (xmlElement != null)
                Load(xmlElement);

            UpdateModel();

            RemoveCommand = new AnonymousCommand(_ => remove());
        }

        private void UpdateModel()
        {
            Model = new SineWaveParameters(
                Compute(amplitude),
                Compute(phase),
                Compute(frequency)
            );
        }

        private double Compute(string str)
        {
            if (str == null)
                return 0.0;

            str = str.Trim();

            if (str.IndexOf('/') >= 0)
            {
                string[] parts = str.Split('/');
                if (parts.Length != 2)
                    return 0.0;

                if (double.TryParse(parts[0].Trim(), out double a) == false || double.TryParse(parts[1].Trim(), out double b) == false)
                    return 0.0;

                if (Math.Abs(b) < 1e-9)
                    return double.PositiveInfinity;

                return a / b;
            }

            double.TryParse(str, out double value);

            return value;
        }

        private void Load(XmlElement xmlElement)
        {
            isActive = XmlConvert.ToBoolean(xmlElement.Attributes["active"].Value);
            amplitude = xmlElement.Attributes["amp"].Value;
            phase = xmlElement.Attributes["phase"].Value;
            frequency = xmlElement.Attributes["freq"].Value;
        }

        public void Save(XmlWriter writer)
        {
            writer.WriteStartElement("parameter");
            writer.WriteAttributeString("active", XmlConvert.ToString(isActive));
            writer.WriteAttributeString("amp", amplitude);
            writer.WriteAttributeString("phase", phase);
            writer.WriteAttributeString("freq", frequency);
            writer.WriteEndElement();
        }
    }
}
