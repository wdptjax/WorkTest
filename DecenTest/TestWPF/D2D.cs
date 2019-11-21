using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using D2D = Microsoft.WindowsAPICodePack.DirectX.Direct2D1;

namespace TestWPF
{
    class DirectDrawing
    {
        private D2D.RenderTarget _renderTarget;
        private D2D.SolidColorBrush _lineBrush;

        private float _width;
        private float _height;
        private System.Windows.Window _window;

        public DirectDrawing(System.Windows.Window window)
        {
            var d2dFactory = D2D.D2DFactory.CreateFactory(D2D.D2DFactoryType.Multithreaded);
            var windowHandle = new WindowInteropHelper(window).Handle;
            _renderTarget = d2dFactory.CreateHwndRenderTarget(new D2D.RenderTargetProperties(),
                    new D2D.HwndRenderTargetProperties(windowHandle,
                        new D2D.SizeU((uint)window.ActualWidth, (uint)window.ActualHeight),
                        D2D.PresentOptions.RetainContents));
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            _width = (float)window.ActualWidth;
            _height = (float)window.ActualHeight;
            _window = window;
            window.SizeChanged += Window_SizeChanged;
            _lineBrush = _renderTarget.CreateSolidColorBrush(new D2D.ColorF(Colors.Lime.R, Colors.Lime.G, Colors.Lime.B));
            //Thread thd = new Thread(test);
            //thd.IsBackground = true;
            //thd.Start();
        }

        //private void test()
        //{
        //    while (true)
        //    {
        //        _window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
        //        {
        //            Draw();
        //        });
        //        Thread.Sleep(50);
        //    }
        //}


        private void Window_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            var window = sender as System.Windows.Window;
            _width = (float)window.ActualWidth;
            _height = (float)window.ActualHeight;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_renderTarget == null)
            {
                return;
            }
            Draw();
        }

        public void Draw()
        {
            _renderTarget.BeginDraw();
            _renderTarget.Clear();

            var points = GetDataPoints(GetRandomData(),
                10, _width - 20, _height - 20, 10);
            for (int i = 0; i < points.Length - 1; i++)
            {
                _renderTarget.DrawLine(points[i], points[i + 1], _lineBrush, 0.1f);
            }
            _renderTarget.EndDraw();
        }

        private Random _random = new Random();
        private float[] GetRandomData()
        {
            int len = 12801;
            float[] data = new float[len];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = _random.Next(0, 50);
            }
            data[len / 2 - 1] += 50;
            data[len / 2] += 100;
            data[len / 2 + 1] += 50;
            return data;
        }

        private D2D.Point2F[] GetDataPoints(float[] data, float minX, float maxX, float minY, float maxY)
        {
            D2D.Point2F[] points = new D2D.Point2F[data.Length];
            float step = (maxX - minX) / data.Length;
            float min = data.Min();
            float max = data.Max();
            for (int i = 0; i < data.Length; i++)
            {
                float x = minX + step * i;
                float y = minY + (data[i] - min) * (maxY - minY) / (max - min);
                points[i] = new D2D.Point2F(x, y);
            }
            return points;
        }
    }
}
