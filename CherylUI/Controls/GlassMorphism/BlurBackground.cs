using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;

namespace CherylUI.Controls.GlassMorphism;

public class BlurBackground : Control
{
    public static readonly StyledProperty<ExperimentalAcrylicMaterial> MaterialProperty =
        AvaloniaProperty.Register<BlurBackground, ExperimentalAcrylicMaterial>("Material");

    public ExperimentalAcrylicMaterial Material
    {
        get => GetValue(MaterialProperty);
        set => SetValue(MaterialProperty, value);
    }

    private static ImmutableExperimentalAcrylicMaterial DefaultAcrylicMaterialDark =
        (ImmutableExperimentalAcrylicMaterial)new ExperimentalAcrylicMaterial
        {
            MaterialOpacity = 0.25,
            TintColor = Colors.Black,
            TintOpacity = 0.7,
            PlatformTransparencyCompensationLevel = 0
        }.ToImmutable();

    private static ImmutableExperimentalAcrylicMaterial DefaultAcrylicMaterialLight =
        (ImmutableExperimentalAcrylicMaterial)new ExperimentalAcrylicMaterial()
        {
            MaterialOpacity = 0.0,
            TintColor = Colors.White,
            TintOpacity = 0.3,
            PlatformTransparencyCompensationLevel = 0
        }.ToImmutable();

    static BlurBackground()
    {
        AffectsRender<BlurBackground>(MaterialProperty);
    }

    private class BlurBehindRenderOperation : ICustomDrawOperation
    {
        private readonly ImmutableExperimentalAcrylicMaterial _material;
        private readonly Rect _bounds;

        public BlurBehindRenderOperation(ImmutableExperimentalAcrylicMaterial material, Rect bounds)
        {
            _material = material;
            _bounds = bounds;
        }

        public void Dispose()
        {

        }

        public bool HitTest(Point p) => _bounds.Contains(p);


        static SKColorFilter CreateAlphaColorFilter(double opacity)
        {
            if (opacity > 1)
                opacity = 1;
            var c = new byte[256];
            var a = new byte[256];
            for (var i = 0; i < 256; i++)
            {
                c[i] = (byte)i;
                a[i] = (byte)(i * opacity);
            }

            return SKColorFilter.CreateTable(a, c, c, c);
        }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature != null)
            {
                using var lease = leaseFeature.Lease();
                var canvas = lease.SkCanvas;

                if (!canvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
                    return;

                if (lease.SkSurface != null)
                {
                    using var backgroundSnapshot = lease.SkSurface.Snapshot();
                    using var backdropShader = SKShader.CreateImage(backgroundSnapshot, SKShaderTileMode.Clamp,
                        SKShaderTileMode.Clamp, currentInvertedTransform);

                    if (lease.GrContext == null) return;
                    using var blurred = SKSurface.Create(lease.GrContext, false, new SKImageInfo(
                        (int)Math.Ceiling(_bounds.Width),
                        (int)Math.Ceiling(_bounds.Height), SKImageInfo.PlatformColorType, SKAlphaType.Premul));
                    using (var filter = SKImageFilter.CreateBlur(15, 15, SKShaderTileMode.Clamp))
                    using (var blurPaint = new SKPaint())
                    {
                        blurPaint.Shader = backdropShader;
                        blurPaint.ImageFilter = filter;
                        blurred.Canvas.DrawRect(5, 5, (float)_bounds.Width - 20, (float)_bounds.Height - 20, blurPaint);
                    }

                    using (var blurSnap = blurred.Snapshot())
                    using (var blurSnapShader = SKShader.CreateImage(blurSnap))
                    using (var blurSnapPaint = new SKPaint())
                    {
                        blurSnapPaint.Shader = blurSnapShader;
                        blurSnapPaint.IsAntialias = true;
                        canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, blurSnapPaint);
                    }
                }
            }
        }

        public Rect Bounds => _bounds.Inflate(4);

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is BlurBehindRenderOperation op && op._bounds == _bounds && op._material.Equals(_material);
        }
    }

    public override void Render(DrawingContext context)
    {
        try
        {
            var mat = (ImmutableExperimentalAcrylicMaterial)Material.ToImmutable();
            context.Custom(new BlurBehindRenderOperation(mat, new Rect(default, Bounds.Size)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}