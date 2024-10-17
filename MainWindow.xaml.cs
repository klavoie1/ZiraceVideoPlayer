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



        public MainWindow()
        {
            InitializeComponent();

            // Set up a timer to update the slider
            durationTimer = new DispatcherTimer();
            durationTimer.Interval = TimeSpan.FromMilliseconds(500);
            durationTimer.Tick += Timer_Tick;
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

        private void DurationSlider_PreviewMouseDown(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            isSeeking = true; // Stops the timer when a user interacts with the slider
            // Need to set the labels for the slider
        }

        private void DurationSlider_PreviewMouseUp(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            isSeeking = false; // Resumes the timer updates
            mediaElement.Position = TimeSpan.FromSeconds(DurationSlider.Value);
        }

        private void DurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan && DurationSlider.IsMouseOver)
            {
                mediaElement.Position = TimeSpan.FromSeconds(DurationSlider.Value);
            }
        }








    }
}