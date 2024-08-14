using MathNet.Numerics.IntegralTransforms;
using System.Linq;
using System.Numerics;

namespace pitchr {
  public class PitchDetector {
    private readonly int _sampleRate;

    public PitchDetector(int sampleRate) {
      _sampleRate = sampleRate;
    }

    public float DetectPitch(float[] buffer) {
      // Apply a Hamming window to the buffer to reduce spectral leakage
      for (int i = 0; i < buffer.Length; i++) {
        buffer[i] *= (float)(0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (buffer.Length - 1)));
      }

      // Convert the float buffer to a complex array for FFT
      var complexBuffer = buffer.Select(b => new Complex(b, 0.0)).ToArray();

      // Perform the FFT
      Fourier.Forward(complexBuffer, FourierOptions.Matlab);

      // Calculate magnitudes and find the index of the peak frequency
      double[] magnitudes = new double[complexBuffer.Length / 2];
      for (int i = 0; i < complexBuffer.Length / 2; i++) {
        magnitudes[i] = complexBuffer[i].Magnitude;
      }

      int peakIndex = Array.IndexOf(magnitudes, magnitudes.Max());

      // Calculate the frequency corresponding to the peak index
      float frequency = peakIndex * (float)_sampleRate / buffer.Length;

      return frequency;
    }

    // Optional: Convert frequency to note name
    public string[] ConvertFrequencyToNoteName(float frequency) {

      //string[] noteAndOctave = [];

      if (frequency <= 0) return ["",""];
      
      string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

      // Calculate the note number relative to A4 (440 Hz)
      int noteNumber = (int)Math.Round(12 * Math.Log2(frequency / 440.0)) + 69;

      // Ensure the noteNumber is within a valid range
      noteNumber = Math.Max(0, Math.Min(noteNumber, 127)); // Limits the noteNumber between 0 and 127

      int octave = (noteNumber / 12) - 1;
      string noteName = noteNames[noteNumber % 12];

      return [noteName, octave.ToString()];
    }
  }
}
