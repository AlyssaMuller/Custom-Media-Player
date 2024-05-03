using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.Win32;


namespace videoplayer
{
    public partial class AudioVideoPlayer : Window
    {
        private bool userIsDraggingSlider = false;
        bool is_loop_mode = false;

        public AudioVideoPlayer()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            
        }

        private double TimeToSeconds(string time)
        {
            char[] delimiters = { ':' };
            string[] parts = time.Split(delimiters);
            int partsCount = parts.Length;
            double timeInSeconds = Double.Parse(parts[partsCount -1]);
            if (partsCount > 1)
            {
                timeInSeconds += Double.Parse(parts[partsCount - 2])*60;
            }
            if (partsCount > 2)
            {
                timeInSeconds += Double.Parse(parts[partsCount - 3]) * 3600;
            }
            return timeInSeconds;
        }

        private void skipBlockedTimes()
        {
            for (int i = 0; i< blockedTimes.Items.Count; i++){
                string line = blockedTimes.Items[i].ToString();
                string[] words = line.Split(" ");
                double startBlock = TimeToSeconds(words[0]);
                double endBlock = TimeToSeconds(words[2]);
                if (mePlayer.Position.TotalSeconds<endBlock && mePlayer.Position.TotalSeconds >= startBlock )
                {
                    mePlayer.Position=TimeSpan.FromSeconds(endBlock);
                }
            }
        }

        
        private void timer_Tick(object sender, EventArgs e)
        {
            skipBlockedTimes();
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                skipBlockedTimes();
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
                sliProgress.SelectionEnd = sliProgress.Value;
                if (mePlayer.Position.TotalSeconds >= sliProgress.Maximum)
                {
                    if (is_loop_mode == false)
                    {
                        mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Maximum);
                        mePlayer.Pause();
                        btnPause.Visibility = Visibility.Hidden;
                        invisiblePause.Visibility = Visibility.Hidden;


                        btnPlay.Visibility = Visibility.Visible;
                        invisiblePlay.Visibility = Visibility.Visible;
                    }
                    if(is_loop_mode == true)
                    {
                        mePlayer.Position = TimeSpan.FromSeconds(0);
                        sliProgress.Value = 0;
                        mePlayer.Play();
                    }

                }
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
            sliProgress.Value = mePlayer.Position.TotalSeconds;
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
            sliProgress.Value = mePlayer.Position.TotalSeconds;
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void muteClicked(object sender, RoutedEventArgs e)
        {
            mePlayer.Volume = 0;
        }


        private void speedClicked(object sender, RoutedEventArgs e)
        {
            if (mePlayer.SpeedRatio < 2) { 
            mePlayer.SpeedRatio += .25;
            }
            else
            {
                mePlayer.SpeedRatio =.25;
            }

            speedBtn.Content = mePlayer.SpeedRatio;
        }

        private void lostFocus(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem != null)
                return;
            string newItem = comboBox.Text;
            char[] delimiters = { '\n'};
            string[] words = newItem.Split(delimiters);
            blockedTimes.SelectedItem = newItem;
        }

        private void UploadVideo(object sender, RoutedEventArgs e)
        {
            var findVideo = new OpenFileDialog();
            findVideo.Filter = "Media files (*.mp3;*.mpg;*.mpeg; *.wmv; *.mp4)|*.mp3;*.mpg;*.mpeg;*.mp4;*.wmv|All files (*.*)|*.*";
            findVideo.Title = "Choose Video";
            if (findVideo.ShowDialog() == true)
            {
                mePlayer.Source = new Uri(findVideo.FileName);

                //hide upload button
                uploadBtn.Visibility = Visibility.Hidden;

                //Show Video Controls
                statusBar.Visibility = Visibility.Visible;
                bottomBar.Visibility = Visibility.Visible;
                topBar.Visibility = Visibility.Visible;
                
                PlayClicked(sender, e); //Play video
                invisiblePlay.Visibility = Visibility.Hidden;
            }
        }

        private void forwardVideo(object sender, RoutedEventArgs e)
        {
            if (mePlayer.Position.TotalSeconds <= mePlayer.Position.TotalSeconds)
            {
                TimeSpan time = TimeSpan.FromSeconds(10);
                time =  TimeSpan.FromSeconds(sliProgress.Value) + time;
                mePlayer.Position = time;
            }
        }

        private void backwardVideo(object sender, RoutedEventArgs e)
        {
            if (mePlayer.Position.TotalSeconds <= mePlayer.Position.TotalSeconds)
            {
                TimeSpan time = TimeSpan.FromSeconds(10);
                time = TimeSpan.FromSeconds(sliProgress.Value) - time;
                mePlayer.Position = time;
            }
        }

        private void PlayClicked(object sender, RoutedEventArgs e)
        {
            if (mePlayer.Source != null) {
                if (mePlayer.Position.TotalSeconds >= sliProgress.Maximum)
                {
                    mePlayer.Position = TimeSpan.FromSeconds(0);
                    sliProgress.Value = 0;
                }
                mePlayer.Play();

                //Hide play controls
                btnPlay.Visibility = Visibility.Hidden;
                invisiblePlay.Visibility = Visibility.Hidden;
                //activate pause controls
                btnPause.Visibility = Visibility.Visible;
                invisiblePause.Visibility = Visibility.Visible;
                lblProgressStatus.Visibility = Visibility.Visible;
            }
        }

        private void PauseClicked(object sender, RoutedEventArgs e)
        {
            if (mePlayer.CanPause)
            {
                mePlayer.Pause();

                btnPause.Visibility = Visibility.Hidden;
                invisiblePause.Visibility = Visibility.Hidden;
                

                btnPlay.Visibility = Visibility.Visible;
                invisiblePlay.Visibility = Visibility.Visible;
            }
        }


        private void loopClicked(object sender, RoutedEventArgs e)
        {
            is_loop_mode = !is_loop_mode;
            if (is_loop_mode == true){
                LoopBtn.Content = "Loop Mode On";
            }
            if (is_loop_mode == false){
                LoopBtn.Content = "Loop Mode Off";
            }
        }
        private void blockedTxtFile(object sender, RoutedEventArgs e)
        {   var findBlockedTxtFile = new OpenFileDialog();
            findBlockedTxtFile.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            findBlockedTxtFile.Title = "Choose the .txt file with list of times to skip";
            if (findBlockedTxtFile.ShowDialog() == true)
            {   blockedTimes.Items.Clear();
                string block_txt = findBlockedTxtFile.FileName;
                using (var reader = new StreamReader(block_txt))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        char[] delimiters = { ' ', '\t' };
                        string[] words = line.Split(delimiters);
                        blockedTimes.Items.Add(words[0] + " - " + words[1]);
                    }
                }
                blockedTimes.Visibility = Visibility.Visible;
                
            }
        }

    }
}