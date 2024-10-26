using System.IO;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ZiraceVideoPlayer.Models;
using Path = System.IO.Path;
using System.Timers;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace ZiraceVideoPlayer
{
    public partial class MainWindow : Window
    {
        // Tracking Variables
        private bool isSeeking = false;
        private bool isPlaying = false;
        private bool isFullscreen = false;

        //Dispatcher Elements
        private readonly DispatcherTimer durationTimer; //Timing for the Duration Slider
        private readonly DispatcherTimer DurationLabelTimer; // Timing for duration labels
        private readonly DispatcherTimer HideControlsTimer; // Timer for hiding the controls overlay
        private readonly DispatcherTimer GarbageCollectionTimer; // Garbage Collector for tests

        public MainWindow()
        {
            InitializeComponent();

            // Set up a timer to update the slider
            durationTimer = new DispatcherTimer();
            durationTimer.Interval = TimeSpan.FromMilliseconds(500);
            durationTimer.Tick += Timer_Tick!;

            // Initialize the timer to hide controls
            HideControlsTimer = new DispatcherTimer();
            HideControlsTimer.Interval = TimeSpan.FromSeconds(3); // Hide after 3 seconds of inactivity
            HideControlsTimer.Tick += HideControlsTimer_Tick!;

            // Start listening for mouse events to show/hide controls
            mediaElement.MouseMove += MediaElement_MouseMove;
            ControlPanel.MouseEnter += ControlPanel_MouseEnter;
            ControlPanel.MouseLeave += ControlPanel_MouseLeave;

            GarbageCollectionTimer = new DispatcherTimer();
            GarbageCollectionTimer.Interval = TimeSpan.FromSeconds(10);
            GarbageCollectionTimer.Tick += Timer_Tick!;

            volumeSlider.Value = 0.3; // Set default volume to 30%
            mediaElement.Volume = volumeSlider.Value; // Sync initial volume

            // Start with the control panel visible
            ShowControls();
            StartDurationTimer();

            // Load the saved state from XML or create a fresh one
            var savedState = LoadVideoState();

            // Check if a video was previously saved and exists on disk
            if (!string.IsNullOrEmpty(savedState.VideoPath) && File.Exists(savedState.VideoPath))
            {
                mediaElement.Source = new Uri(savedState.VideoPath);
                mediaElement.Position = TimeSpan.FromSeconds(savedState.LastPosition);

                Console.WriteLine($"Resuming: {savedState.VideoPath} at {savedState.LastPosition} seconds.");
                mediaElement.Play();  // start playing the video
            }
            else
            {
                Console.WriteLine("No previous video to load.");
            }

            // Debugging Methods
           
            RunMonitor();

            //FPSCounter fpsCounter = new FPSCounter();

            //PerformanceMonitor monitor = new PerformanceMonitor();

        }



        // Menu tab at the top of the video player settings--------------------------------------->//
        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void ToggleFullscreen_Click(object sender, RoutedEventArgs e) => ToggleFullscreen();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Zirace video player.", "About");
        }

        // End of File menu bar settings------------------------------------------------------------>


        // OverLay Control Panel Settings---------------------------------------------------------->
        // Hide the controls when the timer elapses
        private void HideControlsTimer_Tick(object sender, EventArgs e)
        {
            HideControls();
            HideControlsTimer.Stop(); // Stop the timer until the next interaction
            FadeOutAnimation();
        }

        // Helper to show the controls
        private void ShowControls()
        {
            ControlPanel.Visibility = Visibility.Visible;
            ControlPanel.Opacity = 1;
        }

        private void HideControls() => ControlPanel.Visibility = Visibility.Collapsed;

        // Show controls when the mouse moves over the video
        private void MediaElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (ControlPanel.Visibility != Visibility.Visible)
            {
                ControlPanel.BeginAnimation(OpacityProperty, null);
                ShowControls();
            }
            ControlPanel.Opacity = 1;
            HideControlsTimer.Stop(); // Reset the hide timer
            HideControlsTimer.Start(); // Start the hide timer again
        }

        // Ensure controls stay visible when interacting with them
        private void ControlPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            HideControlsTimer.Stop(); // Stop the timer while hovering on the controls
        }

        private void ControlPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            HideControlsTimer.Start(); // Restart the timer when leaving the controls
        }

        private void FadeOutAnimation()
        {
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(2),
                FillBehavior = FillBehavior.Stop // Ensures it doesn’t hold final value
            };

            fadeOut.Completed += (s, e) =>
            {
                ControlPanel.Visibility = Visibility.Collapsed;
                ControlPanel.Opacity = 1; // Reset opacity for next time
            };

            ControlPanel.BeginAnimation(OpacityProperty, fadeOut);
        }


        // End of Overlay Control Panel Settings--------------------------------------------------->



        // Control Panel Settings-------------------------------------------------------------------->

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan && !isSeeking)
            {
                DurationSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                DurationSlider.Value = mediaElement.Position.TotalSeconds;
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

        private void StartDurationTimer()
        {
            if (!durationTimer.IsEnabled)
            {
                durationTimer.Start();
            }
        }


        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Play();
            isPlaying = true;
            ShowControls();
            StartDurationTimer();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
            durationTimer.Stop();
            isPlaying = false;
            ShowControls();
            if (mediaElement.Source != null)
            {
                SaveVideoState(mediaElement.Source.ToString(), mediaElement.Position.TotalSeconds);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e) => Seek(-10);

        private void btnForward_Click(object sender, RoutedEventArgs e) => Seek(10);

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = volumeSlider.Value;
        }

        private void DurationSlider_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isSeeking = true; // Stops the timer when a user interacts with the slider
            durationTimer.Stop();
        }

        private void DurationSlider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isSeeking = false; // Resumes the timer updates
            mediaElement.Position = TimeSpan.FromSeconds(DurationSlider.Value);
            durationTimer.Start();
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
            if (isFullscreen == true)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                TopMenu.Visibility = Visibility.Visible;
                isFullscreen = false;
                Topmost = false;
            }
            else
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
                TopMenu.Visibility = Visibility.Collapsed;
                ResizeMode = ResizeMode.NoResize;
                isFullscreen = true;
                Topmost = true;
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

        private void RewindVideo()
        {
            if (mediaElement.Position.TotalSeconds > 10)
            {
                mediaElement.Position -= TimeSpan.FromSeconds(10);
            }
            else
            {
                mediaElement.Position = TimeSpan.Zero; // Go to start if less than 10 seconds
            }
        }

        private void FastForwardVideo()
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                double newTime = mediaElement.Position.TotalSeconds + 10;
                if (newTime < mediaElement.NaturalDuration.TimeSpan.TotalSeconds)
                {
                    mediaElement.Position = TimeSpan.FromSeconds(newTime);
                }
                else
                {
                    mediaElement.Position = mediaElement.NaturalDuration.TimeSpan; // Go to end
                }
            }
        }

        private void TogglePlayPause()
        {
            if (mediaElement != null && isPlaying == true)
            {
                mediaElement.Pause();
                System.Console.WriteLine("Video Paused");
                isPlaying = false;

                if (mediaElement.Source != null)
                {
                    SaveVideoState(mediaElement.Source.ToString(), mediaElement.Position.TotalSeconds);
                }
            }
            else if (mediaElement != null && isPlaying == false)
            {
                mediaElement.Play();
                System.Console.WriteLine($"Video Resumed @ {FormatTime(mediaElement.Position)}");
                isPlaying = true;
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

                case Key.Right:
                    FastForwardVideo();
                    e.Handled = true;
                    break;

                case Key.Left:
                    RewindVideo();
                    e.Handled = true;
                    break;

                case Key.Space:
                    TogglePlayPause();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    if (e.Key == Key.Escape && isFullscreen == true)
                    {
                        ToggleFullscreen_Click(sender, e);
                    }  
                    e.Handled= true;
                    break;
                
            }
        }

        // End of Keyboard Shortcuts-------------------------------------------------->

        //  Saving video progression settings---------------------------------->
        private static void SaveVideoState(string videoPath, double position)
        {
            try
            {
                VideoState state = new VideoState
                {
                    VideoPath = videoPath,
                    LastPosition = position
                };

                XmlSerializer serializer = new XmlSerializer(typeof(VideoState));
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VideoState.xml");

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(stream, state);
                }

                Console.WriteLine("Video state saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving video state: " + ex.Message);
            }
        }

        private static VideoState LoadVideoState()
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VideoState.xml");

                if (File.Exists(filePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(VideoState));
                    using FileStream stream = new FileStream(filePath, FileMode.Open);
                    return (VideoState)serializer.Deserialize(stream)!;
                }

                Console.WriteLine("No saved state found. Starting fresh.");
                return new VideoState { VideoPath = "", LastPosition = 0 };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading video state: " + ex.Message);
                return new VideoState { VideoPath = "", LastPosition = 0 };
            }
        }




        private void OpenVideo_Click(object sender, RoutedEventArgs e)
        {
            // Load the last saved state
            VideoState state = LoadVideoState();

            // Open the video file
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Video Files|*.mp4;*.avi;*.mkv"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                mediaElement.Source = new Uri(openFileDialog.FileName);

                // If the video matches the last saved one, restore the position
                if (state != null && state.VideoPath == openFileDialog.FileName)
                {
                    mediaElement.Position = TimeSpan.FromSeconds(state.LastPosition);
                }

                mediaElement.Play();
                isPlaying = true;

            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            SaveVideoState(mediaElement.Source.ToString(), mediaElement.Position.TotalSeconds);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (mediaElement.Source != null)
            {
                SaveVideoState(mediaElement.Source.ToString(), mediaElement.Position.TotalSeconds);
            }
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Load the saved position after the media is opened
            var savedState = LoadVideoState();
            if (mediaElement.Source != null && savedState.VideoPath == mediaElement.Source.ToString())
            {
                mediaElement.Position = TimeSpan.FromSeconds(savedState.LastPosition);
                mediaElement.Play(); // Start playing the video from the saved position
                Console.WriteLine($"Resuming: {savedState.VideoPath} at {savedState.LastPosition} seconds.");
            }
        }

        // End of Saving video settings------------------------------------------------>




        // Helper Methods------------------------------------------------------>

        // Helper method to format the time (hh:mm:ss)
        private static string FormatTime(TimeSpan time)
        {
            return time.ToString(time.Hours > 0 ? @"hh\:mm\:ss" : @"mm\:ss");
        }

        private void Seek(int seconds)
        {
            var newPosition = mediaElement.Position + TimeSpan.FromSeconds(seconds);
            mediaElement.Position = newPosition < TimeSpan.Zero ? TimeSpan.Zero : newPosition;
        }

        // End of Helper methods---------------------------------------------------->

        // Settings for Debugging and Performance Checking--------------------------->

        // When the application starts, this Garbage collector cleans up the processes
        // before the main application runs.
        public static void MonitorGC()
        {
            long memoryBefore = GC.GetTotalMemory(false);
            GC.Collect(); // Force garbage collection
            GC.WaitForPendingFinalizers();
            long memoryAfter = GC.GetTotalMemory(true);

            Console.WriteLine($"Memory used before GC: {memoryBefore / 1024} KB");
            Console.WriteLine($"Memory used after GC: {memoryAfter / 1024} KB");
        }

        private void RunMonitor()
        {
            GarbageCollectionTimer.Start();
            MonitorGC();
        }

        // Class for FPS counter on the application
        public class FPSCounter
        {
            private readonly Stopwatch stopwatch;
            private int frameCount;

            public FPSCounter()
            {
                stopwatch = Stopwatch.StartNew();
                CompositionTarget.Rendering += OnRendering;
            }

            private void OnRendering(object sender, EventArgs e)
            {
                frameCount++;
                if (stopwatch.ElapsedMilliseconds >= 1500)
                {
                    Console.WriteLine($"FPS: {frameCount}");
                    frameCount = 0;
                    stopwatch.Restart();
                }
            }
        }

        public class PerformanceMonitor
        {
            private readonly PerformanceCounter cpuCounter;
            private readonly PerformanceCounter ramCounter;
            private readonly System.Timers.Timer performanceTimer;

            public PerformanceMonitor()
            {
                // Set up performance counters for CPU and RAM usage
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                ramCounter = new PerformanceCounter("Memory", "Available MBytes");

                // Timer to periodically fetch metrics
                performanceTimer = new System.Timers.Timer(1000); // Every second
                performanceTimer.Elapsed += MonitorPerformance;
                performanceTimer.Start();
            }

            private void MonitorPerformance(object sender, ElapsedEventArgs e)
            {
                float cpuUsage = cpuCounter.NextValue();
                float availableRAM = ramCounter.NextValue();

                Console.WriteLine($"CPU Usage: {cpuUsage}%");
                Console.WriteLine($"Available RAM: {availableRAM} MB");
            }

            public void StopMonitoring()
            {
                performanceTimer.Stop();
                cpuCounter.Dispose();
                ramCounter.Dispose();
            }
        }

    }
}