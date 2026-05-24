using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views.Controls
{
    public partial class PieChartControl : UserControl
    {
        public static readonly DependencyProperty SlicesProperty =
            DependencyProperty.Register(nameof(Slices), typeof(IEnumerable<PieSlice>),
                typeof(PieChartControl), new PropertyMetadata(null, (d, _) => ((PieChartControl)d).Redraw()));

        public IEnumerable<PieSlice> Slices
        {
            get => (IEnumerable<PieSlice>)GetValue(SlicesProperty);
            set => SetValue(SlicesProperty, value);
        }

        public PieChartControl() => InitializeComponent();

        private void Redraw()
        {
            if (PieCanvas == null) return;

            PieCanvas.Children.Clear();
            Legend.ItemsSource = null;

            if (Slices == null) return;

            var slices = Slices.ToList();
            Legend.ItemsSource = slices;

            double total = slices.Sum(s => s.Value);
            const double cx = 65, cy = 65, r = 60, holeR = 24;

            if (total == 0)
            {
                PutEllipse(cx, cy, r, new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)));
                PutEllipse(cx, cy, holeR, Brushes.White);
                return;
            }

            var active = slices.Where(s => s.Value > 0).ToList();

            if (active.Count == 1)
            {
                PutEllipse(cx, cy, r, active[0].Color);
                PutEllipse(cx, cy, holeR, Brushes.White);
                return;
            }

            double angle = -Math.PI / 2;
            foreach (var slice in active)
            {
                double sweep = slice.Value / total * 2 * Math.PI;
                double end = angle + sweep;

                PieCanvas.Children.Add(new Path
                {
                    Fill = slice.Color,
                    Data = Slice(cx, cy, r, angle, end)
                });
                angle = end;
            }

            PutEllipse(cx, cy, holeR, Brushes.White);
        }

        private void PutEllipse(double cx, double cy, double r, Brush fill)
        {
            var el = new Ellipse { Width = r * 2, Height = r * 2, Fill = fill };
            Canvas.SetLeft(el, cx - r);
            Canvas.SetTop(el, cy - r);
            PieCanvas.Children.Add(el);
        }

        private static Geometry Slice(double cx, double cy, double r, double start, double end)
        {
            var fig = new PathFigure { StartPoint = new Point(cx, cy), IsClosed = true };
            fig.Segments.Add(new LineSegment(
                new Point(cx + r * Math.Cos(start), cy + r * Math.Sin(start)), true));
            fig.Segments.Add(new ArcSegment(
                new Point(cx + r * Math.Cos(end), cy + r * Math.Sin(end)),
                new Size(r, r), 0,
                end - start > Math.PI,
                SweepDirection.Clockwise, true));
            fig.Segments.Add(new LineSegment(new Point(cx, cy), true));
            var geo = new PathGeometry();
            geo.Figures.Add(fig);
            return geo;
        }
    }
}
