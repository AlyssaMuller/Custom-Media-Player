using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace WpfTutorialSamples.Audio_and_Video
{
    public partial class AudioVideoPlayerCompleteSample : Window
    {
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        
        public AudioVideoPlayerCompleteSample()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
            }
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void UploadVideo(object sender, RoutedEventArgs e)
        {
            var findVideo = new OpenFileDialog();
            findVideo.Filter = "Media files (*.mp3;*.mpg;*.mpeg, *.wmv)|*.mp3;*.mpg;*.mpeg; *.wmv|All files (*.*)|*.*";
            findVideo.Title = "Choose Video";
            if (findVideo.ShowDialog() == true)
            {
                mePlayer.Source = new Uri(findVideo.FileName);
                uploadBtn.Visibility = Visibility.Hidden;
                statusBar.Visibility=Visibility.Visible;
                blockedTimes.Visibility = Visibility.Visible;
            }
        }

        private void PlayClicked(object sender, RoutedEventArgs e)
        {
            if (mePlayer.Source != null)
                mePlayer.Play();
        }

        private void PauseClicked(object sender, RoutedEventArgs e)
        {
            if (mePlayer.CanPause)
                mePlayer.Pause();
        }

    }
}