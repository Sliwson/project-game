using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace GameMasterPresentation
{
    class BoardComponent
    {
        private List<Line> BoardMesh;
        private List<Label> BoardGoalAreas;
        private BoardField[,] BoardFields;
        private BoardField[] AgentFields;

        private int BoardRows;
        private int BoardColumns;
        private int BoardGoalAreaRows;
        private double FieldSize;
    }
}
