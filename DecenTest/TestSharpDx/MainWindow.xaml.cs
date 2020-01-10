﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using D2D = SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D3D9 = SharpDX.Direct3D9;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;

namespace TestSharpDx
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _d3D = KsyosqStmckfy;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CreateAndBindTargets();
            //Task.Run(() =>
            //{
            //    while (true)
            //    {
            //        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            //        {
            //            CompositionTarget_Rendering(null, null);
            //        });
            //        Thread.Sleep(10);
            //    }
            //});
        }

        private async void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {

                this.Dispatcher.Invoke(new Action(() =>
                {
                    _d2DRenderTarget.BeginDraw();
                    OnRender(_d2DRenderTarget);
                    _d2DRenderTarget.EndDraw();
                }));

                _device.ImmediateContext.Flush();// 这句代码是等待元素绘制完毕,如果没有这句代码,频繁刷新图像的时候会出现图像显示不完全的情况
            });
            _d3D.Lock();

            _d3D.AddDirtyRect(new Int32Rect(0, 0, _d3D.PixelWidth, _d3D.PixelHeight));

            _d3D.Unlock();
            base.InvalidateVisual();
        }

        private void OnRender(D2D.RenderTarget renderTarget)
        {
            var brush = new D2D.SolidColorBrush(renderTarget, new RawColor4(Colors.Lime.R, Colors.Lime.G, Colors.Lime.B, Colors.Lime.A));//new D2D.SolidColorBrush(_d2DRenderTarget, new RawColor4(1, 0, 0, 1));

            renderTarget.Clear(new RawColor4(Colors.Black.R, Colors.Black.G, Colors.Black.B, Colors.Black.A));

            //renderTarget.DrawRectangle(new RawRectangleF(_x, _y, _x + 10, _y + 10), brush);

            //_x = _x + _dx;
            //_y = _y + _dy;
            //if (_x >= ActualWidth - 10 || _x <= 0)
            //{
            //    _dx = -_dx;
            //}

            //if (_y >= ActualHeight - 10 || _y <= 0)
            //{
            //    _dy = -_dy;
            //}
            int width = _d3D.PixelWidth;
            int height = _d3D.PixelHeight;

            var data = GetRandomData();
            var points = GetDataPoints(data, 0, width, height, 0);
            for (int i = 0; i < points.Length - 1; i++)
            {
                renderTarget.DrawLine(points[i], points[i + 1], brush,0.5f);
            }
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

        private RawVector2[] GetDataPoints(float[] data, float minX, float maxX, float minY, float maxY)
        {
            RawVector2[] points = new RawVector2[data.Length];
            float step = (maxX - minX) / data.Length;
            float min = data.Min();
            float max = data.Max();
            for (int i = 0; i < data.Length; i++)
            {
                float x = minX + step * i;
                float y = minY + (data[i] - min) * (maxY - minY) / (max - min);
                points[i] = new RawVector2(x, y);
            }
            return points;
        }

        private float _x;
        private float _y;
        private float _dx = 1;
        private float _dy = 1;

        private D3D9.Texture _renderTarget;

        private D3DImage _d3D;
        private D3D11.Device _device;

        private D2D.RenderTarget _d2DRenderTarget;

        private void CreateAndBindTargets()
        {
            var width = Math.Max((int)ActualWidth, 100);
            var height = Math.Max((int)ActualHeight, 100);

            var renderDesc = new D3D11.Texture2DDescription
            {
                BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource,
                Format = DXGI.Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                MipLevels = 1,
                SampleDescription = new DXGI.SampleDescription(1, 0),
                Usage = D3D11.ResourceUsage.Default,
                OptionFlags = D3D11.ResourceOptionFlags.Shared,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                ArraySize = 1
            };

            _device = new D3D11.Device(DriverType.Hardware, D3D11.DeviceCreationFlags.BgraSupport);

            var renderTarget = new D3D11.Texture2D(_device, renderDesc);

            var surface = renderTarget.QueryInterface<DXGI.Surface>();

            var d2DFactory = new D2D.Factory();

            var renderTargetProperties =
                new D2D.RenderTargetProperties(new D2D.PixelFormat(DXGI.Format.Unknown, D2D.AlphaMode.Premultiplied));

            _d2DRenderTarget = new D2D.RenderTarget(d2DFactory, surface, renderTargetProperties);

            SetRenderTarget(renderTarget);

            _device.ImmediateContext.Rasterizer.SetViewport(0, 0, (int)ActualWidth, (int)ActualHeight);

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void SetRenderTarget(D3D11.Texture2D target)
        {
            var format = TranslateFormat(target);
            var handle = GetSharedHandle(target);

            var presentParams = GetPresentParameters();
            var createFlags = D3D9.CreateFlags.HardwareVertexProcessing | D3D9.CreateFlags.Multithreaded |
                              D3D9.CreateFlags.FpuPreserve;

            var d3DContext = new D3D9.Direct3DEx();
            var d3DDevice = new D3D9.DeviceEx(d3DContext, 0, D3D9.DeviceType.Hardware, IntPtr.Zero, createFlags,
                presentParams);

            _renderTarget = new D3D9.Texture(d3DDevice, target.Description.Width, target.Description.Height, 1,
                D3D9.Usage.RenderTarget, format, D3D9.Pool.Default, ref handle);

            using (var surface = _renderTarget.GetSurfaceLevel(0))
            {
                _d3D.Lock();
                _d3D.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
                _d3D.Unlock();
            }
        }

        private static D3D9.PresentParameters GetPresentParameters()
        {
            var presentParams = new D3D9.PresentParameters();

            presentParams.Windowed = true;
            presentParams.SwapEffect = D3D9.SwapEffect.Discard;
            presentParams.DeviceWindowHandle = NativeMethods.GetDesktopWindow();
            presentParams.PresentationInterval = D3D9.PresentInterval.Default;

            return presentParams;
        }

        private IntPtr GetSharedHandle(D3D11.Texture2D texture)
        {
            using (var resource = texture.QueryInterface<DXGI.Resource>())
            {
                return resource.SharedHandle;
            }
        }

        private static D3D9.Format TranslateFormat(D3D11.Texture2D texture)
        {
            switch (texture.Description.Format)
            {
                case SharpDX.DXGI.Format.R10G10B10A2_UNorm:
                    return D3D9.Format.A2B10G10R10;
                case SharpDX.DXGI.Format.R16G16B16A16_Float:
                    return D3D9.Format.A16B16G16R16F;
                case SharpDX.DXGI.Format.B8G8R8A8_UNorm:
                    return D3D9.Format.A8R8G8B8;
                default:
                    return D3D9.Format.Unknown;
            }
        }
    }

    public static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();
    }
}
