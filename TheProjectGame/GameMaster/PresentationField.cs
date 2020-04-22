namespace GameMaster
{
    public class PresentationField
    {
        public bool HasPiece { get; private set; }
        public bool IsSham { get; private set; }
        public FieldState State { get; private set; }

        public PresentationField(bool hasPiece, bool isSham, FieldState State)
        {
            this.HasPiece = hasPiece;
            this.IsSham = isSham;
            this.State = State;
        }

        public PresentationField(Field field)
        {
            if (field.State == FieldState.Empty)
            {
                HasPiece = field.Pieces.Count != 0;
                IsSham = false;
                if (HasPiece)
                {
                    IsSham = field.Pieces.Peek().IsSham;
                }
            }
            else
            {
                HasPiece = false;
                IsSham = false;
                State = field.State;
            }
        }
    }
}