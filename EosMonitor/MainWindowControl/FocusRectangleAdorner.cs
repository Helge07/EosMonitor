using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace EosMonitor
{
    // Class FocusRectangleAdorner:  Defines the adorner for the focus rectangle
    public class FocusRectangleAdorner : Adorner
    {
        // Call the base class constructor.
        public FocusRectangleAdorner(UIElement adornedElement) : base(adornedElement)
        {
            VisualBrush _brush = new(adornedElement);

            _child = new Rectangle();
            _child.Width = adornedElement.RenderSize.Width;
            _child.Height = adornedElement.RenderSize.Height;

            DoubleAnimation animation = new(0.3, 1, new Duration(TimeSpan.FromSeconds(1)));
            animation.AutoReverse = true;
            animation.RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
            _brush.BeginAnimation(Brush.OpacityProperty, animation);

            _child.Fill = _brush;
        }

        // Override the OnRender method
        protected override void OnRender(DrawingContext drawingContext)
        {
            // Get a rectangle that represents the desired size of the rendered element
            // after the rendering pass.  
            Rect adornedElementRect = new(AdornedElement.DesiredSize);

            // Draw adorner:  green rectangle
            SolidColorBrush renderBrush = new(Colors.Green);
            renderBrush.Opacity = 0.2;
            Pen renderPen = new(new SolidColorBrush(Colors.Navy), 1.5);
            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);
        }
        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(finalSize));
            return finalSize;
        }
        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
        public double LeftOffset
        {
            get { return _leftOffset; }
            set { _leftOffset = value; UpdatePosition(); }
        }
        public double TopOffset
        {
            get { return _topOffset; }
            set { _topOffset = value; UpdatePosition(); }
        }
        private void UpdatePosition()
        {
            AdornerLayer? adornerLayer = Parent as AdornerLayer;
            adornerLayer?.Update(AdornedElement);
        }
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
            return result;
        }
        private readonly Rectangle _child;
        private double _leftOffset = 0;
        private double _topOffset = 0;
    }
}