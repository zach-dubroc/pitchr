using NAudio.Wave;

namespace pitchr {
  public partial class MainPage : ContentPage {
    //main todo:
    //1. get mic input DONE DID

    //--not doin yet:
    //2. visualize it DO DIS
    //3. make the tuner actually tune

    //globally for to start, separate later
    private WaveFileWriter testWav;
    private WaveInEvent input;

    public MainPage() {
      InitializeComponent();
    }

    //exp with higher sample rates in testing(48k,96k)
    //32, 64, 128, 256, 512,1024-common buffers
    //if I even need a buffer size?? why wouldn't I?

    //start stop recording test input
    private void OnStartClick(object sender, EventArgs e) {
      startBtn.IsVisible = false;
      stopBtn.IsVisible = true;
      testLbl.TextColor = Color.FromHex("#ff0000");
      int sampleRate = 48000;
      //int bufferSize = 1024;

      input = new WaveInEvent();

      input.DeviceNumber = 0; //grab default device
      input.WaveFormat = new WaveFormat(sampleRate, 16, 1);
      input.DataAvailable += MicOn;

      testWav = new WaveFileWriter($"C:\\Users\\MCA\\Desktop\\test2.wav", input.WaveFormat);
      input.StartRecording();
    }

    //when data comes in
    private void MicOn(object sender, WaveInEventArgs e) {
      if (testWav != null) {
        byte[] buffer = e.Buffer;
        int bytesRecorded = e.BytesRecorded;
        testWav.Write(buffer, 0, bytesRecorded);
        
      }
    }

    private void OnStopClick(object sender, EventArgs e) {
      stopBtn.IsVisible = false;
      startBtn.IsVisible = true;
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
    }
  }
}
