using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster
{
    public class PresentationField
    {
        public bool hasPiece { get; private set; }
        public bool isSham { get; private set; }
        public FieldState State { get; private set; }

        public PresentationField(bool hasPiece, bool isSham, FieldState State)
        {
            this.hasPiece = hasPiece;
            this.isSham = isSham;
            this.State = State;
        }
    }
}
