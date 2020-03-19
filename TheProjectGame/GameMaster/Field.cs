using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
