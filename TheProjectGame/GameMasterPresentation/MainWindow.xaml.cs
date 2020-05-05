using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace GameMasterPresentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //properties
        private GameMaster.GameMaster gameMaster;

        private Configuration.Configuration _gmConfig;

        public Configuration.Configuration GMConfig
        {
            get
            {
                return _gmConfig;
            }
            set
            {
                _gmConfig = value;
                NotifyPropertyChanged();
            }
        }

        private BoardComponent _board;

        public BoardComponent Board
        {
            get
            {
                return _board;
            }
            set
            {
                _board = value;
                NotifyPropertyChanged();
            }
        }

        private bool IsConnecting = false;
        private bool IsStartedGamePaused = false;

        private DispatcherTimer timer;
        private Stopwatch stopwatch;
        private Stopwatch frameStopwatch;

        private int frameCount = 0;
        private long previousTime = 0;
        private int fps = 0;

        public int FPS
        {
            get
            {
                return fps;
            }
            set
            {
                fps = value;
                NotifyPropertyChanged();
            }
        }

        //log
        private StringBuilder logStringBuilder = new StringBuilder();

        private bool IsUserScrollingLog = false;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();

            var configuration = GameMaster.GameMasterConfiguration.GetDefault();
            GMConfig = Configuration.Configuration.ReadFromFile(Constants.ConfigurationFilePath);
            if (GMConfig != null)
                configuration = GMConfig.ConvertToGMConfiguration();

            gameMaster = new GameMaster.GameMaster(configuration);

            Board = new BoardComponent(BoardCanvas);

            //TODO: handle null exeption if default not loaded
            GMConfig.PropertyChanged += GMConfig_PropertyChanged;

            timer = new DispatcherTimer();
            stopwatch = new Stopwatch();
            frameStopwatch = new Stopwatch();
            //33-> 30FPS
            timer.Interval = TimeSpan.FromMilliseconds(33);
            timer.Tick += TimerEvent;

            stopwatch.Start();

            frameStopwatch.Start();
            timer.Start();
        }

        private void GMConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(GMConfig));
        }

        private void TimerEvent(object sender, EventArgs e)
        {
            long currentFrame = frameStopwatch.ElapsedMilliseconds;
            frameCount++;
            if (currentFrame - previousTime >= 1000)
            {
                FPS = frameCount;
                frameCount = 0;
                previousTime = currentFrame;
            }
            stopwatch.Stop();
            var elapsed = (double)stopwatch.ElapsedMilliseconds / 1000.0;
            stopwatch.Reset();
            stopwatch.Start();
            Update(elapsed);

            Board.UpdateBoard(gameMaster.PresentationComponent.GetPresentationData());
        }

        private void ConnectRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsConnecting == true)
                return;

            IsConnecting = true;
            gameMaster.SetConfiguration(GMConfig.ConvertToGMConfiguration());

            try
            {
                gameMaster.ConnectToCommunicationServer();
                ConnectRadioButton.Content = "Connected";
                StartRadioButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception occured!", MessageBoxButton.OK, MessageBoxImage.Error);
                IsConnecting = false;
                ConnectRadioButton.IsChecked = false;
            }
        }

        private void StartRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsStartedGamePaused == true)
            {
                ConnectRadioButton.Content = "Connected";
                ConnectRadioButton.IsEnabled = false;
                StartRadioButton.Content = "In Game";
                //resume game
                ResumeGame();
                IsStartedGamePaused = false;
                PauseRadioButton.Content = "Pause";
            }
            else
            {
                if (StartGame())
                {
                    ConnectRadioButton.Content = "Connected";
                    ConnectRadioButton.IsEnabled = false;
                    StartRadioButton.Content = "In Game";
                    //TODO:
                    //create agents List
                    AgentsCountLabel.Content = gameMaster.Agents.Count.ToString();

                    PauseRadioButton.IsEnabled = true;
                }
                else
                {
                    ConnectRadioButton.IsChecked = true;
                    MessageBox.Show("Error starting Game!", "Game Master", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void PauseRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            StartRadioButton.Content = "Resume";
            PauseRadioButton.Content = "Paused";
            IsStartedGamePaused = true;

            //pause game in game master
            PauseGame();
        }

        private void BreakpointButton_Click(object sender, RoutedEventArgs e)
        {
            ;
        }

        private void UpdateLog(string text)
        {
            logStringBuilder.AppendLine(text);
            LogTextBlock.Text = logStringBuilder.ToString();
            if (IsUserScrollingLog == false)
            {
                //TODO: allow user to scroll
                LogScrollViewer.ScrollToEnd();
                //IsUserScrollingLog = false;
            }
        }

        private void LogScrollViewer_LostFocus(object sender, RoutedEventArgs e)
        {
            IsUserScrollingLog = false;
        }

        private void LogScrollViewer_GotFocus(object sender, RoutedEventArgs e)
        {
            IsUserScrollingLog = true;
        }

        private bool StartGame()
        {
            if (gameMaster.StartGame())
            {
                Board.InitializeBoard(gameMaster.Agents.Count, GMConfig);
                return true;
            }
            return false;
        }

        private void PauseGame()
        {
            gameMaster.PauseGame();
        }

        private void ResumeGame()
        {
            gameMaster.ResumeGame();
        }

        private void Update(double dt)
        {
            gameMaster.Update(dt);
            FlushLogs();
        }

        private void FlushLogs()
        {
            var logs = gameMaster.Logger.GetPendingLogs();
            foreach (var log in logs)
                UpdateLog(log);
        }

        private void ConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            var configurationWindows = Application.Current.Windows.OfType<Configuration.ConfigurationWindow>();
            if (configurationWindows.Any() == false)
            {
                if (GMConfig == null)
                    GMConfig = new Configuration.Configuration();
                var ConfigurationWindow = new Configuration.ConfigurationWindow(GMConfig);
                //this line should be useful but produces weird behaviour of minimizing main window after closing child window
                //ConfigurationWindow.Owner = this;
                ConfigurationWindow.Show();
            }
            else
            {
                if (configurationWindows.First().WindowState == WindowState.Minimized)
                    configurationWindows.First().WindowState = WindowState.Normal;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            var configurationWindows = Application.Current.Windows.OfType<Configuration.ConfigurationWindow>();
            if (configurationWindows.Any() == true)
            {
                configurationWindows.First().Close();
                if (configurationWindows.First() is Configuration.ConfigurationWindow confWindow)
                {
                    if (confWindow.IsClosed == false)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            gameMaster.OnDestroy();
        }
    }
}