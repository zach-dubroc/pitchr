using SkiaSharp;
using SkiaSharp.Views.Maui;

public class WaveformDrawable {
    public void Draw(SKCanvas canvas, SKRect dirtyRect) {
        var paint = new SKPaint {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White.WithAlpha(128), // Opaque white color
            StrokeWidth = 2
        };

        // Simulate a static waveform
        var path = new SKPath();
        path.MoveTo(0, dirtyRect.MidY);
        for (int i = 0; i < dirtyRect.Width; i++) {
            float y = (float)(Math.Sin(i * 0.05) * 20 + dirtyRect.MidY);
            path.LineTo(i, y);
        }

        canvas.DrawPath(path, paint);
    }
}
