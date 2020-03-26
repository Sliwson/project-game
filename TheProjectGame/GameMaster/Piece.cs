using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster
{
    public class Piece
    {
        public bool IsSham { get; private set; }

        public Piece(bool isSham)
        {
            IsSham = isSham;
        }
    }
}
