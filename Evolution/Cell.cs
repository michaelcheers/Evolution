using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution
{
    public class Cell
    {
        public Direction direction;

        public InterpreterProgram program;
        public Point location;
        public byte energy = 10;
        public byte health = 10;

        public void Eat ()
        {
            if (new Rectangle(0, 0, program.game.foodGrid.GetLength(0), program.game.foodGrid.GetLength(1)).Contains(location))
            {
                var food = program.game.foodGrid[location.X, location.Y];
                unchecked
                {
                    energy += (byte)(food * 6);
                    program.game.registers[0] = energy;
                }
                program.game.foodGrid[location.X, location.Y] = 0;
            }
        }

        public static Point Pointify (Direction value)
        {
            Point change;
            switch (value)
            {
                case Direction.Left:
                    change = new Point(-1, 0);
                    break;
                case Direction.Up:
                    change = new Point(0, -1);
                    break;
                case Direction.Down:
                    change = new Point(0, 1);
                    break;
                case Direction.Right:
                    change = new Point(1, 0);
                    break;
                default:
                    throw new Exception();
            }
            return change;
        }

        public void Move ()
        {
            Point change = Pointify(direction);
            if (!program.game.toAdd.ContainsKey(location - change))
            {
                if (program.game.cells.ContainsKey(location - change))
                {
                    program.game.cells[location - change].health--;
                }
                program.game.toRemove.Add(location);
                location -= change;
                program.game.toAdd.Add(location, this);
            }
        }

        public byte GetVision ()
        {

        }

        public void Turn (Direction value)
        {
            direction = (Direction)(((byte)direction + (byte)value) % (byte)Direction.Count);
        }
        
        public void StartBreed (Direction value)
        {
            if (energy >= 2)
            {
                breed = Pointify((Direction)(((byte)direction + (byte)value) % (byte)Direction.Count));
                Point targetPos = breed + location;
                if (program.game.cells.ContainsKey(targetPos))
                    return;
                Cell cell = new Cell();
                {
                    cell.location = breed + location;
                    cell.direction = Direction.Up;
                    cell.energy = 2;
                    cell.program = new InterpreterProgram(program.game, new byte[] { }, cell.Eat, cell.Move, cell.Turn, cell.StartBreed, cell.WriteProgramBreed, cell.Die);
                };
                energy -= 2;
                program.game.registers[0] = energy;
                program.game.toAdd[breed + location] = cell;
            }
        }

        public Point breed;
        
        public void WriteProgramBreed ()
        {
            if (program.game.toAdd.ContainsKey(location + breed))
            program.game.toAdd[location + breed].program.program = Corrupt(Enumerable.Concat(program.game.toAdd[location + breed].program.program, program.programReg).ToArray());
        }

        private static byte[] Corrupt(byte[] v)
        {
            Random rnd = Game1.rnd;
            while (true)
            {
                if (rnd.Next(2) == 1)
                {
                    v[rnd.Next(v.Length)] += (byte)((rnd.Next(2) * 2) - 1);
                }
                else
                    return v;
            }
        }

        public void Die ()
        {
            program.game.toRemove.Add(location);
        }
    }
}
