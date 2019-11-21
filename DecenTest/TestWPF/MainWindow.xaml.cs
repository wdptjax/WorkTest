using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TestWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DrawingVisual _drawingVisual = new DrawingVisual();

        private Random _random = new Random();

        private DirectDrawing _directDrawing;

        public MainWindow()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //_directDrawing = new DirectDrawing(this);
            KsyosqStmckfy.CreateAndBindTargets((int)this.ActualWidth, (int)this.ActualHeight);
            this.SizeChanged += (s, ea) =>
              {
                  //KsyosqStmckfy.CreateAndBindTargets((int)this.ActualWidth, (int)this.ActualHeight);
              };
            //drawingCanvas.AddVisual(_drawingVisual);
            //Task.Factory.StartNew(() =>
            //{
            //    while (true)
            //    {
            //        try
            //        {
            //            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            //            {
            //                Draw();
            //            });
            //        }
            //        catch (Exception)
            //        {
            //        }
            //        Thread.Sleep(10);
            //    }
            //});
        }

        private void Draw()
        {
            DateTime dt = DateTime.Now;

            var data = GetRandomData();
            DrawingContext dc = _drawingVisual.RenderOpen();
            Pen pen = new Pen(Brushes.Green, 1);
            Pen blue = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1000ff00")), 1);
            pen.Freeze();  //冻结画笔，这样能加快绘图速度
            blue.Freeze();

            for (int i = 0; i < data.Length - 1; i++)
            {

                dc.DrawLine(pen, new Point(i, -data[i] * this.ActualHeight / 90 - this.ActualHeight / 3), new Point(i + 1, -data[i + 1] * this.ActualHeight / 90 - this.ActualHeight / 3));

                dc.DrawLine(blue, new Point(i, -data[i] * this.ActualHeight / 90 - this.ActualHeight / 3), new Point(i, 0));
                if (i == data.Length - 2)
                    dc.DrawLine(blue, new Point(i + 1, -data[i + 1] * this.ActualHeight / 90 - this.ActualHeight / 3), new Point(i + 1, 0));
            }

            dc.Close();
            var span = DateTime.Now.Subtract(dt).TotalMilliseconds;
            textBlock.Text = span.ToString();
        }

        //必须重载这两个方法，不然是画不出来的
        // 重载自己的VisualTree的孩子的个数，由于只有一个DrawingVisual，返回1
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        // 重载当WPF框架向自己要孩子的时候，返回返回DrawingVisual
        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
                return _drawingVisual;

            throw new IndexOutOfRangeException();
        }

        private float[] GetRandomData()
        {
            float[] data = new float[1601];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = _random.Next(0, 50);
            }
            data[799] += 50;
            data[800] += 100;
            data[801] += 50;
            return data;
        }

        private Point[] GetDataPoints(float[] data, float minX, float maxX, float minY, float maxY)
        {
            Point[] points = new Point[data.Length];
            float step = (maxX - minX) / data.Length;
            float min = data.Min();
            float max = data.Max();
            for (int i = 0; i < data.Length; i++)
            {
                float x = minX + step * i;
                float y = minY + (data[i] - min) * (maxY - minY) / (max - min);
                points[i] = new Point(x, y);
            }
            return points;
        }

    }
}
