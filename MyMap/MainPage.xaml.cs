using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyMap
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int numOfPoint = 10000;
        private SortedSet<Px> xs = new SortedSet<Px>();
        private SortedSet<Px> ys = new SortedSet<Px>();
        private Ellipse[] points;
        private Ellipse ellipse;
        public MainPage()
        {
            this.InitializeComponent();
            Random random = new Random();
            double width = whiteBoard.Width;
            double height = whiteBoard.Height;
            points = new Ellipse[numOfPoint];
            for (int i = 0; i < numOfPoint; i++)
            {
                Ellipse point = new Ellipse();
                point.Stroke = new SolidColorBrush(Colors.Blue);
                point.Fill = new SolidColorBrush(Colors.Blue);
                point.Height = 1;
                point.Width = 1;
                point.Margin = new Thickness(random.NextDouble() * width, random.NextDouble() * height, 0, 0);
                whiteBoard.Children.Add(point);

                points[i] = point;
                xs.Add(new Px(i, point.Margin.Left));
                ys.Add(new Px(i, point.Margin.Top));
            }
            ellipse = new Ellipse();
            ellipse.Stroke = new SolidColorBrush(Windows.UI.Colors.Green);
            ellipse.Width = 2 * R;
            ellipse.Height = 2 * R;
            whiteBoard.Children.Add(ellipse);
        }

        private List<Shape> results = new List<Shape>();
        private double R = 50;
        
        private void whiteBoard_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            foreach (var item in results)
            {
                item.Stroke = new SolidColorBrush(Colors.Blue);
                item.Fill = new SolidColorBrush(Colors.Blue);
            }
            Point point = e.GetCurrentPoint(whiteBoard).Position;
            ellipse.Margin = new Thickness(point.X - R, point.Y - R, 0, 0);
            SortedSet<Px> pxes = xs.GetViewBetween(new Px(-1, point.X - R), new Px(-1, point.X + R));
            SortedSet<Px> pyes = ys.GetViewBetween(new Px(-1, point.Y - R), new Px(-1, point.Y + R));
            foreach (var ps in pxes.Intersect(pyes, new PxComparer()))
            {
                Shape p = points[ps.Index];
                double distance = Math.Sqrt(Math.Pow(point.X - p.Margin.Left, 2) + Math.Pow(point.Y - p.Margin.Top, 2));
                if (distance <= R)
                {
                    p.Stroke = new SolidColorBrush(Colors.Red);
                    p.Fill = new SolidColorBrush(Colors.Red);
                    results.Add(p);
                }
            }
        }
    }

    public class PxComparer : IEqualityComparer<Px>
    {
        public bool Equals(Px x, Px y)
        {
            return x.Index == y.Index;
        }

        public int GetHashCode(Px obj)
        {
            return obj.Index.GetHashCode();
        }
    }

    public class Px : IComparable<Px>
    {
        public int Index { get; set; }
        public double Value { get; set; }

        public Px(int index, double value)
        {
            Index = index;
            Value = value;
        }

        public int CompareTo(Px other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}
