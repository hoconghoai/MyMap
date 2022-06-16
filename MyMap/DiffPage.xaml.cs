using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MyMap
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DiffPage : Page
    {
        private int numOfPoint = 1000000;
        private int maxX = 10000;
        private int maxY = 10000;
        private int R = 100;
        private PointX[] points;
        private Random random = new Random();
        public DiffPage()
        {
            this.InitializeComponent();
            points = new PointX[numOfPoint];
            for (int i = 0; i < numOfPoint; i++)
            {
                points[i] = new PointX(random.Next(maxX), random.Next(maxY));
            }
        }
        private PointX TargetPoint;
        private void btnNext_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TargetPoint = new PointX(random.Next(maxX), random.Next(maxY));
            
            Task.Run(async () => {
                //DateTime start = DateTime.Now;
                Stopwatch stopwatch = Stopwatch.StartNew();
                int numOfPoint = Way1();
                stopwatch.Stop();
                //DateTime end = DateTime.Now;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    txt1.Text = txt1.Text + "\r\n" + BuildResult(stopwatch, TargetPoint, numOfPoint);
                });
            });

            Task.Run(async () => {
                //DateTime start = DateTime.Now;
                Stopwatch stopwatch = Stopwatch.StartNew();
                int numOfPoint = Way2();
                stopwatch.Stop();
                //DateTime end = DateTime.Now;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    txt2.Text = txt2.Text + "\r\n" + BuildResult(stopwatch, TargetPoint, numOfPoint);
                });
            });

            Task.Run(async () => {
                Stopwatch stopwatch = Stopwatch.StartNew();
                int numOfPoint = Way3();
                stopwatch.Stop();
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    txt3.Text = txt3.Text + "\r\n" + BuildResult(stopwatch, TargetPoint, numOfPoint);
                });
            });
        }

        private string BuildResult(Stopwatch stopwatch, PointX target, int numOfResult)
        {
            return string.Format("X: {0}, Y: {1}; NumOfResult: {2}; Duration: {3} ms", target.X, target.Y, numOfResult,
                stopwatch.ElapsedMilliseconds);
        }

        //private string BuildResult(DateTime start, DateTime end, PointX target, int numOfResult)
        //{
        //    TimeSpan timeSpan = end - start;
        //    return string.Format("X: {0}, Y: {1}; NumOfResult: {2}; Duration: {3}:{4}:{5}:{6}", target.X, target.Y, numOfResult, 
        //        timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        //}

        private void BuildLog(string key, DateTime start, DateTime end)
        {
            //TimeSpan timeSpan = end - start;
            //Debug.WriteLine(string.Format("Key: {0}; Duration: {1}:{2}:{3}:{4}", key,
                //timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds));
        }

        private int Way1()
        {
            int count = 0;
            for (int i = 0; i < points.Length; i++)
            {
                PointX point = points[i];
                if (CheckDistance(TargetPoint, point))
                {
                    count++;
                }
            }
            return count;
        }

        private SortedSet<Px1> xs = new SortedSet<Px1>();
        private SortedSet<Px1> ys = new SortedSet<Px1>();
        private int Way2()
        {
            if (xs.Count == 0)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    PointX point = points[i];
                    Px1 px = new Px1(i, point.X);
                    xs.Add(px);

                    Px1 py = new Px1(i, point.Y);
                    ys.Add(py);
                }
            }

            int count = 0;
            SortedSet<Px1> pxes = xs.GetViewBetween(new Px1(0, TargetPoint.X - R), new Px1(points.Length, TargetPoint.X + R));
            SortedSet<Px1> pyes = ys.GetViewBetween(new Px1(0, TargetPoint.Y - R), new Px1(points.Length, TargetPoint.Y + R));
            var ps = pxes.Intersect(pyes, new Px1Comparer());
            foreach (var p in ps)
            {
                if (CheckDistance(TargetPoint, points[p.Index]))
                {
                    count++;
                }
            }

            return count;
        }

        private SortedSet<Px1> xs3 = new SortedSet<Px1>();
        private SortedSet<Px1> ys3 = new SortedSet<Px1>();
        private int Way3()
        {
            if (xs3.Count == 0)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    PointX point = points[i];
                    Px1 px = new Px1(i, point.X);
                    xs3.Add(px);

                    Px1 py = new Px1(i, point.Y);
                    ys3.Add(py);
                }
            }

            int count = 0;
            Dictionary<int, Px1> pxes = xs3.GetViewBetween(new Px1(0, TargetPoint.X - R), new Px1(points.Length, TargetPoint.X + R))
                .ToDictionary(x => x.Index);
            SortedSet<Px1> pyes = ys3.GetViewBetween(new Px1(0, TargetPoint.Y - R), new Px1(points.Length, TargetPoint.Y + R));
            var ps = Intersect(pxes, pyes);
            foreach (var p in ps)
            {
                if (CheckDistance(TargetPoint, points[p]))
                {
                    count++;
                }
            }

            return count;
        }

        private List<int> Intersect(Dictionary<int, Px1> pxes, SortedSet<Px1> pyes)
        {
            List<int> result = new List<int>();
            foreach (var item in pyes)
            {
                if (pxes.ContainsKey(item.Index))
                {
                    result.Add(item.Index);
                }
            }
            return result;
        }

        private bool CheckDistance(PointX point1, PointX point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2)) <= R;
        }
    }

    public class PointX
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PointX(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Px1 : IComparable<Px1>
    {
        public int Index { get; set; }
        public int Value { get; set; }

        public Px1(int index, int value)
        {
            Index = index;
            Value = value;
        }

        public int CompareTo(Px1 other)
        {
            int value = Value.CompareTo(other.Value);
            return value != 0 ? value : Index.CompareTo(other.Index);
        }
    }

    public class Px1Comparer : IEqualityComparer<Px1>
    {
        public bool Equals(Px1 x, Px1 y)
        {
            return x.Index == y.Index;
        }

        public int GetHashCode(Px1 obj)
        {
            return obj.Index.GetHashCode();
        }
    }
}
