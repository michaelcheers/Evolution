﻿using Microsoft.Xna.Framework;
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

        public InterpreterProgram program;
        public Point location;
        public byte energy = 10;
        public byte health = 10;
        public const int energyPerFood = 30;

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
                    program.game.foodGrid[location.X, location.Y] -= amountEaten;
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
            Point change;
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

        public void Move ()
        {
            Point newPos = location + Pointify(direction);
            int maxX = program.game.foodGrid.GetLength(0) - 1;
            int maxY = program.game.foodGrid.GetLength(1) - 1;

            if (newPos.X < 0)
                newPos.X = maxX;
            else if (newPos.X == maxX)
                newPos.X = 0;

            if (newPos.Y < 0)
                newPos.Y = maxY;
            else if (newPos.Y == maxY)
                newPos.Y = 0;

            if (!program.game.toAdd.ContainsKey(newPos))
            {
                if (program.game.cells.ContainsKey(newPos))
                {
                    program.game.cells[newPos].health--;
                }
                else
                {
                    program.game.toRemove.Add(location);
                    location = newPos;
                    program.game.toAdd.Add(newPos, this);
                }
            }
        }

        public byte GetVision ()
        {
            return 0;
        }

        public void Turn (Direction value)
        {
            direction = (Direction)(((byte)direction + (byte)value) % (byte)Direction.Count);
        }
        
        public void StartBreed (Direction value)
        {
            if (energy > 2)
            {
                breed = new Point(0,0)-Pointify((Direction)(((byte)direction + (byte)value) % (byte)Direction.Count));
                Point targetPos = breed + location;
                if (program.game.cells.ContainsKey(targetPos))
                    return;

                byte halfEnergy = (byte)(energy / 2);
                Cell cell = new Cell();
                {
                    cell.location = breed + location;
                    cell.direction = Unpointify(new Point(0,0)-breed);
                    cell.energy = halfEnergy;
                    cell.program = new InterpreterProgram(program.game, new byte[] { }, cell.Eat, cell.Move, cell.Turn, cell.StartBreed, cell.WriteProgramBreed, cell.Die);
                };
                energy -= halfEnergy;
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
            }
        }

        public void Die ()
        {
//            int preFoodCheck = program.game.FoodCheck();
            program.game.toRemove.Add(location);
            if(energy >= energyPerFood && location.X >= 0 && location.Y >= 0 && location.X < program.game.foodGrid.GetLength(0) && location.Y < program.game.foodGrid.GetLength(1))
            {
                program.game.foodGrid[location.X, location.Y] += energy / energyPerFood;
                energy = 0;
            }
   //         int postFoodCheck = program.game.FoodCheck();
     //       Debug.Assert(preFoodCheck >= postFoodCheck);
        }
    }
}
