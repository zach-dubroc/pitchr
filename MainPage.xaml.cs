using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace pitchr {
    public partial class MainPage : ContentPage {

        private WaveformDrawable waveformDrawable;

        public MainPage() {
            InitializeComponent();
            InitializeWaveform();
        }

        private void InitializeWaveform() {
            var random = new Random();
            foreach (var child in WaveformGrid.Children) {
                if (child is BoxView boxView) {
                    boxView.HeightRequest = random.Next(10, 60); // Simulate waveform heights
                }
            }
        }
    }
}
