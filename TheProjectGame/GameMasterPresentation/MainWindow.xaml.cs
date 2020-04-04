using System;
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
        private List<Line> BoardMesh;

        private List<Label> BoardGoalAreas;

        private BoardField[,] BoardFields;

        private BoardField[] AgentFields;
        private int BoardRows;
        private int BoardColumns;
        private int BoardGoalAreaRows;
        private double FieldSize;

        private bool IsStartedGamePaused = false;

        private DispatcherTimer timer;
        private DateTime timeDiffStart;
        private TimeSpan timeDiff;

        private Random random = new Random();

        private StringBuilder logStringBuilder = new StringBuilder();
        private bool IsUserScrollingLog = false;

        public MainWindow()
        {
            InitializeComponent();
            gameMaster = new GameMaster.GameMaster();
            timer = new DispatcherTimer();
            //33-> 30FPS
            timer.Interval = TimeSpan.FromMilliseconds(33);
            timer.Tick += TimerEvent;
        }

        private void TimerEvent(object sender, EventArgs e)
        {
            timeDiff = DateTime.Now - timeDiffStart;
            timeDiffStart = DateTime.Now;
            Update(timeDiff.TotalMilliseconds);
            UpdateLog($"Timespan: {timeDiff.TotalMilliseconds} sample looooooooooooooooggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggg");
            //UpdateBoard();
            
        }

        private void GenerateBoard(Canvas canvas)
        {
            //canvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //canvas.Arrange(new Rect(0, 0, canvas.DesiredSize.Width, canvas.DesiredSize.Height));

            double widthStep = canvas.ActualWidth / (double)BoardColumns;
            double heightStep = canvas.ActualHeight / (double)BoardRows;

            if (widthStep < 20)
                widthStep = 20;
            if (heightStep < 20)
                heightStep = 20;

            double min = Math.Min(heightStep, widthStep);
            FieldSize = min;
            heightStep = min;
            widthStep = min;
            canvas.Width = widthStep * BoardColumns;
            canvas.Height = heightStep * BoardRows;

            SetBoardMesh(canvas, widthStep, heightStep);
            SetGoalAreasBackgrounds(canvas, widthStep, heightStep);
            SetBoardFields(canvas, widthStep, heightStep);
        }

        private void SetBoardMesh(Canvas canvas, double widthStep, double heightStep)
        {
            BoardMesh = new List<Line>();
            //lines
            double pointX = 0d;
            double pointY = 0d;
            double point2X = 0d;
            double point2Y = heightStep * BoardRows;
            //vertical
            for (int i = 0; i < BoardColumns + 1; i++)
            {
                int lineThickness = 1;
                if (i == 0 || i == BoardColumns)
                    lineThickness = 3;

                Line line = new Line
                {
                    X1 = pointX,
                    Y1 = pointY,
                    X2 = point2X,
                    Y2 = point2Y,
                    StrokeThickness = lineThickness,
                    Stroke = new SolidColorBrush(Colors.Black)
                };
                BoardMesh.Add(line);
                Panel.SetZIndex(line, 20);
                canvas.Children.Add(line);
                pointX += widthStep;
                point2X += widthStep;
            }
            //horizontal
            pointX = 0d;
            pointY = 0d;
            point2X = widthStep * BoardColumns;
            point2Y = 0d;
            for (int i = 0; i < BoardRows + 1; i++)
            {
                int lineThickness = 1;
                if (i == 0 || i == BoardRows || i == BoardGoalAreaRows || i == BoardRows - BoardGoalAreaRows)
                    lineThickness = 3;
                Line line = new Line
                {
                    X1 = pointX,
                    Y1 = pointY,
                    X2 = point2X,
                    Y2 = point2Y,
                    StrokeThickness = lineThickness,
                    Stroke = new SolidColorBrush(Colors.Black)
                };
                BoardMesh.Add(line);
                Panel.SetZIndex(line, 100);
                canvas.Children.Add(line);
                pointY += heightStep;
                point2Y += heightStep;
            }
        }

        private void SetGoalAreasBackgrounds(Canvas canvas, double widthStep, double heightStep)
        {
            BoardGoalAreas = new List<Label>();

            Label label1 = new Label
            {
                Background = new SolidColorBrush(Color.FromRgb(115, 194, 251)),
                Width = widthStep * BoardColumns,
                Height = heightStep * BoardGoalAreaRows
            };
            Canvas.SetLeft(label1, 0);
            Canvas.SetTop(label1, heightStep * (BoardRows - BoardGoalAreaRows));
            Panel.SetZIndex(label1, 10);

            BoardGoalAreas.Add(label1);
            canvas.Children.Add(label1);

            Label label2 = new Label
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Width = widthStep * BoardColumns,
                Height = heightStep * (BoardRows - 2 * BoardGoalAreaRows)
            };

            Canvas.SetLeft(label2, 0);
            Canvas.SetTop(label2, heightStep * BoardGoalAreaRows);
            Panel.SetZIndex(label2, 10);

            BoardGoalAreas.Add(label2);
            canvas.Children.Add(label2);

            Label label3 = new Label
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 190, 188)),
                Width = widthStep * BoardColumns,
                Height = heightStep * BoardGoalAreaRows
            };

            Canvas.SetLeft(label3, 0);
            Canvas.SetTop(label3, 0);
            Panel.SetZIndex(label3, 10);

            BoardGoalAreas.Add(label3);
            canvas.Children.Add(label3);
        }

        private void SetBoardFields(Canvas canvas, double widthStep, double heightStep)
        {
            BoardFields = new BoardField[BoardRows, BoardColumns];
            //fields
            double pointX = 0d;
            double pointY = heightStep * (BoardRows - 1);
            for (int y = 0; y < BoardRows; y++)
            {
                for (int x = 0; x < BoardColumns; x++)
                {
                    BoardFields[y, x] = new BoardField(canvas, widthStep, heightStep, pointX, pointY, Colors.Transparent);
                    pointX += widthStep;
                }
                pointX = 0d;
                pointY -= heightStep;
                if (pointY < 0)
                    pointY = 0d;
            }
        }

        private void GetGameMasterConfiguration()
        {
            BoardRows = gameMaster.Configuration.BoardY;
            BoardColumns = gameMaster.Configuration.BoardX;
            BoardGoalAreaRows = gameMaster.Configuration.GoalAreaHeight;
        }

        private void SetAgentFields(Canvas canvas)
        {
            AgentFields = new BoardField[gameMaster.Agents.Count];
            for (int i = 0; i < AgentFields.Length; i++)
            {
                AgentFields[i] = new BoardField(canvas, FieldSize, FieldSize, 0, 0, Colors.Transparent);
                SetSingleAgent(gameMaster.Agents[i], AgentFields[i]);
            }
        }

        private void SetSingleAgent(GameMaster.Agent agent, BoardField boardField)
        {
            var pos = agent.Position;
            //for testing
            //double pointX2 = random.Next(0, BoardColumns * (int)FieldSize);
            double pointX = FieldSize * pos.X;
            double pointY = FieldSize * (BoardRows - 1 - pos.Y);
            bool isRed = agent.Team == Messaging.Enumerators.TeamId.Red ? true : false;
            bool hasPiece = agent.Piece == null ? false : true;
            boardField.Move(pointX, pointY);
            BoardField.SetAgentBoardField(boardField, agent.Id, isRed, hasPiece);
        }

        private void MockBoard()
        {
            BoardField.SetGoalBoardField(BoardFields[0, 3], true, true);
            BoardField.SetGoalBoardField(BoardFields[1, 2], false, true);
            BoardField.SetGoalBoardField(BoardFields[1, 4], true, true);
            BoardField.SetGoalBoardField(BoardFields[2, 3], false, true);
            BoardField.SetGoalBoardField(BoardFields[2, 4], true, false);

            BoardField.SetGoalBoardField(BoardFields[9, 3], false, true);
            BoardField.SetGoalBoardField(BoardFields[10, 2], false, true);
            BoardField.SetGoalBoardField(BoardFields[10, 4], true, true);
            BoardField.SetGoalBoardField(BoardFields[10, 5], true, false);
            BoardField.SetGoalBoardField(BoardFields[11, 3], false, true);
            BoardField.SetGoalBoardField(BoardFields[11, 4], true, false);

            BoardField.SetAgentBoardField(BoardFields[4, 2], 1, false, false);
            BoardField.SetAgentBoardField(BoardFields[7, 4], 2, false, true);
            BoardField.SetAgentBoardField(BoardFields[6, 5], 3, true, false);
            BoardField.SetAgentBoardField(BoardFields[8, 3], 4, true, false);

            BoardField.SetPieceBoardField(BoardFields[5, 1], false);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitPresentation();
            GetGameMasterConfiguration();
            GenerateBoard(BoardCanvas);
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
                timer.Start();
                IsStartedGamePaused = false;
                PauseRadioButton.Content = "Pause";
            }
            else
            {
                StartGame();
                SetAgentFields(BoardCanvas);
                //TODO:
                //do it better
                AgentsCountLabel.Content = gameMaster.Agents.Count.ToString();

                timer.Start();
                PauseRadioButton.IsEnabled = true;
            }
        }

        private void PauseRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            StartRadioButton.Content = "Resume";
            PauseRadioButton.Content = "Paused";
            IsStartedGamePaused = true;

            //pause game in game master
            timer.Stop();
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
    }
}