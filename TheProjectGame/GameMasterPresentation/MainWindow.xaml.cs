﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GameMasterPresentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //properties

        private GameMaster.GameMaster gameMaster;

        private bool IsStartedGamePaused = false;

        private DispatcherTimer timer;
        private Stopwatch stopwatch;

        private Random random = new Random();

        //log
        private StringBuilder logStringBuilder = new StringBuilder();
        private bool IsUserScrollingLog = false;

        public MainWindow()
        {
            InitializeComponent();

            gameMaster = new GameMaster.GameMaster();
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
        }

        

        

        

        

        //private void GetGameMasterConfiguration()
        //{
        //    BoardRows = gameMaster.Configuration.BoardY;
        //    BoardColumns = gameMaster.Configuration.BoardX;
        //    BoardGoalAreaRows = gameMaster.Configuration.GoalAreaHeight;
        //}

        

        

        //private void MockBoard()
        //{
        //    BoardField.SetGoalBoardField(BoardFields[0, 3], true, true);
        //    BoardField.SetGoalBoardField(BoardFields[1, 2], false, true);
        //    BoardField.SetGoalBoardField(BoardFields[1, 4], true, true);
        //    BoardField.SetGoalBoardField(BoardFields[2, 3], false, true);
        //    BoardField.SetGoalBoardField(BoardFields[2, 4], true, false);

        //    BoardField.SetGoalBoardField(BoardFields[9, 3], false, true);
        //    BoardField.SetGoalBoardField(BoardFields[10, 2], false, true);
        //    BoardField.SetGoalBoardField(BoardFields[10, 4], true, true);
        //    BoardField.SetGoalBoardField(BoardFields[10, 5], true, false);
        //    BoardField.SetGoalBoardField(BoardFields[11, 3], false, true);
        //    BoardField.SetGoalBoardField(BoardFields[11, 4], true, false);

        //    BoardField.SetAgentBoardField(BoardFields[4, 2], 1, false, false);
        //    BoardField.SetAgentBoardField(BoardFields[7, 4], 2, false, true);
        //    BoardField.SetAgentBoardField(BoardFields[6, 5], 3, true, false);
        //    BoardField.SetAgentBoardField(BoardFields[8, 3], 4, true, false);

        //    BoardField.SetPieceBoardField(BoardFields[5, 1], false);
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //InitPresentation();
            //GetGameMasterConfiguration();
            //GenerateBoard(BoardCanvas);
            //MockBoard();            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Abort();
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
                //SetAgentFields(BoardCanvas);
                //TODO:
                //do it better
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

        private void SetScore()
        {
            int scoreRed = gameMaster.ScoreComponent.GetScore(Messaging.Enumerators.TeamId.Red);
            int scoreBlue = gameMaster.ScoreComponent.GetScore(Messaging.Enumerators.TeamId.Blue);

            RedTeamScoreLabel.Content = scoreRed.ToString();
            BlueTeamScoreLabel.Content = scoreBlue.ToString();
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
        private void Abort()
        {
            //TODO:
            //fix
            //foreach (var t in threads)
            //    t.Abort();
        }

        //private void UpdateAgents()
        //{
        //    for (int i = 0; i < AgentFields.Length; i++)
        //    {
        //        SetSingleAgent(gameMaster.Agents[i], AgentFields[i]);
        //    }
        //}
    }
}