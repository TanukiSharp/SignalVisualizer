using System;
using System.Windows;

namespace SignalVisualizer.DefaultExtensions
{
    /// <summary>
    /// Interaction logic for HysteresisFilterConfigWindow.xaml
    /// </summary>
    public partial class HysteresisFilterConfigWindow : Window
    {
        private HysteresisFilter filter;

        public HysteresisFilterConfigWindow(HysteresisFilter filter)
        {
            InitializeComponent();

            this.filter = filter;
            InitializeValues(filter);

            DataContext = this;
        }

        internal void InitializeValues(HysteresisFilter filter)
        {
            maxValue = filter.maxValue;
            highTrigger = filter.highTrigger;
            lowTrigger = filter.lowTrigger;
            minValue = filter.minValue;
        }

        private double maxValue;
        public double MaxValue
        {
            get { return maxValue; }
            set
            {
                if (maxValue != value)
                {
                    maxValue = value;
                    UpdateSettings();
                }
            }
        }

        private double highTrigger;
        public double HighTrigger
        {
            get { return highTrigger; }
            set
            {
                if (highTrigger != value)
                {
                    highTrigger = value;
                    UpdateSettings();
                }
            }
        }

        private double lowTrigger;
        public double LowTrigger
        {
            get { return lowTrigger; }
            set
            {
                if (lowTrigger != value)
                {
                    lowTrigger = value;
                    UpdateSettings();
                }
            }
        }

        private double minValue;
        public double MinValue
        {
            get { return minValue; }
            set
            {
                if (minValue != value)
                {
                    minValue = value;
                    UpdateSettings();
                }
            }
        }

        private void UpdateSettings()
        {
            filter.UpdateSettings(maxValue, highTrigger, lowTrigger, minValue);
        }
    }
}
