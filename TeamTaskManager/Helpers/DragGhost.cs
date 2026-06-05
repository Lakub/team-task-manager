using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace TeamTaskManager.Helpers
{
    public class DragGhost : Adorner
    {
        private readonly VisualBrush _brush;
        private readonly Size _size;
        private readonly Point _offset;
        private Point _pos;

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT p);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        public DragGhost(UIElement root, UIElement source, Point clickOffset) : base(root)
        {
            _brush = new VisualBrush(source) { Stretch = Stretch.None };
            _size = source.RenderSize;
            _offset = clickOffset;
            IsHitTestVisible = false;
            Opacity = 0.92;
            Effect = new DropShadowEffect
            {
                BlurRadius = 16,
                Opacity = 0.45,
                ShadowDepth = 8,
                Direction = 270
            };
        }

        public void Follow()
        {
            GetCursorPos(out POINT p);
            var screen = new Point(p.X - _offset.X, p.Y - _offset.Y);
            _pos = AdornedElement.PointFromScreen(screen);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(_brush, null, new Rect(_pos, _size));
        }
    }
}
