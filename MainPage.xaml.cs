using NAudio.Wave;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;
using Newtonsoft.Json.Linq;
namespace pitchr {
  public partial class MainPage : ContentPage {

    private WaveInEvent input;
    private bool isRecording = false;
    private List<float> audioBuffer;
    private PitchDetector pitchDetector;
    private readonly int sampleRate = 48000;

    public MainPage() {
      InitializeComponent();
      audioBuffer = new List<float>();
      pitchDetector = new PitchDetector(sampleRate);
      FetchAndSetQuoteAsync();
    }
    #region open/close
    protected override void OnAppearing() {
      base.OnAppearing();
      StartRecording();
    }
    protected override void OnDisappearing() {
      StopRecording();
      base.OnDisappearing();
    }
    #endregion

    #region capture mic input data
    private void StartRecording() {
      if (isRecording) return;
      input = new WaveInEvent();
      input.DeviceNumber = 0;
      input.WaveFormat = new WaveFormat(sampleRate, 16, 1);
      //32bit maybe more accurate but can't test yet
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

      //keep everything dependant on audio data to update on single thread
      MainThread.BeginInvokeOnMainThread(() => {
        freqLbl.Text = $"{pitch:F2} Hz";
        noteLbl.Text = noteInfo[0];
        octaveLbl.Text = noteInfo[1];

        try {
          //get current notes target frequency
          //set needle rotation angle
          float targetFrequency = pitchDetector.GetTargetFrequency(noteInfo[0], int.Parse(noteInfo[1]));
          float rotationAngle = CalculateNeedleRotation(pitch, noteInfo);
          needleImage.Rotation = rotationAngle;

          //needle rotation closer to zero --> gradient of note/octave closer to green
          //stop need from rotating off screen
          float deviation = Math.Abs(rotationAngle);
          float maxDeviation = 45.0f;
          float hue = (1.0f - Math.Min(deviation / maxDeviation, 1.0f)) * 120.0f; // hue: 0 (Red) to 120 (Green) 
          Color gradientColor = Color.FromHsla(hue / 360.0f, 1.0f, 0.5f);
          noteLbl.TextColor = gradientColor;
          octaveLbl.TextColor = gradientColor;
        }
        catch (FormatException ex) {
          Console.WriteLine($"Error parsing input: {ex.Message}");
          noteLbl.TextColor = Colors.Red;
          octaveLbl.TextColor = Colors.Red;
        }
        catch (ArgumentException ex) {
          Console.WriteLine($"Invalid argument: {ex.Message}");
          noteLbl.TextColor = Colors.Red;
          octaveLbl.TextColor = Colors.Red;
        }
        waveFormCanvas.InvalidateSurface();
      });
    } 
    #endregion

    //UI updates/SK event funcs
    #region xaml updates
    private float currentRotation = 0;
    private float smoothingFactor = 0.5f; // Adjust this value to control the smoothness
    private float CalculateNeedleRotation(float pitch, string[] noteInfo) {
      float targetFrequency;

      // Validate the noteInfo to get the target frequency
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

      // Calculate the difference between the detected pitch and the target frequency
      float offset = pitch - targetFrequency;
      float targetRotation = offset * 5; // Scale the offset to an appropriate rotation value

      // Apply exponential smoothing to reduce jitter
      float smoothingFactor = 0.1f; // Adjust this value to control the smoothness (0 < smoothingFactor < 1)
      currentRotation = (1 - smoothingFactor) * currentRotation + smoothingFactor * targetRotation;

      // Cap the rotation to ensure the needle stays within a reasonable range
      float maxRotation = 30.0f; // Example max rotation angle in degrees
      currentRotation = Math.Max(-maxRotation, Math.Min(currentRotation, maxRotation));
      return currentRotation;
    }



    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e) {
      var canvas = e.Surface.Canvas;

      // Try to parse the color
      bool col = SKColor.TryParse("#121212", out SkiaSharp.SKColor color);

      // Use the parsed color if successful, otherwise default to black
      canvas.Clear(col ? color : SKColors.Black);

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
    //random inspirational quote with an incorrect author for bottom text
    private async void FetchAndSetQuoteAsync() {
      var quoteFetch = new QuoteFetch();
      string quote = await quoteFetch.GetRandomQuoteAsync();
      quoteLbl.Text = quote;
    } 
    #endregion
  }
}
