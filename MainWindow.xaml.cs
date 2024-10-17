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
        private DispatcherTimer durationTimer;



        public MainWindow()
        {
            InitializeComponent();

            // Set up a timer to update the slider and labels
            durationTimer = new DispatcherTimer();
            durationTimer.Interval = TimeSpan.FromSeconds(1);
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
            MessageBox.Show("This is a simple video player.", "About");
        }


        // End of File menu bar settings------------------------------------------------------------>
        

        // OverLay Control Panel Settings---------------------------------------------------------->
    
        // End of Overlay Control Panel Settings--------------------------------------------------->



        // Control Panel Settings------------------------------------------------------------------>
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



    }
}