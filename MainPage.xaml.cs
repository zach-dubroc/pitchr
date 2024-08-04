using NAudio.Wave;


namespace pitchr {
  public partial class MainPage : ContentPage {
    //done:
    //get mic input
    
    //todo:
    //visualize waveform (waveFormVisual in xaml file)
    //finish process byte sample proccessing steps (comments in bottom of file)
    //once waveform is visual, re-factor to function on app open until close
    //find the tuning algo
    //add tune functionality, customize displays
    //clean
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

      

      testWav = new WaveFileWriter($"C:\\Users\\zacha\\OneDrive\\Desktop\\test.mp3", input.WaveFormat);
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

    static void ProcessAudioBuffer(byte[] buffer, int bytesRecorded) {

    }

  }
}
// MainPage.Xaml.cs  is above, MainPage.Xaml is below


//--------------------


//To visualize the waveform using the binary values from the microphone input, you can update the position or size of the waveFormVisual element based on the amplitude of the audio samples. Here's a basic implementation to visualize the waveform by updating the HeightRequest of the BoxView named waveFormVisual.

//First, add a method to convert the audio buffer into amplitude values:

//    Add a method to process the audio buffer:

//csharp

//private void ProcessAudioBuffer(byte[] buffer, int bytesRecorded) {
//  int bytesPerSample = input.WaveFormat.BitsPerSample / 8;
//  int samplesRecorded = bytesRecorded / bytesPerSample;
//  float[] sampleValues = new float[samplesRecorded];

//  for (int i = 0; i < samplesRecorded; i++) {
//    int sampleIndex = i * bytesPerSample;
//    // Convert the bytes to a 16-bit sample
//    short sample = BitConverter.ToInt16(buffer, sampleIndex);
//    // Normalize the sample value to a range between -1 and 1
//    sampleValues[i] = sample / 32768f;
//  }

//  // Calculate the average amplitude for this buffer
//  float averageAmplitude = sampleValues.Average(Math.Abs);

//  // Update the waveform visualization on the UI thread
//  Device.BeginInvokeOnMainThread(() => UpdateWaveformVisual(averageAmplitude));
//}

//private void UpdateWaveformVisual(float amplitude) {
//  // Scale the amplitude to fit within a reasonable range for the UI element
//  double visualHeight = amplitude * 150; // Adjust the multiplier as needed
//  waveFormVisual.HeightRequest = Math.Max(visualHeight, 1); // Ensure a minimum height
//}

//Update the MicOn method to process the buffer:

//csharp

//private void MicOn(object sender, WaveInEventArgs e) {
//  if (testWav != null) {
//    byte[] buffer = e.Buffer;
//    int bytesRecorded = e.BytesRecorded;
//    testWav.Write(buffer, 0, bytesRecorded);

//    // Process the audio buffer to update the waveform visualization
//    ProcessAudioBuffer(buffer, bytesRecorded);
//  }
//}

//With this code, the waveFormVisual element's height will be adjusted based on the average amplitude of the audio samples in each buffer. This creates a basic visualization of the waveform that will vary in size as the microphone input changes.

//You can further refine this visualization by updating the BoxView's properties or using a more sophisticated drawing approach if needed. This basic example provides a starting point for visualizing the waveform in your .NET MAUI application.