using System;
using System.Windows;

namespace SignalVisualizer.DefaultExtensions
{
    /// <summary>
    /// Interaction logic for CutoffFilterConfigWindow.xaml
    /// </summary>
    public partial class CutoffFilterConfigWindow : Window
    {
        public CutoffFilterConfigWindow(string filterName, string dataSourceName, string componentName)
            : this($"{filterName} Filter Configuration - {dataSourceName} - {componentName}")
        {
        }

        public CutoffFilterConfigWindow(string title)
        {
            InitializeComponent();

            Title = title;

            sldValue.ValueChanged += SliderValue_ValueChanged;
            cboMode.SelectionChanged += ComboBoxMode_SelectionChanged;
        }

        protected override void OnClosed(EventArgs e)
        {
            sldValue.ValueChanged -= SliderValue_ValueChanged;
            cboMode.SelectionChanged -= ComboBoxMode_SelectionChanged;

            base.OnClosed(e);
        }

        private void ComboBoxMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Mode = (FilterMode)cboMode.SelectedIndex;
            txtSliderValue.Text = Mode.ToString();
        }

        private void SliderValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderValue = sldValue.Value;
        }

        public void SetInitialCutoff(double min, double max, double step, double cutoff)
        {
            sldValue.Minimum = min;
            sldValue.Maximum = max;
            sldValue.TickFrequency = step;

            sldValue.ValueChanged -= SliderValue_ValueChanged;
            sldValue.Value = cutoff;
            sldValue.ValueChanged += SliderValue_ValueChanged;
        }

        public enum FilterMode
        {
            Alpha,
            RC
        }

        private FilterMode mode;
        public FilterMode Mode
        {
            get { return mode; }
            private set
            {
                if (mode == value)
                    return;

                mode = value;

                ModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private double sliderValue;
        public double SliderValue
        {
            get { return sliderValue; }
            private set
            {
                if (sliderValue == value)
                    return;

                sliderValue = value;

                SliderValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ModeChanged;
        public event EventHandler SliderValueChanged;
    }
}
