using NAudio.Wave;

namespace pitchr {
  public partial class MainPage : ContentPage {
    private WaveFileWriter testWav;
    private WaveInEvent input;
    private bool isRecording = false;

    public MainPage() {
      InitializeComponent();
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

      testLbl.TextColor = Color.FromHex("#ff0000");
      int sampleRate = 48000;

      input = new WaveInEvent();
      input.DeviceNumber = 0; // grab default device
      input.WaveFormat = new WaveFormat(sampleRate, 16, 1);
      input.DataAvailable += MicOn;

      testWav = new WaveFileWriter($"C:\\Users\\MCA\\Desktop\\test.mp3", input.WaveFormat);
      input.StartRecording();
      isRecording = true;
    }

    private void StopRecording() {
      if (!isRecording) return;

      testLbl.TextColor = Color.FromHex("#1DB954");

      if (testWav != null) {
        testWav.Dispose();
        testWav = null;
      }

      if (input != null) {
        input.StopRecording();
        input.Dispose();
        input = null;
      }
      isRecording = false;
    }

    private void MicOn(object sender, WaveInEventArgs e) {
      //if signal is being captured, run visualizer/logic
      if (testWav != null) {
        byte[] buffer = e.Buffer;
        int bytesRecorded = e.BytesRecorded;
        testWav.Write(buffer, 0, bytesRecorded);

        // Here you could add the code to visualize the waveform using SkiaSharp
      }
    }
  }
}
