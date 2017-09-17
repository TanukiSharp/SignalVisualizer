using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.ViewModels
{
    public class SignalViewViewModel : SignalViewViewModelBase
    {
        private RingBuffer<TimedData> ringBuffer;
        public RingBuffer<TimedData> RingBuffer
        {
            get { return ringBuffer; }
            private set { SetValue(ref ringBuffer, value); }
        }

        private static Stopwatch stopwatch = Stopwatch.StartNew();

        private int signalSampleCount;
        public int SignalSampleCount
        {
            get { return signalSampleCount; }
            set
            {
                if (SetDataValue(ref signalSampleCount, value))
                {
                    RingBuffer<TimedData> oldRingBuffer = RingBuffer;
                    RingBuffer = new RingBuffer<TimedData>(SignalSampleCount);
                    if (oldRingBuffer != null)
                        RingBuffer.Write(oldRingBuffer.ToArray());
                }
            }
        }

        public SignalViewViewModel(RootViewModel root, SignalViewContainerViewModel parent, IDataSource dataSource, ISignalView signalView)
            : base(root, parent, dataSource, signalView)
        {
            UnitsPerSecond = 100.0;
            SignalSampleCount = 300;

            MinimumRange = -1.0;
            MaximumRange = +1.0;
            UnitsPerTick = 0.5;
        }

        protected override IDisposable OnSignalViewSubscription(ISignalView signalView)
        {
            return signalView.Subscribe(new AnonymousObserver<double>(OnSignalViewValue, OnSignalViewCompleted, OnSignalViewError));
        }

        private void OnSignalViewValue(double value)
        {
            double elapsedTime = stopwatch.Elapsed.TotalSeconds;

            lock (Parent.SignalFiltersPipeline.SignalFilters)
            {
                foreach (SignalFilterViewModel filter in Parent.SignalFiltersPipeline.SignalFilters)
                    value = filter.ProcessValue(elapsedTime, value);
            }

            RingBuffer.Write(new TimedData
            {
                ElapsedTime = elapsedTime,
                Value = value,
            });
        }

        private void OnSignalViewError(Exception ex)
        {
            Error = ex;
        }

        private void OnSignalViewCompleted()
        {
        }

        private double minimumRange;
        public double MinimumRange
        {
            get { return minimumRange; }
            set
            {
                if (SetDataValue(ref minimumRange, value))
                    RecomputeScaleBarValues();
            }
        }

        private double marginedMinimumRange;
        public double MarginedMinimumRange
        {
            get { return marginedMinimumRange; }
            private set { SetValue(ref marginedMinimumRange, value); }
        }

        private double maximumRange;
        public double MaximumRange
        {
            get { return maximumRange; }
            set
            {
                if (SetDataValue(ref maximumRange, value))
                    RecomputeScaleBarValues();
            }
        }

        private double marginedMaximumRange;
        public double MarginedMaximumRange
        {
            get { return marginedMaximumRange; }
            private set { SetValue(ref marginedMaximumRange, value); }
        }

        private double unitsPerTick;
        public double UnitsPerTick
        {
            get { return unitsPerTick; }
            set
            {
                if (SetDataValue(ref unitsPerTick, value))
                    RecomputeScaleBarValues();
            }
        }

        private void RecomputeScaleBarValues()
        {
            double range = Math.Abs(MaximumRange - MinimumRange);

            MarginedMinimumRange = MinimumRange - range * 0.1;
            MarginedMaximumRange = MaximumRange + range * 0.1;

            PointsPerTick = (ViewHeight * UnitsPerTick) / Math.Abs(MarginedMaximumRange - MarginedMinimumRange);
        }

        private double pointsPerTick;
        public double PointsPerTick
        {
            get { return pointsPerTick; }
            private set { SetValue(ref pointsPerTick, value); }
        }

        private double unitsPerSecond;
        public double UnitsPerSecond
        {
            get { return unitsPerSecond; }
            set { SetDataValue(ref unitsPerSecond, value); }
        }

        protected override void OnLoadLayout(XmlElement viewNode)
        {
            XmlAttribute attr;

            try
            {
                attr = viewNode.Attributes["MinRange"];
                MinimumRange = XmlConvert.ToDouble(attr.Value);
            }
            catch { }

            try
            {
                attr = viewNode.Attributes["MaxRange"];
                MaximumRange = XmlConvert.ToDouble(attr.Value);
            }
            catch { }

            try
            {
                attr = viewNode.Attributes["UnitsPerTick"];
                UnitsPerTick = XmlConvert.ToDouble(attr.Value);
            }
            catch { }

            try
            {
                attr = viewNode.Attributes["UnitsPerSecond"];
                UnitsPerSecond = XmlConvert.ToDouble(attr.Value);
            }
            catch { }

            try
            {
                attr = viewNode.Attributes["SampleCount"];
                SignalSampleCount = XmlConvert.ToInt32(attr.Value);
            }
            catch { }
        }

        protected override void OnSaveLayout(XmlTextWriter writer)
        {
            writer.WriteAttributeString("MinRange", XmlConvert.ToString(MinimumRange));
            writer.WriteAttributeString("MaxRange", XmlConvert.ToString(MaximumRange));
            writer.WriteAttributeString("UnitsPerTick", XmlConvert.ToString(UnitsPerTick));

            writer.WriteAttributeString("UnitsPerSecond", XmlConvert.ToString(UnitsPerSecond));
            writer.WriteAttributeString("SampleCount", XmlConvert.ToString(SignalSampleCount));
        }
    }
}
