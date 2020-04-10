using System.Collections.Generic;

namespace GameMaster
{
    public class Field
    {
        public FieldState State { get; set; } = FieldState.Empty;
        public Agent Agent { get; set; }
        public Stack<Piece> Pieces { get; } = new Stack<Piece>();

        public void Clean()
        {
            State = FieldState.Empty;
            Agent = null;
            Pieces.Clear();
        }

        public PresentationField GetPresentationField()
        {
            switch (State)
            {
                case FieldState.Empty:
                    bool hasPiece = Pieces.Count != 0;
                    bool isSham = false;
                    if (hasPiece)
                    {
                        isSham = Pieces.Peek().IsSham;
                    }
                    return new PresentationField(hasPiece, isSham, State);

                case FieldState.Goal:
                case FieldState.CompletedGoal:
                default:
                    return new PresentationField(false, false, State);
            }
        }
    }
}