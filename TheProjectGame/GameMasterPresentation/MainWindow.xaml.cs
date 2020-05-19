using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace GameMasterPresentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Private variables

        private Mutex gameMasterMutex = new Mutex();
        private Task gameMasterThread = null;
        private GameMaster.GameMaster gameMaster;

        private Mutex shouldCloseMutex = new Mutex();
        private bool shouldClose = false;

        private Configuration.Configuration _gmConfig;

        private BoardComponent _board;

        private bool IsConnecting = false;

        private DispatcherTimer guiTimer;

        private int _fps = 0;

        //log
        private string _searchString;

        private bool IsUserScrollingLog = false;

        private List<LogEntry> LogEntries { get; set; } = new List<LogEntry>();
        public ObservableCollection<LogEntry> FilteredLogEntries { get; set; } = new ObservableCollection<LogEntry>();

        #endregion Private variables

        #region Properties

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

        #endregion Properties

        #region Event handlers

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GMConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(GMConfig));
        }

        private void ConnectRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsConnecting == true)
                return;

            gameMasterMutex.WaitOne();
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


            gameMasterMutex.ReleaseMutex();
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

        private void LogScrollViewer_LostFocus(object sender, RoutedEventArgs e)
        {
            IsUserScrollingLog = false;
        }

        private void LogScrollViewer_GotFocus(object sender, RoutedEventArgs e)
        {
            IsUserScrollingLog = true;
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
            shouldCloseMutex.WaitOne();
            shouldClose = true;
            shouldCloseMutex.ReleaseMutex();
        }

        #endregion Event handlers

        #region Timer events

        private void GuiTimerEvent(object sender, EventArgs e)
        {
            gameMasterMutex.WaitOne();
            var presentationData = gameMaster.PresentationComponent.GetPresentationData();
            gameMasterMutex.ReleaseMutex();

            Board.UpdateBoard(presentationData);
            FlushLogs();
        }

        #endregion Timer events

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

            guiTimer = new DispatcherTimer();
            guiTimer.Interval = TimeSpan.FromMilliseconds(16);
            guiTimer.Tick += GuiTimerEvent;
            guiTimer.Start();

            gameMasterThread = new Task(RunGameMasterThread);
            gameMasterThread.Start();
        }

        #region Private Methods

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

        private bool StartGame()
        {
            gameMasterMutex.WaitOne();
            var result = gameMaster.StartGame();

            if (result)
                Board.InitializeBoard(gameMaster.Agents.Count, GMConfig);

            gameMasterMutex.ReleaseMutex();
            return result;
        }

        private void ResetGame()
        {
            guiTimer.Stop();
            FPS = 0;
            IsConnecting = false;

            //schedule close of old gm
            shouldCloseMutex.WaitOne();
            shouldClose = true;
            shouldCloseMutex.ReleaseMutex();
            gameMasterThread.Wait();

            shouldClose = false;
            gameMaster = null;

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

            // start new gm
            gameMaster = new GameMaster.GameMaster(GMConfig.ConvertToGMConfiguration());
            Board = new BoardComponent(BoardCanvas);
            guiTimer.Start();

            gameMasterThread = new Task(RunGameMasterThread);
            gameMasterThread.Start();
        }

        private void RunGameMasterThread()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            double frameTimer = 0;
            int frameCount = 0;

            shouldCloseMutex.WaitOne();
            var close = shouldClose;
            shouldCloseMutex.ReleaseMutex();

            while (close == false)
            {
                stopwatch.Stop();
                var elapsed = (double)stopwatch.ElapsedMilliseconds / 1000.0;
                stopwatch.Reset();
                stopwatch.Start();

                frameTimer += elapsed;
                frameCount++;

                if (frameTimer >= 1)
                {
                    FPS = frameCount;
                    frameTimer = 0;
                    frameCount = 0;
                }

                gameMasterMutex.WaitOne(); 
                gameMaster.Update(elapsed);
                var state = gameMaster.state;
                gameMasterMutex.ReleaseMutex();

                if (state == GameMaster.GameMasterState.CriticalError)
                    OnCriticalError();

                Thread.Sleep(3);

                shouldCloseMutex.WaitOne();
                close = shouldClose;
                shouldCloseMutex.ReleaseMutex();
            }

            gameMaster.OnDestroy();
        }

        private void OnCriticalError()
        {
            MessageBox.Show(gameMaster.LastException?.Message, "Critical exception occured, application will close", MessageBoxButton.OK, MessageBoxImage.Error);
            if (Application.Current != null)
                Application.Current.Shutdown();
        }

        private void FlushLogs()
        {
            gameMasterMutex.WaitOne();
            var logs = gameMaster.Logger.GetPendingLogs();
            gameMasterMutex.ReleaseMutex();

            foreach (var log in logs)
                UpdateLog(log);
        }

        #endregion Private Methods
    }
}