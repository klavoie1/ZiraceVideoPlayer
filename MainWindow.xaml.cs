using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ZiraceVideoPlayer
{
    
    public partial class MainWindow : Window
    {
        // Tracking Variables
        private bool isSeeking = false;
        private bool isPlaying = false;
        private bool isTimerUpdate = false;
        private bool isFullscreen = false;


        //Dispatcher Elements
        private DispatcherTimer durationTimer; //Timing for the Duration Slider
        private DispatcherTimer DurationLabelTimer; // Timing for duration labels


        public MainWindow()
        {
            InitializeComponent();

            // Set up a timer to update the slider
            durationTimer = new DispatcherTimer();
            durationTimer.Interval = TimeSpan.FromMilliseconds(500);
            durationTimer.Tick += Timer_Tick;

            DurationLabelTimer = new DispatcherTimer();
            DurationLabelTimer.Interval = TimeSpan.FromSeconds(2);
            DurationLabelTimer.Tick += Timer_Tick;

            volumeSlider.Value = 0.3; // Set default volume to 30%
            mediaElement.Volume = volumeSlider.Value; // Sync initial volume
            volumeSlider.Value = mediaElement.Volume;
        }

        

        // Menu tab at the top of the video player settings--------------------------------------->//
        private void OpenVideo_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mkv"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                mediaElement.Source = new Uri(openFileDialog.FileName);
                mediaElement.Play();
                durationTimer.Start();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Zirace video player.", "About");
        }


        // End of File menu bar settings------------------------------------------------------------>


        // OverLay Control Panel Settings---------------------------------------------------------->

        // End of Overlay Control Panel Settings--------------------------------------------------->



        // Control Panel Settings-------------------------------------------------------------------->

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan && !isSeeking)
            {
                isTimerUpdate = true;
                DurationSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                DurationSlider.Value = mediaElement.Position.TotalSeconds;
                isTimerUpdate = false;
            }
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                // Update the elapsed time
                TimeSpan currentTime = mediaElement.Position;
                ElapsedTime.Text = FormatTime(currentTime);

                // Update the remaining time
                TimeSpan remaining = mediaElement.NaturalDuration.TimeSpan - currentTime;
                RemainingTime.Text = $"-{FormatTime(remaining)}";

                // Update the slider value based on the current position
                DurationSlider.Value = currentTime.TotalSeconds;
                DurationSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            }
        }


        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.Position.TotalSeconds >= 10)
            {
                mediaElement.Position -= TimeSpan.FromSeconds(10);
            }
            else
            {
                mediaElement.Position = TimeSpan.Zero;
            }
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position += TimeSpan.FromSeconds(10);
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = volumeSlider.Value;
        }

        private void DurationSlider_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isSeeking = true; // Stops the timer when a user interacts with the slider
            DurationLabelTimer.Stop();
        }

        private void DurationSlider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isSeeking = false; // Resumes the timer updates
            mediaElement.Position = TimeSpan.FromSeconds(DurationSlider.Value);
            DurationLabelTimer.Start();
        }

        private void DurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan && DurationSlider.IsMouseOver)
            {
                mediaElement.Position = TimeSpan.FromSeconds(DurationSlider.Value);
            }
        }

        private void ToggleFullscreen()
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None; // Optional: Hide window borders
            }
            else
            {
                WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow; // Restore borders
            }
        }

        private void IncreaseVolume()
        {
            if (mediaElement.Volume < 1)
            {
                mediaElement.Volume = Math.Min(mediaElement.Volume + 0.1, 1.0); // Max Volume is 1
                volumeSlider.Value = mediaElement.Volume;
                Console.WriteLine($"Volume: {mediaElement.Volume * 100}%");
            }
        }

        private void DecreaseVolume()
        {
            if (mediaElement.Volume > 0.0)
            {
                mediaElement.Volume = Math.Max(mediaElement.Volume - 0.1, 0.0); //Min Volume is 0
                volumeSlider.Value = mediaElement.Volume;
                Console.WriteLine($"Volume: {mediaElement.Volume * 100}%");
            }
        }

        // End of Control Panel Settings------------------------------------------------------->


        // Keyboard Shortcut Settings---------------------------------------------->
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F:
                    ToggleFullscreen();
                    e.Handled = true;
                    break;

                case Key.Up:
                    IncreaseVolume();
                    e.Handled = true;
                    break;

                case Key.Down:
                    DecreaseVolume();
                    e.Handled = true;
                    break;


                
            }
        }




        // Helper Methods------------------------------------------------------>

        // Helper method to format the time (hh:mm:ss)
        private string FormatTime(TimeSpan time)
        {
            return time.ToString(time.Hours > 0 ? @"hh\:mm\:ss" : @"mm\:ss");
        }

        }
}