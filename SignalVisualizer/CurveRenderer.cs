using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SignalVisualizer
{
    public struct TimedData
    {
        public double ElapsedTime;
        public double Value;
    }

    public class CurveRenderer : Control
    {
        public CurveRenderer()
        {
            ClipToBounds = true;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
            this.Unloaded += CurveRenderer_Unloaded;
        }

        private void CurveRenderer_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            this.Unloaded -= CurveRenderer_Unloaded;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        public double MinimumRange
        {
            get { return (double)GetValue(MinimumRangeProperty); }
            set { SetValue(MinimumRangeProperty, value); }
        }

        public static readonly DependencyProperty MinimumRangeProperty = DependencyProperty.Register(
            "MinimumRange",
            typeof(double),
            typeof(CurveRenderer));

        public double MaximumRange
        {
            get { return (double)GetValue(MaximumRangeProperty); }
            set { SetValue(MaximumRangeProperty, value); }
        }

        public static readonly DependencyProperty MaximumRangeProperty = DependencyProperty.Register(
            "MaximumRange",
            typeof(double),
            typeof(CurveRenderer));

        public double PointsPerTick
        {
            get { return (double)GetValue(PointsPerTickProperty); }
            set { SetValue(PointsPerTickProperty, value); }
        }

        public static readonly DependencyProperty PointsPerTickProperty = DependencyProperty.Register(
            "PointsPerTick",
            typeof(double),
            typeof(CurveRenderer),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double UnitPerValue
        {
            get { return (double)GetValue(UnitPerValueProperty); }
            set { SetValue(UnitPerValueProperty, value); }
        }

        public static readonly DependencyProperty UnitPerValueProperty = DependencyProperty.Register(
            "UnitPerValue",
            typeof(double),
            typeof(CurveRenderer),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double UnitsPerSecond
        {
            get { return (double)GetValue(UnitsPerSecondProperty); }
            set { SetValue(UnitsPerSecondProperty, value); }
        }

        public static readonly DependencyProperty UnitsPerSecondProperty = DependencyProperty.Register(
            "UnitsPerSecond",
            typeof(double),
            typeof(CurveRenderer),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public RingBuffer<TimedData> CurveData
        {
            get { return (RingBuffer<TimedData>)GetValue(CurveDataProperty); }
            set { SetValue(CurveDataProperty, value); }
        }

        public static readonly DependencyProperty CurveDataProperty = DependencyProperty.Register(
            "CurveData",
            typeof(RingBuffer<TimedData>),
            typeof(CurveRenderer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        private static Pen signalPen = new Pen(Brushes.Blue, 1.0);
        private static Pen gradePen = new Pen(Brushes.WhiteSmoke, 1.0);

        static CurveRenderer()
        {
            if (signalPen.CanFreeze)
                signalPen.Freeze();

            if (gradePen.CanFreeze)
                gradePen.Freeze();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);

            drawingContext.DrawRectangle(Background, null, new Rect(0.0, 0.0, ActualWidth, ActualHeight));

            RingBuffer<TimedData> ringBuffer = CurveData;

            if (ringBuffer == null)
                return;

            double width = ActualWidth;
            double height = ActualHeight;
            double halfHeight = height / 2.0;

            double pointsPerTick = PointsPerTick;
            double unitsPerValue = pointsPerTick / UnitPerValue;
            double unitsPerSecond = UnitsPerSecond;

            double maxRange = MaximumRange;
            double minRange = MinimumRange;

            bool first = true;
            double firstTime = 0.0;
            var prevData = new TimedData();

            double x1 = width;

            double k = -minRange / (maxRange - minRange);
            double zeroTop = k * height - 1.0;

            for (double top = zeroTop; top < height; top += pointsPerTick)
                drawingContext.DrawLine(gradePen, new Point(0.0, top), new Point(width, top));
            for (double top = zeroTop; top > 0.0; top -= pointsPerTick)
                drawingContext.DrawLine(gradePen, new Point(0.0, top), new Point(width, top));

            ringBuffer.BeginReverseEnumerate();

            try
            {
                TimedData timedData;

                while (ringBuffer.GetReverseEnumerateValue(out timedData))
                {
                    if (first)
                    {
                        prevData = timedData;
                        firstTime = timedData.ElapsedTime;
                        first = false;
                        continue;
                    }

                    double y1 = height - 1.0 - ((prevData.Value * unitsPerValue) + zeroTop);
                    double y2 = height - 1.0 - ((timedData.Value * unitsPerValue) + zeroTop);

                    double dt = firstTime - prevData.ElapsedTime;
                    double x2 = width - dt * unitsPerSecond;

                    drawingContext.DrawLine(signalPen, new Point(x1, y1), new Point(x2, y2));

                    prevData = timedData;
                    x1 = x2;
                }
            }
            finally
            {
                ringBuffer.EndReverseEnumerate();
            }

            //foreach (TimedData timedData in ringBuffer.ReverseEnumerate())
            //{
            //    if (first)
            //    {
            //        prevData = timedData;
            //        firstTime = timedData.ElapsedTime;
            //        first = false;
            //        continue;
            //    }

            //    double y1 = height - 1.0 - ((prevData.Value * unitsPerValue) + zeroTop);
            //    double y2 = height - 1.0 - ((timedData.Value * unitsPerValue) + zeroTop);

            //    double dt = firstTime - prevData.ElapsedTime;
            //    double x2 = width - dt * unitsPerSecond;

            //    drawingContext.DrawLine(signalPen, new Point(x1, y1), new Point(x2, y2));

            //    prevData = timedData;
            //    x1 = x2;
            //}
        }
    }
}
