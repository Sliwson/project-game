using System.Windows.Controls;
using System.Windows.Media;

namespace GameMasterPresentation
{
    internal class BoardField
    {
        public BoardField(Canvas canvas, double width, double height, double x, double y, Color color)
        {
            this.canvas = canvas;
            label = new Label
            {
                Width = width,
                Height = height,
                Background = new SolidColorBrush(color),
                Foreground = new SolidColorBrush(Colors.Black),
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = width / 2
            };

            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, y);
            Panel.SetZIndex(label, 15);
            canvas.Children.Add(label);
        }

        public void SetText(string text)
        {
            label.Content = text;
        }

        public void SetTextColor(Color color)
        {
            label.Foreground = new SolidColorBrush(color);
        }

        public void SetBackgroundColor(Color color)
        {
            label.Background = new SolidColorBrush(color);
        }

        public void Move(double x, double y)
        {
            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, y);
        }

        public void Remove()
        {
            canvas.Children.Remove(label);
        }

        public void Reset()
        {
            label.Foreground = new SolidColorBrush(Colors.Transparent);
            label.Background = new SolidColorBrush(Colors.Transparent);
            label.Content = null;
            label.FontSize = label.Width / 2;
        }

        public static void SetAgentBoardField(BoardField boardField, int id, bool isRed, bool hasPiece)
        {
            boardField.Reset();
            boardField.label.FontSize = boardField.label.Width / 3;
            if (isRed == true)
                boardField.SetBackgroundColor(Colors.Red);
            else
                boardField.SetBackgroundColor(Colors.Blue);

            if (hasPiece == true)
                boardField.SetText(id.ToString() + "P");
            else
                boardField.SetText(id.ToString());
            boardField.SetTextColor(Colors.Black);
        }

        public static void SetPieceBoardField(BoardField boardField, bool isSham)
        {
            boardField.Reset();
            boardField.SetBackgroundColor(Colors.Black);
            boardField.SetTextColor(Colors.LightGray);
            if (isSham == true)
                boardField.SetText("SP");
            else
                boardField.SetText("P");
        }

        public static void SetGoalBoardField(BoardField boardField, bool isDiscovered, bool isGoal)
        {
            if (isGoal == false && isDiscovered == false)
                return;

            boardField.Reset();
            boardField.SetTextColor(Colors.Black);

            if (isGoal == false)
            {
                boardField.SetText("N");
                return;
            }

            boardField.SetBackgroundColor(Colors.Yellow);
            if (isDiscovered == true)
                boardField.SetText("YG");
            else
                boardField.SetText("G");
        }

        private Label label;

        private Canvas canvas;
    }
}