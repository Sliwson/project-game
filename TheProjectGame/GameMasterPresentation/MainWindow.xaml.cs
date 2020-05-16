using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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

        private DispatcherTimer timer;
        private Stopwatch stopwatch;
        private Stopwatch frameStopwatch;

        private int frameCount = 0;
        private long previousTime = 0;
        private int _fps = 0;

        public int FPS
        {
            get
            {
                return _fps;
            }
            set
            {
                _fps = value;
                NotifyPropertyChanged();
            }
        }

        //log
        private string _searchString;

        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                _searchString = value;
                FilteredLogEntries = new ObservableCollection<LogEntry>(LogEntries.Where(l => l.Message.ToLower().Contains(_searchString.ToLower())));
                NotifyPropertyChanged();
                NotifyPropertyChanged("FilteredLogEntries");
            }
        }


        private bool IsUserScrollingLog = false;

        private List<LogEntry> LogEntries { get; set; } = new List<LogEntry>();
        public ObservableCollection<LogEntry> FilteredLogEntries { get; set; } = new ObservableCollection<LogEntry>();

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
                ResetRadioButton.IsEnabled = true;
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
            if (StartGame())
            {
                ConnectRadioButton.Content = "Connected";
                ConnectRadioButton.IsEnabled = false;
                StartRadioButton.Content = "In Game";
            }
            else
            {
                ConnectRadioButton.IsChecked = true;
                MessageBox.Show("Error starting Game!", "Game Master", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void ResetRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure?", Constants.GameMasterMessageBoxName, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                ResetGame();
        }

        private void UpdateLog(string text)
        {
            var log = new LogEntry(text);
            LogEntries.Add(log);
            if (string.IsNullOrEmpty(SearchString) || text.ToLower().Contains(SearchString.ToLower()))
                FilteredLogEntries.Add(log);
            if (IsUserScrollingLog == false)
            {
                LogScrollViewer.ScrollToEnd();
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

        private void ResetGame()
        {
            stopwatch.Stop();
            frameStopwatch.Stop();
            timer.Stop();
            frameCount = 0;
            previousTime = 0;
            FPS = 0;
            IsConnecting = false;

            gameMaster.OnDestroy();

            StartRadioButton.Content = "Start";
            StartRadioButton.IsEnabled = false;
            StartRadioButton.IsChecked = false;

            ConnectRadioButton.Content = "Connect";
            ConnectRadioButton.IsChecked = false;

            Binding myBinding = new Binding(nameof(GMConfig));
            myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            myBinding.Mode = BindingMode.OneWay;
            myBinding.Converter = new GMConfigToBoolConverter();
            BindingOperations.SetBinding(ConnectRadioButton, RadioButton.IsEnabledProperty, myBinding);

            ResetRadioButton.IsChecked = false;
            ResetRadioButton.IsEnabled = false;

            LogEntries.Clear();
            FilteredLogEntries.Clear();

            Board.ClearBoard();

            gameMaster = new GameMaster.GameMaster(GMConfig.ConvertToGMConfiguration());

            Board = new BoardComponent(BoardCanvas);

            stopwatch.Start();
            frameStopwatch.Start();
            timer.Start();
        }

        private void Update(double dt)
        {
            gameMaster.Update(dt);
            if (gameMaster.state == GameMaster.GameMasterState.CriticalError)
            {
                MessageBox.Show(gameMaster.LastException?.Message, "Critical exception occured, application will close", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
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