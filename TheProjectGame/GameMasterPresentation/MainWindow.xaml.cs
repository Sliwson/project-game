﻿using System;
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

        private BoardComponent board;

        private Configuration.Configuration GMConfig;

        public BoardComponent Board
        {
            get
            {
                return board;
            }
            set
            {
                board = value;
                NotifyPropertyChanged();
            }
        }

        private bool IsStartedGamePaused = false;

        private DispatcherTimer timer;
        private Stopwatch stopwatch;

        private Random random = new Random();

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

            gameMaster = new GameMaster.GameMaster();

            GMConfig = Configuration.Configuration.ReadFromFile(Constants.ConfigurationFilePath);

            Board = new BoardComponent(BoardCanvas);

            timer = new DispatcherTimer();
            stopwatch = new Stopwatch();
            //33-> 30FPS
            timer.Interval = TimeSpan.FromMilliseconds(33);
            timer.Tick += TimerEvent;
            timer.Start();
        }

        private void TimerEvent(object sender, EventArgs e)
        {
            stopwatch.Stop();
            Update((double)stopwatch.ElapsedMilliseconds / 1000.0);
            stopwatch.Start();
            Board.UpdateBoard(gameMaster.PresentationComponent.GetPresentationData());
        }

        private void ConnectRadioButton_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void StartRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ConnectRadioButton.IsEnabled = false;
            StartRadioButton.Content = "In Game";
            if (IsStartedGamePaused == true)
            {
                //resume game
                ResumeGame();
                IsStartedGamePaused = false;
                PauseRadioButton.Content = "Pause";
            }
            else
            {
                StartGame();

                Board.InitializeBoard(gameMaster.Agents.Count, GMConfig);

                //TODO:
                //create agents List
                AgentsCountLabel.Content = gameMaster.Agents.Count.ToString();

                PauseRadioButton.IsEnabled = true;
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

        private void StartGame()
        {
            //logger.Debug("Game Started!");
            gameMaster.StartGame();
        }

        private void PauseGame()
        {
            //logger.Debug("Game Paused!");
            gameMaster.PauseGame();
        }

        private void ResumeGame()
        {
            //logger.Debug("Resume Game");
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
                var ConfigurationWindow = new Configuration.ConfigurationWindow(GMConfig);
                ConfigurationWindow.Show();
            }
            else
            {
                if (configurationWindows.First().WindowState == WindowState.Minimized)
                    configurationWindows.First().WindowState = WindowState.Normal;
            }
        }
    }
}