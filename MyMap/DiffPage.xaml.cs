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
        private int numOfPoint = 10000000;
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
                DateTime start = DateTime.Now;
                int numOfPoint = Way1();
                DateTime end = DateTime.Now;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    txt1.Text = txt1.Text + "\r\n" + BuildResult(start, end, TargetPoint, numOfPoint);
                });
            });

            Task.Run(async () => {
                DateTime start = DateTime.Now;
                int numOfPoint = Way2();
                DateTime end = DateTime.Now;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    txt2.Text = txt2.Text + "\r\n" + BuildResult(start, end, TargetPoint, numOfPoint);
                });
            });
        }

        private string BuildResult(DateTime start, DateTime end, PointX target, int numOfResult)
        {
            TimeSpan timeSpan = end - start;
            return string.Format("X: {0}, Y: {1}; NumOfResult: {2}; Duration: {3}:{4}:{5}:{6}", target.X, target.Y, numOfResult, 
                timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }

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
                    DateTime d1 = DateTime.Now;
                    Px1 px = xs.Where(x => x.Value == point.X).FirstOrDefault();
                    DateTime d2 = DateTime.Now;
                    BuildLog("1", d1, d2);
                    if (px == null)
                    {
                        px = new Px1(point.X);
                        xs.Add(px);
                        DateTime d3 = DateTime.Now;
                        BuildLog("2", d2, d3);
                    }
                    px.Indexs.Add(i);

                    DateTime d4 = DateTime.Now;
                    Px1 py = ys.Where(x => x.Value == point.Y).FirstOrDefault();
                    DateTime d5 = DateTime.Now;
                    BuildLog("3", d4, d5);
                    if (py == null)
                    {
                        py = new Px1(point.Y);
                        ys.Add(py);
                        DateTime d6 = DateTime.Now;
                        BuildLog("4", d5, d6);
                    }
                    py.Indexs.Add(i);
                }
            }

            int count = 0;
            SortedSet<Px1> pxes = xs.GetViewBetween(new Px1(TargetPoint.X - R), new Px1(TargetPoint.X + R));
            SortedSet<Px1> pyes = ys.GetViewBetween(new Px1(TargetPoint.Y - R), new Px1(TargetPoint.Y + R));
            List<int> indexes = Intersect(pxes.ToArray(), pyes.ToArray());
            foreach (var index in indexes)
            {
                if (CheckDistance(TargetPoint, points[index]))
                {
                    count++;
                }
            }

            return count;
        }

        private List<int> Intersect(Px1[] setX, Px1[] setY)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < setX.Length; i++)
            {
                Px1 px = setX[i];
                for (int j = 0; j < setY.Length; j++)
                {
                    Px1 py = setY[j];
                    result.AddRange(px.Indexs.Intersect(py.Indexs));
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
        public SortedSet<int> Indexs { get; set; }
        public int Value { get; set; }

        public Px1(int value)
        {
            Indexs = new SortedSet<int>();
            Value = value;
        }

        public int CompareTo(Px1 other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}
