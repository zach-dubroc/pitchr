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
      //ham window
      for (int i = 0; i < buffer.Length; i++) {
        buffer[i] *= (float)(0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (buffer.Length - 1)));
      }

      //float buffer --> complex array for fft
      var complexBuffer = buffer.Select(b => new Complex(b, 0.0)).ToArray();

      // adds empty space to complexArray so smooth results, not implemented
      var paddedBuffer = new Complex[complexBuffer.Length * 2];
      complexBuffer.CopyTo(paddedBuffer, 0);

      // call fft class
      Fourier.Forward(complexBuffer, FourierOptions.Matlab);

      // finds peak/strongest point frequency
      double[] magnitudes = new double[complexBuffer.Length / 2];
      for (int i = 0; i < complexBuffer.Length / 2; i++) {
        magnitudes[i] = complexBuffer[i].Magnitude;
      }
      int peakIndex = Array.IndexOf(magnitudes, magnitudes.Max());

      double interpolation = 0.0;

      if (peakIndex > 0 && peakIndex < magnitudes.Length - 1) {
        double left = magnitudes[peakIndex - 1];
        double right = magnitudes[peakIndex + 1];
        interpolation = (right - left) / (2 * (2 * magnitudes[peakIndex] - right - left));
      }
      else {
        // if peak at end interpolation will index error
        Console.WriteLine("Peak index at boundary, skipping interpolation.");
      }

      float preciseFrequency = (float)((peakIndex + interpolation) * _sampleRate / buffer.Length);

      return preciseFrequency;

    }
    public string[] ConvertFrequencyToNoteName(float frequency) {
      //A440 as reference pitch
      //all midi notes 0-C1 to 127-G9
      if (frequency <= 0) return ["", ""];
      string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
      int noteNumber = (int)Math.Round(12 * Math.Log2(frequency / 440.0)) + 69;
      noteNumber = Math.Max(0, Math.Min(noteNumber, 127)); 
      //examples in notes.txt
      int octave = (noteNumber / 12) - 1;
      string noteName = noteNames[noteNumber % 12];
      return [noteName, octave.ToString()];
    }

    //returns frequency of note name, example in notes
    public float GetTargetFrequency(string noteName, int octave) {
      string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

      int noteIndex = Array.IndexOf(noteNames, noteName);
      if (noteIndex == -1) throw new ArgumentException("Invalid note name");

      int noteNumber = noteIndex + (octave + 1) * 12;
      float frequency = (float)(440.0 * Math.Pow(2, (noteNumber - 69) / 12.0));

      return frequency;
    }
  }
}