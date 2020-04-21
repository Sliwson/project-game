using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameMasterPresentation
{
    public class BoardComponent : INotifyPropertyChanged
    {
        private List<Line> BoardMesh;
        private List<Label> BoardAreas;
        private BoardField[,] BoardFields;
        private BoardField[] AgentFields;

        private Canvas canvas;

        private bool IsInitialized;

        private int BoardRows;
        private int BoardColumns;
        private int BoardGoalAreaRows;
        private double FieldSize;

        private int _redTeamScore;

        public int RedTeamScore
        {
            get
            {
                return _redTeamScore;
            }
            set
            {
                _redTeamScore = value;
                NotifyPropertyChanged();
            }
        }

        private int _blueTeamScore;

        public int BlueTeamScore
        {
            get
            {
                return _blueTeamScore;
            }
            set
            {
                _blueTeamScore = value;
                NotifyPropertyChanged();
            }
        }

        private GameMaster.GameMasterState _gmstate;
        public GameMaster.GameMasterState GMState
        {
            get
            {
                return _gmstate;
            }
            set
            {
                _gmstate = value;
                NotifyPropertyChanged();
            }
        }

        private int HorizontalLineZIndex = Constants.HorizontalLineZIndex;
        private int VerticalLineZIndex = Constants.VerticalLineZIndex;
        private int BackgroundZIndex = Constants.BackgroundZIndex;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BoardComponent(Canvas canvas)
        {
            this.canvas = canvas;
            IsInitialized = false;
            BoardRows = -1;
            BoardColumns = -1;
            BoardGoalAreaRows = -1;
            FieldSize = -1d;
            BlueTeamScore = 0;
            RedTeamScore = 0;
        }

        public void InitializeBoard(int AgentsCount, Configuration.Configuration configuration)
        {
            this.BoardRows = configuration.BoardY;
            this.BoardColumns = configuration.BoardX;
            this.BoardGoalAreaRows = configuration.GoalAreaHeight;
            CalculateBoardSize();
            SetBoardMesh();
            SetAreasBackgrounds();
            SetBoardFields();
            SetAgentFields(AgentsCount);
            IsInitialized = true;
        }

        public void UpdateBoard(GameMaster.PresentationData data)
        {
            if (IsInitialized)
            {
                if (data.Agents.Count != AgentFields.Length)
                    ;//TODO: error something
                int i = 0;
                foreach (var agent in data.Agents)
                {
                    SetSingleAgent(agent, AgentFields[i]);
                    i++;
                }

                for (int y = 0; y < BoardRows; y++)
                {
                    for (int x = 0; x < BoardColumns; x++)
                    {
                        SetSingleBoardField(data.BoardFields[y, x], BoardFields[y, x]);
                    }
                }

                RedTeamScore = data.Score.RedTeamScore;
                BlueTeamScore = data.Score.BlueTeamScore;
                CheckGameResult(data.Score.GameResult);
            }
            GMState = data.State;
        }

        private void CalculateBoardSize()
        {
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
        }

        private void SetBoardMesh()
        {
            BoardMesh = new List<Line>();
            //lines
            //vertical
            double pointX = 0d;
            double pointY = 0d;
            double point2X = 0d;
            double point2Y = FieldSize * BoardRows;
            for (int i = 0; i < BoardColumns + 1; i++)
            {
                int lineThickness = 1;
                if (i == 0 || i == BoardColumns)
                    lineThickness = 3;
                CreateLine(pointX, pointY, point2X, point2Y, lineThickness, VerticalLineZIndex);
                pointX += FieldSize;
                point2X += FieldSize;
            }
            //horizontal
            pointX = 0d;
            pointY = 0d;
            point2X = FieldSize * BoardColumns;
            point2Y = 0d;
            for (int i = 0; i < BoardRows + 1; i++)
            {
                int lineThickness = 1;
                if (i == 0 || i == BoardRows || i == BoardGoalAreaRows || i == BoardRows - BoardGoalAreaRows)
                    lineThickness = 3;
                CreateLine(pointX, pointY, point2X, point2Y, lineThickness, HorizontalLineZIndex);
                pointY += FieldSize;
                point2Y += FieldSize;
            }
        }

        private void CreateLine(double pointX, double pointY, double point2X, double point2Y, int lineThickness, int lineZIndex)
        {
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
            Panel.SetZIndex(line, lineZIndex);
            canvas.Children.Add(line);
        }

        private void SetAreasBackgrounds()
        {
            BoardAreas = new List<Label>();

            //blue goal
            Label label1 = new Label
            {
                Background = new SolidColorBrush(Color.FromRgb(115, 194, 251)),
                Width = FieldSize * BoardColumns,
                Height = FieldSize * BoardGoalAreaRows
            };
            Canvas.SetLeft(label1, 0);
            Canvas.SetTop(label1, FieldSize * (BoardRows - BoardGoalAreaRows));
            Panel.SetZIndex(label1, BackgroundZIndex);

            BoardAreas.Add(label1);
            canvas.Children.Add(label1);

            //task area
            Label label2 = new Label
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Width = FieldSize * BoardColumns,
                Height = FieldSize * (BoardRows - 2 * BoardGoalAreaRows)
            };

            Canvas.SetLeft(label2, 0);
            Canvas.SetTop(label2, FieldSize * BoardGoalAreaRows);
            Panel.SetZIndex(label2, BackgroundZIndex);

            BoardAreas.Add(label2);
            canvas.Children.Add(label2);

            //red goal
            Label label3 = new Label
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 190, 188)),
                Width = FieldSize * BoardColumns,
                Height = FieldSize * BoardGoalAreaRows
            };

            Canvas.SetLeft(label3, 0);
            Canvas.SetTop(label3, 0);
            Panel.SetZIndex(label3, BackgroundZIndex);

            BoardAreas.Add(label3);
            canvas.Children.Add(label3);
        }

        private void SetBoardFields()
        {
            BoardFields = new BoardField[BoardRows, BoardColumns];
            //fields
            double pointX = 0d;
            double pointY = FieldSize * (BoardRows - 1);
            for (int y = 0; y < BoardRows; y++)
            {
                for (int x = 0; x < BoardColumns; x++)
                {
                    BoardFields[y, x] = new BoardField(canvas, FieldSize, FieldSize, pointX, pointY, Colors.Transparent);
                    pointX += FieldSize;
                }
                pointX = 0d;
                pointY -= FieldSize;
                if (pointY < 0)
                    pointY = 0d;
            }
        }

        private void SetAgentFields(int AgentsCount)
        {
            AgentFields = new BoardField[AgentsCount];
            for (int i = 0; i < AgentFields.Length; i++)
            {
                AgentFields[i] = new BoardField(canvas, FieldSize, FieldSize, 0, 0, Colors.Transparent);
            }
        }

        private void SetSingleAgent(GameMaster.PresentationAgent agent, BoardField boardField)
        {
            var pos = agent.Position;
            double pointX = FieldSize * pos.X;
            double pointY = FieldSize * (BoardRows - 1 - pos.Y);
            bool isRed = agent.Team == Messaging.Enumerators.TeamId.Red ? true : false;
            boardField.Move(pointX, pointY);
            BoardField.SetAgentBoardField(boardField, agent.Id, isRed, agent.HasPiece);
        }

        private void SetSingleBoardField(GameMaster.PresentationField field, BoardField boardField)
        {
            boardField.Reset();
            switch (field.State)
            {
                case GameMaster.FieldState.Empty:
                    if (field.HasPiece)
                        BoardField.SetPieceBoardField(boardField, field.IsSham);
                    break;

                case GameMaster.FieldState.Goal:
                    //TODO:
                    //define when goal is completed
                    BoardField.SetGoalBoardField(boardField, false, true);
                    break;

                case GameMaster.FieldState.CompletedGoal:
                    BoardField.SetGoalBoardField(boardField, true, true);
                    break;

                case GameMaster.FieldState.DiscoveredNonGoal:
                    BoardField.SetGoalBoardField(boardField, true, false);
                    break;

                default:
                    break;
            }
        }

        private void CheckGameResult(GameMaster.Enums.GameResult result)
        {
            switch (result)
            {
                case GameMaster.Enums.GameResult.None:
                    break;
                case GameMaster.Enums.GameResult.BlueWin:
                    MessageBox.Show("Blue Team Won!", Constants.GameMasterMessageBoxName, MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case GameMaster.Enums.GameResult.RedWin:
                    MessageBox.Show("Red Team Won!", Constants.GameMasterMessageBoxName, MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case GameMaster.Enums.GameResult.Draw:
                    MessageBox.Show("Draw!", Constants.GameMasterMessageBoxName, MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                default:
                    break;
            }
        }
    }
}