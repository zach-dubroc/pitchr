using NAudio.Wave;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace pitchr {
  public partial class MainPage : ContentPage {

    private WaveInEvent input;
    private bool isRecording = false;
    private List<float> audioBuffer;
    private PitchDetector pitchDetector;
    private readonly int sampleRate = 48000; // Define the sample rate
    public MainPage() {
      InitializeComponent();
      audioBuffer = new List<float>();
      pitchDetector = new PitchDetector(sampleRate);
    }

    protected override void OnAppearing() {
      base.OnAppearing();
      StartRecording();
    }

    protected override void OnDisappearing() {
      StopRecording();
      base.OnDisappearing();
    }

    private void StartRecording() {
      if (isRecording) return;

      input = new WaveInEvent();
      input.DeviceNumber = 0; // grab default device
      input.WaveFormat = new WaveFormat(sampleRate, 16, 1);
      input.DataAvailable += MicOn;
      input.StartRecording();
      isRecording = true;
    }

    private void StopRecording() {
      if (!isRecording) return;
      //reset waveIn event
      if (input != null) {
        input.StopRecording();
        input.Dispose();
        input = null;
      }
      isRecording = false;
    }

    private void MicOn(object sender, WaveInEventArgs e) {
        byte[] buffer = e.Buffer;
        int bytesRecorded = e.BytesRecorded;
        //process buffer
        audioBuffer.Clear();
        for (int i = 0; i < bytesRecorded; i+=2) {
          short sample = BitConverter.ToInt16(buffer, i); //convert each sample to 16bit int
          //because max val of 65535
          audioBuffer.Add(sample / 32768f); //bring sample range from -1.0 to 1.0
        }


      float pitch = pitchDetector.DetectPitch(audioBuffer.ToArray());

      // Update the UI with the detected pitch frequency and note name

      MainThread.BeginInvokeOnMainThread(() => {
        noteLbl.Text = pitchDetector.ConvertFrequencyToNoteName(pitch)[0];
        octaveLbl.Text = pitchDetector.ConvertFrequencyToNoteName(pitch)[1];
        waveFormCanvas.InvalidateSurface();
      });

      waveFormCanvas.InvalidateSurface();
    }

    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e) {
      var canvas = e.Surface.Canvas;
      canvas.Clear(SKColors.Black);
      //check for data to draw
      if (audioBuffer.Count > 0) {
        //create new paint obj
        using (var paint = new SKPaint()) {
          paint.Style = SKPaintStyle.Stroke;
          paint.Color = SKColors.LightSlateGray;
          paint.StrokeWidth = 1;

          paint.IsAntialias = true;
          //blur?
          //paint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 1);
          float middle = e.Info.Height / 2;
          float width = e.Info.Width;

          float step = width / (float)audioBuffer.Count;

          SKPath path = new SKPath();
          path.MoveTo(0, middle);

          for (int i = 0; i < audioBuffer.Count; i++) {
            float x = i * step;
            float y = middle + (audioBuffer[i] * middle);
            path.LineTo(x, y);
          }
          canvas.DrawPath(path, paint);
        }
      }
    }
  }
}
