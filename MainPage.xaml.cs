using NAudio.Wave;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace pitchr {
  public partial class MainPage : ContentPage {

    private WaveInEvent input;
    private bool isRecording = false;
    private List<float> audioBuffer;
    private PitchDetector pitchDetector;
    private readonly int sampleRate = 96000;

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
      input.DeviceNumber = 0;
      input.WaveFormat = new WaveFormat(sampleRate, 16, 1);
      input.DataAvailable += MicOn;
      input.StartRecording();
      isRecording = true;
    }

    private void StopRecording() {
      if (!isRecording) return;
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
      audioBuffer.Clear();
      for (int i = 0; i < bytesRecorded; i += 2) {
        short sample = BitConverter.ToInt16(buffer, i);
        audioBuffer.Add(sample / 32768f);
      }

      float pitch = pitchDetector.DetectPitch(audioBuffer.ToArray());
      string[] noteInfo = pitchDetector.ConvertFrequencyToNoteName(pitch);

      MainThread.BeginInvokeOnMainThread(() => {
        freqLbl.Text = $"{pitch:F2} Hz";
        noteLbl.Text = noteInfo[0];
        octaveLbl.Text = noteInfo[1];

        // Update needle rotation based on the pitch offset from the target note
        float rotationAngle = CalculateNeedleRotation(pitch, noteInfo);
        needleImage.Rotation = rotationAngle;

        // Change label colors based on tuning accuracy
        bool isInTune = Math.Abs(rotationAngle) < 2; // Example threshold
        noteLbl.TextColor = isInTune ? Color.FromHex("#1DB954") : Color.FromHex("#ff6347");
        octaveLbl.TextColor = isInTune ? Color.FromHex("#1DB954") : Color.FromHex("#ff6347");

        waveFormCanvas.InvalidateSurface();
      });
    }

    private float currentRotation = 0;

    private float smoothingFactor = 0.1f; // Adjust this value to control the smoothness

    private float CalculateNeedleRotation(float pitch, string[] noteInfo) {
      float targetFrequency;

      if (noteInfo == null || noteInfo.Length != 2 || string.IsNullOrEmpty(noteInfo[0]) || !int.TryParse(noteInfo[1], out int octave)) {
        Console.WriteLine("Invalid noteInfo provided, defaulting to A4 (440 Hz).");
        targetFrequency = 440.0f; // A4 frequency
      }
      else {
        try {
          targetFrequency = pitchDetector.GetTargetFrequency(noteInfo[0], octave);
        }
        catch (Exception ex) {
          Console.WriteLine($"Error calculating target frequency: {ex.Message}. Defaulting to A4 (440 Hz).");
          targetFrequency = 440.0f; // A4 frequency
        }
      }

      float offset = pitch - targetFrequency;
      float targetRotation = offset * 5;

      // Exponential smoothing
      currentRotation = (1 - smoothingFactor) * currentRotation + smoothingFactor * targetRotation;

      // Ensure the rotation is capped to prevent excessive movement
      float maxRotation = 45.0f; // Example max rotation angle
      currentRotation = Math.Max(-maxRotation, Math.Min(currentRotation, maxRotation));

      return currentRotation;
    }

    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e) {
      var canvas = e.Surface.Canvas;
      canvas.Clear(SKColors.Black);

      if (audioBuffer.Count > 0) {
        using (var paint = new SKPaint()) {
          paint.Style = SKPaintStyle.Stroke;
          paint.Color = SKColors.LightSlateGray;
          paint.StrokeWidth = 1;
          paint.IsAntialias = true;

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
