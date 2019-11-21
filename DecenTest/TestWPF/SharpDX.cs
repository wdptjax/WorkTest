using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D2D = SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D3D9 = SharpDX.Direct3D9;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace TestWPF
{
    public class SharpDXDraw : SharpDxImage
    {
        private int _width;
        private int _height;

        public SharpDXDraw()
        {
            _width = this.PixelWidth;
            _height = this.PixelHeight;
        }

        public override void CreateAndBindTargets(int actualWidth, int actualHeight)
        {
            base.CreateAndBindTargets(actualWidth, actualHeight);
            _width = actualWidth;
            _height = actualHeight;
        }


        protected override void OnRender(SharpDX.Direct2D1.RenderTarget renderTarget)
        {
            D2D.SolidColorBrush lineBrush = new D2D.SolidColorBrush(renderTarget, new RawColor4(Colors.Lime.R, Colors.Lime.G, Colors.Lime.B, Colors.Lime.A));
            var data = GetRandomData();
            var points = GetDataPoints(data, 10, _width - 20, _height - 20, 10);
            for (int i = 0; i < points.Length - 1; i++)
            {
                renderTarget.DrawLine(points[i], points[i + 1], lineBrush, 1f);
            }
        }

        private Random _random = new Random();
        private float[] GetRandomData()
        {
            int len = 801;
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

    }

    /// <summary>
    /// [WPF 使用 SharpDX 在 D3DImage 显示](https://lindexi.oschina.io/lindexi/post/WPF-%E4%BD%BF%E7%94%A8-SharpDX-%E5%9C%A8-D3DImage-%E6%98%BE%E7%A4%BA.html#%E7%94%BB%E5%87%BA%E6%9D%A5 )
    /// </summary>    
    public abstract class SharpDxImage : D3DImage
    {
        public virtual void CreateAndBindTargets(int actualWidth, int actualHeight)
        {
            var width = Math.Max(actualWidth, 100);
            var height = Math.Max(actualHeight, 100);

            var renderDesc = new SharpDX.Direct3D11.Texture2DDescription
            {
                BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | SharpDX.Direct3D11.BindFlags.ShaderResource,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                MipLevels = 1,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.Shared,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                ArraySize = 1
            };

            var device = new SharpDX.Direct3D11.Device(DriverType.Hardware,
                SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport);

            var renderTarget = new SharpDX.Direct3D11.Texture2D(device, renderDesc);

            var surface = renderTarget.QueryInterface<SharpDX.DXGI.Surface>();

            var d2DFactory = new SharpDX.Direct2D1.Factory();

            var renderTargetProperties =
                new SharpDX.Direct2D1.RenderTargetProperties(
                    new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.Unknown,
                        SharpDX.Direct2D1.AlphaMode.Premultiplied));

            _d2DRenderTarget = new SharpDX.Direct2D1.RenderTarget(d2DFactory, surface, renderTargetProperties);

            SetRenderTarget(renderTarget);

            device.ImmediateContext.Rasterizer.SetViewport(0, 0, width, height);

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }


        protected abstract void OnRender(SharpDX.Direct2D1.RenderTarget renderTarget);

        private SharpDX.Direct3D9.Texture _renderTarget;
        private SharpDX.Direct2D1.RenderTarget _d2DRenderTarget;


        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Rendering();
        }


        private void Rendering()
        {
            _d2DRenderTarget.BeginDraw();

            OnRender(_d2DRenderTarget);

            _d2DRenderTarget.EndDraw();


            Lock();

            AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));

            Unlock();
        }

        private void SetRenderTarget(SharpDX.Direct3D11.Texture2D target)
        {
            var format = TranslateFormat(target);
            var handle = GetSharedHandle(target);

            var presentParams = GetPresentParameters();
            var createFlags = SharpDX.Direct3D9.CreateFlags.HardwareVertexProcessing |
                              SharpDX.Direct3D9.CreateFlags.Multithreaded |
                              SharpDX.Direct3D9.CreateFlags.FpuPreserve;

            var d3DContext = new SharpDX.Direct3D9.Direct3DEx();
            var d3DDevice = new SharpDX.Direct3D9.DeviceEx(d3DContext, 0, SharpDX.Direct3D9.DeviceType.Hardware,
                IntPtr.Zero, createFlags,
                presentParams);

            _renderTarget = new SharpDX.Direct3D9.Texture(d3DDevice, target.Description.Width,
                target.Description.Height, 1,
                SharpDX.Direct3D9.Usage.RenderTarget, format, SharpDX.Direct3D9.Pool.Default, ref handle);

            using (var surface = _renderTarget.GetSurfaceLevel(0))
            {
                Lock();
                SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
                Unlock();
            }
        }

        private static SharpDX.Direct3D9.PresentParameters GetPresentParameters()
        {
            var presentParams = new SharpDX.Direct3D9.PresentParameters();

            presentParams.Windowed = true;
            presentParams.SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard;
            presentParams.DeviceWindowHandle = NativeMethods.GetDesktopWindow();
            presentParams.PresentationInterval = SharpDX.Direct3D9.PresentInterval.Default;

            return presentParams;
        }

        private IntPtr GetSharedHandle(SharpDX.Direct3D11.Texture2D texture)
        {
            using (var resource = texture.QueryInterface<SharpDX.DXGI.Resource>())
            {
                return resource.SharedHandle;
            }
        }

        private static SharpDX.Direct3D9.Format TranslateFormat(SharpDX.Direct3D11.Texture2D texture)
        {
            switch (texture.Description.Format)
            {
                case SharpDX.DXGI.Format.R10G10B10A2_UNorm:
                    return SharpDX.Direct3D9.Format.A2B10G10R10;
                case SharpDX.DXGI.Format.R16G16B16A16_Float:
                    return SharpDX.Direct3D9.Format.A16B16G16R16F;
                case SharpDX.DXGI.Format.B8G8R8A8_UNorm:
                    return SharpDX.Direct3D9.Format.A8R8G8B8;
                default:
                    return SharpDX.Direct3D9.Format.Unknown;
            }
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = false)]
            public static extern IntPtr GetDesktopWindow();
        }
    }
}
