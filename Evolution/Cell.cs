using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution
{
    public enum Direction
    {
        North,
        South,
        East,
        West
    };

    public class Cell
    {
        public Direction direction;

        public InterpreterProgram program;
    }
}
