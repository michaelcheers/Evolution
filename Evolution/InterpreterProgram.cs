using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution
{
    public class InterpreterProgram
    {
        public byte[] program;
        public byte[] programReg;
        public Game1 game;
        public byte[] registers => game.registers;
        public Action Eat;
        public Action Move;
        public Action<Direction> Turn;
        public InterpreterProgram(Game1 game, byte[] program, Action eat, Action move, Action<Direction> turn)
        {
            this.program = program;
            this.programReg = new byte[] { };
            this.game = game;
            Eat = eat;
            Move = move;
            Turn = turn;
        }

        public void Run(int cycles)
        {
            for (int n = 0, location = 0; n < cycles; n++, location += 3)
            {
                Instruction instruction = (Instruction)program[n];
                byte byte2 = program[n + 1];
                byte byte3 = program[n + 2];
                unchecked
                {
                    switch (instruction)
                    {
                        case Instruction.Add:
                            registers[byte2] += registers[byte3];
                            break;
                        case Instruction.Subtract:
                            registers[byte2] -= registers[byte3];
                            break;
                        case Instruction.Multiply:
                            registers[byte2] *= registers[byte3];
                            break;
                        case Instruction.Divide:
                            registers[byte2] /= registers[byte3];
                            break;
                        case Instruction.AddConstant:
                            registers[byte2] += byte3;
                            break;
                        case Instruction.SubtractConstant:
                            registers[byte2] -= byte3;
                            break;
                        case Instruction.MultiplyConstant:
                            registers[byte2] *= byte3;
                            break;
                        case Instruction.DivideConstant:
                            registers[byte2] /= byte3;
                            break;
                        case Instruction.Jump:
                            location = (byte2 << 8) + byte3;
                            break;
                        case Instruction.RegisterCopy:
                            registers[byte2] = registers[byte3];
                            break;
                        case Instruction.RegisterSet:
                            registers[byte2] = byte3;
                            break;
                        case Instruction.CopyRegisterPointer:
                            registers[byte2] = registers[byte3];
                            break;
                        case Instruction.Eat:
                            Eat();
                            return;
                        case Instruction.SetProgramToRegister:
                            programReg = program;
                            break;
                        case Instruction.SetProgramRegisterAtIndex:
                            programReg[byte2] = registers[byte3];
                            break;
                        case Instruction.Move:
                            Move();
                            break;
                        case Instruction.Turn:
                            Turn((Direction)byte2);
                            break;
                        default:
                            throw new Exception();
                    }
                }
            }
        }
    }
}