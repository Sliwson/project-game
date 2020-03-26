using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    public class Field
    {
        public GoalInformation goalInfo;

        public int distToPiece;

        public DateTime distLearned;

        public DateTime deniedMove;

        public Field()
        {
            goalInfo = GoalInformation.NoInformation;
            distToPiece = int.MaxValue;
            distLearned = DateTime.MinValue;
            deniedMove = DateTime.MinValue;
        }
    }
}
