using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution
{
    public class Cell
    {
        public Direction direction;
        public State state;
        public InterpreterProgram program;
        public Point location;
        public byte energy = 10;
        public byte health = 10;
        public const int energyPerFood = 30;
        int age = 0;

        public void Eat ()
        {
            if (new Rectangle(0, 0, program.game.foodGrid.GetLength(0), program.game.foodGrid.GetLength(1)).Contains(location))
            {
                var food = program.game.foodGrid[location.X, location.Y];
                if(food > 0)
                {
                    //int foodCheck = program.game.FoodCheck();
                    int amountEaten = 1;// Game1.rnd.Next(food-1)+1;
                    Debug.Assert(amountEaten > 0 && amountEaten <= food);
                    unchecked
                    {
                        energy += (byte)(amountEaten * Cell.energyPerFood);
                        program.game.registers[0] = energy;
                    }
                    if (program.game.foodGrid[location.X, location.Y] != 255)
                    {
                        program.game.foodGrid[location.X, location.Y] -= (byte)amountEaten;
                    }
                    //Debug.Assert(foodCheck >= program.game.FoodCheck());
                }
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

        public static Direction Unpointify(Point p)
        {
            if(p.X == 0)
            {
                if(p.Y > 0)
                {
                    return Direction.Down;
                }
                else
                {
                    return Direction.Up;
                }
            }
            else if(p.X > 0)
            {
                return Direction.Right;
            }
            else
            {
                return Direction.Left;
            }
        }

        public Point WrapPoint(Point p)
        {
            int maxX = program.game.foodGrid.GetLength(0) - 1;
            int maxY = program.game.foodGrid.GetLength(1) - 1;

            if (p.X < 0)
                p.X = maxX;
            else if (p.X == maxX)
                p.X = 0;

            if (p.Y < 0)
                p.Y = maxY;
            else if (p.Y == maxY)
                p.Y = 0;

            return p;
        }

        public void Move ()
        {
            Point newPos = location + Pointify(direction);

            if (!program.game.toAdd.ContainsKey(newPos))
            {
                if (program.game.cells.ContainsKey(newPos))
                {
                    program.game.cells[newPos].health--;
                }
                else
                {
                    //Debug.Assert(!program.game.toRemove.Contains(location));
                    program.game.toRemove.Add(location);
                    //Debug.Assert(program.game.cells.ContainsKey(location));
                    location = newPos;
                    program.game.toAdd.Add(newPos, this);
                }
            }
        }

        public byte GetVision()
        {
            var target = this.location + Pointify(direction);
            if (new Rectangle(0, 0, program.game.foodGrid.GetLength(0), program.game.foodGrid.GetLength(1)).Contains(target))
            {
                if (program.game.cells.ContainsKey(target))
                    return 255;
                else
                    return program.game.foodGrid[target.X, target.Y];
            }
            return 254;
        }

        public void Turn (Direction value)
        {
            direction = (Direction)(((byte)direction + (byte)value) % (byte)Direction.Count);
        }
        
        public void StartBreed (Direction value)
        {
            if (energy > 2)
            {
                breed = new Point(0, 0) - Pointify((Direction)(((byte)direction + (byte)value) % (byte)Direction.Count));
                Point targetPos = WrapPoint(breed + location);
                if (program.game.cells.ContainsKey(targetPos))
                    return;

                byte halfEnergy = (byte)(energy / 2);
                Cell cell;
                if (program.game.dead.Count == 0)
                {
                    cell = new Cell();
                }
                else
                {
                    cell = program.game.dead.Dequeue();
                    //Debug.Assert(!program.game.cells.ContainsKey(cell.location) || program.game.cells[cell.location] != cell);
                }

                {
                    cell.location = targetPos;
                    cell.direction = Unpointify(new Point(0,0)-breed);
                    cell.energy = halfEnergy;
                    cell.program = new InterpreterProgram(program.game, new byte[] { }, cell.Eat, cell.Move, cell.Turn, cell.StartBreed, cell.WriteProgramBreed, cell.Die, cell.GetVision);
                    cell.state = State.Alive;
                    cell.age = 0;
                };
                energy -= halfEnergy;
                program.game.registers[0] = energy;
                program.game.toAdd[targetPos] = cell;
                age++;

                //if (age >= 10)
                //    Die();
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
                switch (rnd.Next(4))
                {
                    case 0:
                        v[rnd.Next(v.Length)] = (byte)(rnd.Next(255));
                        break;
                    case 1:
                        v[rnd.Next(v.Length)] += (byte)(rnd.Next(2) - 1);
                        break;
                    default:
                        return v;
                }

                /*else if (rnd.Next(2) == 1)
                {
                    var nc = new byte[v.Length + 1];
                    v.CopyTo(nc, 1);
                    nc[0] = (byte)rnd.Next(256);
                    v = nc;
                }
                else
                    return v;
                    */
            }
        }

        public void Die ()
        {
            //            int preFoodCheck = program.game.FoodCheck();
            state = State.Dead;
            Debug.Assert(!program.game.toRemove.Contains(location));
            program.game.toRemove.Add(location);
            Debug.Assert(program.game.cells.ContainsKey(location));

            if (energy >= energyPerFood && location.X >= 0 && location.Y >= 0 && location.X < program.game.foodGrid.GetLength(0) && location.Y < program.game.foodGrid.GetLength(1))
            {
                int oldFood = program.game.foodGrid[location.X, location.Y];
                int foodDropped = (int)(energy / energyPerFood);
                if (oldFood != 255)
                {
                    if (oldFood + foodDropped >= 255)
                        program.game.foodGrid[location.X, location.Y] = 254;
                    else if (oldFood + foodDropped > 0)
                        program.game.foodGrid[location.X, location.Y] = (byte)(oldFood + foodDropped);
                    else
                        program.game.foodGrid[location.X, location.Y] = 0;
                }

                energy = 0;
            }
   //         int postFoodCheck = program.game.FoodCheck();
     //       Debug.Assert(preFoodCheck >= postFoodCheck);
        }
    }
}
