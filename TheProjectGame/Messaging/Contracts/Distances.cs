using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts
{
    public class Distances
    {
        public int distanceFromCurrent;
        public int distanceN;
        public int distanceNE;
        public int distanceE;
        public int distanceSE;
        public int distanceS;
        public int distanceSW;
        public int distanceW;
        public int distanceNW;

        public Distances(int distanceFromCurrent, int distanceN, int distanceNE, int distanceE, int distanceSE, int distanceS, int distanceSW, int distanceW, int distanceNW)
        {
            this.distanceFromCurrent = distanceFromCurrent;
            this.distanceN = distanceN;
            this.distanceNE = distanceNE;
            this.distanceE = distanceE;
            this.distanceSE = distanceSE;
            this.distanceS = distanceS;
            this.distanceSW = distanceSW;
            this.distanceW = distanceW;
            this.distanceNW = distanceNW;
        }
    }
}
