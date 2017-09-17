using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SignalVisualizer
{
    public delegate void CustomRenderRoutedEventHandler(object sender, CustomRenderRoutedEventArgs e);

    public class CustomRenderRoutedEventArgs : RoutedEventArgs
    {
        public DrawingContext DrawingContext { get; }

        public CustomRenderRoutedEventArgs(RoutedEvent routedEvent, DrawingContext drawingContext)
        {
            RoutedEvent = routedEvent;
            DrawingContext = drawingContext;
        }
    }

    public class ScaleBar : FrameworkElement
    {
        public static readonly DependencyProperty CustomDrawingContextProperty = DependencyProperty.Register(
            "CustomDrawingContext",
            typeof(DrawingContext),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background",
            typeof(Brush),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty LargeTickPenProperty = DependencyProperty.Register(
            "LargeTickPen",
            typeof(Pen),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(new Pen(Brushes.Black, 1.0), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SmallTickPenProperty = DependencyProperty.Register(
            "SmallTickPen",
            typeof(Pen),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(new Pen(Brushes.Gray, 1.0), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty LargeTickTopProperty = DependencyProperty.Register(
            "LargeTickTop",
            typeof(double),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(0.625, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty LargeTickBottomProperty = DependencyProperty.Register(
            "LargeTickBottom",
            typeof(double),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SmallTickTopProperty = DependencyProperty.Register(
            "SmallTickTop",
            typeof(double),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(0.75, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SmallTickBottomProperty = DependencyProperty.Register(
            "SmallTickBottom",
            typeof(double),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty DecimalCountRoundingProperty = DependencyProperty.Register(
            "DecimalCountRounding",
            typeof(int),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(6, null, CoerceDecimalCountRoundingPropertyValue));

        public static readonly DependencyProperty TextPositionOriginProperty = DependencyProperty.Register(
            "TextPositionOrigin",
            typeof(Point),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(new Point(0.5, 0.0), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TextPositionProperty = DependencyProperty.Register(
            "TextPosition",
            typeof(double),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground",
            typeof(Brush),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FontProperty = DependencyProperty.Register(
            "Font",
            typeof(Typeface),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(new Typeface("Meiryo"), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize",
            typeof(double),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(9.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StartUnitProperty = DependencyProperty.Register(
            "StartUnit",
            typeof(double),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty UnitsPerTickProperty = DependencyProperty.Register(
            "UnitsPerTick",
            typeof(double),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnUnitsPerTickPropertyChanged, CoerceUnitsPerTickPropertyValue));

        public static readonly DependencyProperty PointsPerTickProperty = DependencyProperty.Register(
            "PointsPerTick",
            typeof(double),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender, OnPointsPerTickPropertyChanged, CoercePointsPerTickPropertyValue));

        private static readonly DependencyPropertyKey AdjustedUnitsPerTickPropertyKey = DependencyProperty.RegisterReadOnly(
            "AdjustedUnitsPerTick",
            typeof(double),
            typeof(ScaleBar),
            new PropertyMetadata());
        public static readonly DependencyProperty AdjustedUnitsPerTickProperty = AdjustedUnitsPerTickPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey AdjustedPointsPerTickPropertyKey = DependencyProperty.RegisterReadOnly(
            "AdjustedPointsPerTick",
            typeof(double),
            typeof(ScaleBar),
            new PropertyMetadata());
        public static readonly DependencyProperty AdjustedPointsPerTickProperty = AdjustedPointsPerTickPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey PointsPerUnitPropertyKey = DependencyProperty.RegisterReadOnly(
            "PointsPerUnit",
            typeof(double),
            typeof(ScaleBar),
            new PropertyMetadata());
        public static readonly DependencyProperty PointsPerUnitProperty = PointsPerUnitPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey AdjustedPointsPerUnitPropertyKey = DependencyProperty.RegisterReadOnly(
            "AdjustedPointsPerUnit",
            typeof(double),
            typeof(ScaleBar),
            new PropertyMetadata());
        public static readonly DependencyProperty AdjustedPointsPerUnitProperty = AdjustedPointsPerUnitPropertyKey.DependencyProperty;

        public static readonly DependencyProperty IsAliasedProperty = DependencyProperty.Register(
            "IsAliased",
            typeof(bool),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnIsAliasedPropertyChanged));

        public static readonly DependencyProperty IsTextVisibleProperty = DependencyProperty.Register(
            "IsTextVisible",
            typeof(bool),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsSmallTickVisibleProperty = DependencyProperty.Register(
            "IsSmallTickVisible",
            typeof(bool),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsZoomingOnMouseWheelProperty = DependencyProperty.Register(
            "IsZoomingOnMouseWheel",
            typeof(bool),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty MouseWheelZoomCoeficientProperty = DependencyProperty.Register(
            "MouseWheelZoomCoeficient",
            typeof(double),
            typeof(ScaleBar),
            new FrameworkPropertyMetadata(1.1, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly RoutedEvent BeforeRenderEvent = EventManager.RegisterRoutedEvent(
            "BeforeRender",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ScaleBar));

        public static readonly RoutedEvent AfterRenderEvent = EventManager.RegisterRoutedEvent(
            "AfterRender",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ScaleBar));

        public static readonly RoutedEvent BeforeTicksRenderEvent = EventManager.RegisterRoutedEvent(
            "BeforeTicksRender",
            RoutingStrategy.Bubble,
            typeof(CustomRenderRoutedEventHandler),
            typeof(ScaleBar));

        public static readonly RoutedEvent AfterTicksRenderEvent = EventManager.RegisterRoutedEvent(
            "AfterTicksRender",
            RoutingStrategy.Bubble,
            typeof(CustomRenderRoutedEventHandler),
            typeof(ScaleBar));

        public ScaleBar()
        {
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnLoaded;

            SetValue(AdjustedUnitsPerTickPropertyKey, UnitsPerTick);
            SetValue(AdjustedPointsPerTickPropertyKey, PointsPerTick);
            RenderOptions.SetEdgeMode(this, IsAliased ? EdgeMode.Aliased : EdgeMode.Unspecified);
            InvalidateVisual();
        }

        public event RoutedEventHandler BeforeRender
        {
            add { AddHandler(BeforeRenderEvent, value); }
            remove { RemoveHandler(BeforeRenderEvent, value); }
        }

        public event RoutedEventHandler AfterRender
        {
            add { AddHandler(AfterRenderEvent, value); }
            remove { RemoveHandler(AfterRenderEvent, value); }
        }

        public event CustomRenderRoutedEventHandler BeforeTicksRender
        {
            add { AddHandler(BeforeTicksRenderEvent, value); }
            remove { RemoveHandler(BeforeTicksRenderEvent, value); }
        }

        public event CustomRenderRoutedEventHandler AfterTicksRender
        {
            add { AddHandler(AfterTicksRenderEvent, value); }
            remove { RemoveHandler(AfterTicksRenderEvent, value); }
        }

        private void RaiseBeforeRenderEvent()
        {
            RaiseEvent(new RoutedEventArgs(BeforeRenderEvent));
        }

        private void RaiseAfterRenderEvent()
        {
            RaiseEvent(new RoutedEventArgs(AfterRenderEvent));
        }

        private void RaiseBeforeTicksRenderEvent(DrawingContext drawingContext)
        {
            RaiseEvent(new CustomRenderRoutedEventArgs(BeforeTicksRenderEvent, drawingContext));
        }

        private void RaiseAfterTicksRenderEvent(DrawingContext drawingContext)
        {
            RaiseEvent(new CustomRenderRoutedEventArgs(AfterTicksRenderEvent, drawingContext));
        }

        public DrawingContext CustomDrawingContext
        {
            get { return (DrawingContext)GetValue(CustomDrawingContextProperty); }
            set { SetValue(CustomDrawingContextProperty, value); }
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public Pen LargeTickPen
        {
            get { return (Pen)GetValue(LargeTickPenProperty); }
            set { SetValue(LargeTickPenProperty, value); }
        }

        public Pen SmallTickPen
        {
            get { return (Pen)GetValue(SmallTickPenProperty); }
            set { SetValue(SmallTickPenProperty, value); }
        }

        /// <summary>
        /// Gets or sets the relative top (Y) coordinate of the drawn large ticks. This is a dependency property.
        /// </summary>
        /// <remarks>The coordinate is relative, that means 0.0 is top and 1.0 is bottom.
        /// The coordinate can be set to less than 0.0 or more than 1.0 where additional offset is needed.</remarks>
        public double LargeTickTop
        {
            get { return (double)GetValue(LargeTickTopProperty); }
            set { SetValue(LargeTickTopProperty, value); }
        }

        /// <summary>
        /// Gets or sets the relative bottom (Y) coordinate of the drawn large ticks. This is a dependency property.
        /// </summary>
        /// <remarks>The coordinate is relative, that means 0.0 is top and 1.0 is bottom.
        /// The coordinate can be set to less than 0.0 or more than 1.0 where additional offset is needed.</remarks>
        public double LargeTickBottom
        {
            get { return (double)GetValue(LargeTickBottomProperty); }
            set { SetValue(LargeTickBottomProperty, value); }
        }

        /// <summary>
        /// Gets or sets the relative top (Y) coordinate of the drawn small ticks. This is a dependency property.
        /// </summary>
        /// <remarks>The coordinate is relative, that means 0.0 is top and 1.0 is bottom.
        /// The coordinate can be set to less than 0.0 or more than 1.0 where additional offset is needed.</remarks>
        public double SmallTickTop
        {
            get { return (double)GetValue(SmallTickTopProperty); }
            set { SetValue(SmallTickTopProperty, value); }
        }

        /// <summary>
        /// Gets or sets the relative bottom (Y) coordinate of the drawn small ticks. This is a dependency property.
        /// </summary>
        /// <remarks>The coordinate is relative, that means 0.0 is top and 1.0 is bottom.
        /// The coordinate can be set to less than 0.0 or more than 1.0 where additional offset is needed.</remarks>
        public double SmallTickBottom
        {
            get { return (double)GetValue(SmallTickBottomProperty); }
            set { SetValue(SmallTickBottomProperty, value); }
        }

        public int DecimalCountRounding
        {
            get { return (int)GetValue(DecimalCountRoundingProperty); }
            set { SetValue(DecimalCountRoundingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the center point of drawn text, relative to the bounds of the drawn text itself. This is a dependency property.
        /// </summary>
        /// <remarks>Each coordinate axis can be set to less than 0.0 or more than 1.0 where additional offset is needed.</remarks>
        public Point TextPositionOrigin
        {
            get { return (Point)GetValue(TextPositionOriginProperty); }
            set { SetValue(TextPositionOriginProperty, value); }
        }

        /// <summary>
        /// Gets or sets the relative top (Y) coordinate of the center of the drawn text. This is a dependency property.
        /// </summary>
        /// <remarks>The coordinate is relative, that means 0.0 is top and 1.0 is bottom.
        /// The coordinate can be set to less than 0.0 or more than 1.0 where additional offset is needed.</remarks>
        public double TextPosition
        {
            get { return (double)GetValue(TextPositionProperty); }
            set { SetValue(TextPositionProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Typeface Font
        {
            get { return (Typeface)GetValue(FontProperty); }
            set { SetValue(FontProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public double StartUnit
        {
            get { return (double)GetValue(StartUnitProperty); }
            set { SetValue(StartUnitProperty, value); }
        }

        public double UnitsPerTick
        {
            get { return (double)GetValue(UnitsPerTickProperty); }
            set { SetValue(UnitsPerTickProperty, value); }
        }

        public double AdjustedUnitsPerTick
        {
            get { return (double)GetValue(AdjustedUnitsPerTickProperty); }
            private set { SetValue(AdjustedUnitsPerTickPropertyKey, value); }
        }

        public double PointsPerTick
        {
            get { return (double)GetValue(PointsPerTickProperty); }
            set { SetValue(PointsPerTickProperty, value); }
        }

        public double AdjustedPointsPerTick
        {
            get { return (double)GetValue(AdjustedPointsPerTickProperty); }
            private set { SetValue(AdjustedPointsPerTickPropertyKey, value); }
        }

        public double PointsPerUnit
        {
            get { return (double)GetValue(PointsPerUnitProperty); }
            private set { SetValue(PointsPerUnitPropertyKey, value); }
        }

        public double AdjustedPointsPerUnit
        {
            get { return (double)GetValue(AdjustedPointsPerUnitProperty); }
            private set { SetValue(AdjustedPointsPerUnitPropertyKey, value); }
        }

        public bool IsAliased
        {
            get { return (bool)GetValue(IsAliasedProperty); }
            set { SetValue(IsAliasedProperty, value); }
        }

        public bool IsTextVisible
        {
            get { return (bool)GetValue(IsTextVisibleProperty); }
            set { SetValue(IsTextVisibleProperty, value); }
        }

        public bool IsSmallTickVisible
        {
            get { return (bool)GetValue(IsSmallTickVisibleProperty); }
            set { SetValue(IsSmallTickVisibleProperty, value); }
        }

        public bool IsZoomingOnMouseWheel
        {
            get { return (bool)GetValue(IsZoomingOnMouseWheelProperty); }
            set { SetValue(IsZoomingOnMouseWheelProperty, value); }
        }

        public double MouseWheelZoomCoeficient
        {
            get { return (double)GetValue(MouseWheelZoomCoeficientProperty); }
            set { SetValue(MouseWheelZoomCoeficientProperty, value); }
        }

        private static void OnUnitsPerTickPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ScaleBar scalebar = (ScaleBar)sender;

            scalebar.AdjustedUnitsPerTick = AdjustUnitInterval((double)e.NewValue);
            scalebar.AdjustedPointsPerTick = scalebar.PointsPerTick * scalebar.AdjustedUnitsPerTick / scalebar.UnitsPerTick;
            scalebar.AdjustedPointsPerUnit = scalebar.AdjustedPointsPerTick / scalebar.AdjustedUnitsPerTick;
            scalebar.PointsPerUnit = scalebar.PointsPerTick / scalebar.UnitsPerTick;
        }

        private static void OnPointsPerTickPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ScaleBar scalebar = (ScaleBar)sender;

            scalebar.AdjustedPointsPerTick = scalebar.PointsPerTick * scalebar.AdjustedUnitsPerTick / scalebar.UnitsPerTick;
            scalebar.AdjustedPointsPerUnit = scalebar.AdjustedPointsPerTick / scalebar.AdjustedUnitsPerTick;
            scalebar.PointsPerUnit = scalebar.PointsPerTick / scalebar.UnitsPerTick;
        }

        private static void OnIsAliasedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RenderOptions.SetEdgeMode(sender, (bool)e.NewValue ? EdgeMode.Aliased : EdgeMode.Unspecified);
        }

        private static object CoerceUnitsPerTickPropertyValue(DependencyObject sender, object value)
        {
            return Math.Max(1e-12, (double)value);
        }

        private static object CoercePointsPerTickPropertyValue(DependencyObject sender, object value)
        {
            return Math.Max(10.0, (double)value);
        }

        private static object CoerceDecimalCountRoundingPropertyValue(DependencyObject sender, object value)
        {
            return Math.Max(0, 12);
        }

        public double GetPointAt(double unit)
        {
            return ((unit - StartUnit) * AdjustedPointsPerTick) / AdjustedUnitsPerTick;
        }

        public double GetUnitAt(double point)
        {
            return StartUnit + (point * AdjustedUnitsPerTick) / AdjustedPointsPerTick;
        }

        public void SetUnitAt(double unit, double point)
        {
            StartUnit = unit - (point * AdjustedUnitsPerTick) / AdjustedPointsPerTick;
            InvalidateVisual();
        }

        public void SetUnitsPerTickAt(double unitsPerTick, double point)
        {
            double unit = GetUnitAt(point);
            UnitsPerTick = unitsPerTick;
            SetUnitAt(unit, point);
        }

        public void SetPointsPerTickAt(double pointsPerTick, double point)
        {
            double unit = GetUnitAt(point);
            PointsPerTick = pointsPerTick;
            SetUnitAt(unit, point);
        }

        protected Pen largeTickPen;
        protected Pen smallTickPen;

        protected Point textPositionOrigin;
        protected double textPosition;
        protected bool isTextVisible;
        protected Brush foreground;
        protected Typeface font;
        protected double fontSize;

        protected double actualWidth;
        protected double actualHeight;

        protected double largeTickTopPosition;
        protected double largeTickBottomPosition;
        protected double smallTickTopPosition;
        protected double smallTickBottomPosition;

        protected override void OnRender(DrawingContext localDrawingContext)
        {
            DrawingContext drawingContext = CustomDrawingContext ?? localDrawingContext;

            if (AdjustedPointsPerTick == 0)
                SetValue(AdjustedPointsPerTickPropertyKey, PointsPerTick);

            actualWidth = ActualWidth;
            actualHeight = ActualHeight;

            largeTickTopPosition = actualHeight * LargeTickTop;
            largeTickBottomPosition = actualHeight * LargeTickBottom;
            smallTickTopPosition = actualHeight * SmallTickTop;
            smallTickBottomPosition = actualHeight * SmallTickBottom;

            largeTickPen = LargeTickPen;
            smallTickPen = SmallTickPen;

            bool isSmallTickVisible = IsSmallTickVisible;

            textPositionOrigin = TextPositionOrigin;
            textPosition = TextPosition;

            isTextVisible = IsTextVisible;
            if (isTextVisible)
            {
                foreground = Foreground;
                font = Font;
                fontSize = FontSize;
            }

            double adjustedUnitsPerTick = AdjustedUnitsPerTick;
            double adjustedPointsPerTick = AdjustedPointsPerTick;
            int decimalCountRounding = DecimalCountRounding;

            double currentUnit = (int)(StartUnit / adjustedUnitsPerTick) * adjustedUnitsPerTick;
            double currentPoint = ((currentUnit - StartUnit) / adjustedUnitsPerTick) * adjustedPointsPerTick;

            if (StartUnit >= 0.0)
            {
                currentPoint += adjustedPointsPerTick;
                currentUnit += adjustedUnitsPerTick;
                currentUnit = Math.Round(currentUnit, decimalCountRounding);
            }

            RaiseBeforeRenderEvent();

            drawingContext.DrawRectangle(Background, null, new Rect(0.0, 0.0, actualWidth, actualHeight));

            RaiseBeforeTicksRenderEvent(drawingContext);

            if (isSmallTickVisible)
            {
                for (int i = 0; i < 9; i++)
                {
                    double smallLeft = currentPoint - ((i + 1) * adjustedPointsPerTick) * 0.1;
                    if (smallLeft < 0.0)
                        break;
                    DrawSmallTick(drawingContext, smallLeft);
                }
            }

            while (currentPoint < actualWidth)
            {
                DrawLargeTick(drawingContext, currentUnit, currentPoint + 1.0);

                if (isSmallTickVisible)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        double smallLeft = currentPoint + ((i + 1) * adjustedPointsPerTick) * 0.1;
                        if (smallLeft > actualWidth)
                            break;
                        DrawSmallTick(drawingContext, smallLeft + 1.0);
                    }
                }

                currentPoint += adjustedPointsPerTick;
                currentUnit += adjustedUnitsPerTick;
                currentUnit = Math.Round(currentUnit, decimalCountRounding);
            }

            RaiseAfterTicksRenderEvent(drawingContext);

            RaiseAfterRenderEvent();
        }

        private static double AdjustUnitInterval(double value)
        {
            // computing cannot be done on negative values
            bool negative = (value <= 0.0f);
            if (negative)
                value = -value;

            double L = Math.Log10(value);
            double L0 = Math.Pow(10.0, Math.Floor(L));

            double result;

            L = value / L0;
            if (L < (1.0f + 2.0f) * 0.5f) result = L0;
            else if (L < (2.0f + 5.0f) * 0.5f) result = L0 * 2.0f;
            else if (L < (5.0f + 10.0f) * 0.5f) result = L0 * 5.0f;
            else result = L0 * 10.0f;

            if (negative)
                result = -result;

            return result;
        }

        protected virtual void DrawLargeTick(DrawingContext drawingContext, double unit, double position)
        {
            if (isTextVisible)
            {
                FormattedText ft = new FormattedText(unit.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, font, fontSize, foreground);
                drawingContext.DrawText(ft, new Point(position - ft.Width * textPositionOrigin.X, (textPosition * actualHeight) - (ft.Height * textPositionOrigin.Y)));
            }

            drawingContext.DrawLine(largeTickPen, new Point(position, largeTickTopPosition), new Point(position, largeTickBottomPosition));
        }

        protected virtual void DrawSmallTick(DrawingContext drawingContext, double position)
        {
            drawingContext.DrawLine(smallTickPen, new Point(position, smallTickTopPosition), new Point(position, smallTickBottomPosition));
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (IsZoomingOnMouseWheel)
            {
                double coef = MouseWheelZoomCoeficient;
                if (e.Delta < 0.0)
                    coef = 1.0 / MouseWheelZoomCoeficient;

                Point pos = e.GetPosition(this);

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    SetPointsPerTickAt(PointsPerTick * coef, pos.X);
                else
                    SetUnitsPerTickAt(UnitsPerTick / coef, pos.X);
            }
        }
    }
}
