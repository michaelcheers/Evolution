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
        public byte[] programReg = new byte[256];
        public Game1 game;
        public byte[] registers => game.registers;
        public Action Eat;
        public Action Move;
        public Action<Direction> Turn;
        public Action<Direction> StartBreed;
        public Action WriteProgramBreed;
        public Action Die;
        public Func<byte> GetVision;
        int location;

        public InterpreterProgram(Game1 game, byte[] program, Action eat, Action move, Action<Direction> turn, Action<Direction> StartBreed, Action WriteProgramBreed, Action Die, Func<byte> GetVision)
        {
            this.program = program;
            this.game = game;
            Eat = eat;
            Move = move;
            Turn = turn;
            this.StartBreed = StartBreed;
            this.WriteProgramBreed = WriteProgramBreed;
            this.Die = Die;
            this.GetVision = GetVision;
            this.location = 0;
        }

        public void Run(int cycles)
        {
            if (program.Length == 0)
                return;

            for (int ran = 0; ran < cycles; ran++)
            {
                if (location >= program.Length || location < 0)
                {
                    location = 0;
                }
                Instruction instruction = (Instruction)program[location];
                byte byte2 = (location+1 < program.Length)? program[location + 1]: (byte)0;
                byte byte3 = (location+2 < program.Length)? program[location + 2]: (byte)0;
                location += 3;

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
                            if (registers[byte3] == 0)
                                registers[byte2] = 255;
                            else
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
                            if (byte3 == 0)
                                registers[byte2] = 255;
                            else
                                registers[byte2] /= byte3;
                            break;
                        case Instruction.Jump:
                            location = (registers[byte2] << 8) + registers[byte3] - 3;
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
                            if (programReg.Length < program.Length)
                                programReg = program;
                            else
                                Array.Copy(program, programReg, program.Length);
                            break;
                        case Instruction.SetProgramRegisterAtIndex:
                            programReg[byte2] = registers[byte3];
                            break;
                        case Instruction.Move:
                            Move();
                            return;
                        case Instruction.Turn:
                            Turn((Direction)byte2);
                            break;
                        case Instruction.StartBreed:
                            StartBreed((Direction)byte2);
                            break;
                        case Instruction.WriteProgramBreed:
                            WriteProgramBreed();
                            return;
                        case Instruction.End:
                            return;
                        case Instruction.IfEqual0:
                            if (registers[byte2] != 0)
                                location += 3;
                            break;
                        case Instruction.IfNotEqual0:
                            if (registers[byte2] == 0)
                                location += 3;
                            break;
                        case Instruction.IfGreater:
                            if (registers[byte2] <= registers[byte3])
                                location += 3;
                            break;
                        case Instruction.GetVision:
                            registers[byte2] = GetVision();
                            break;
                        default:
                            location++;
                            break;
                            //throw new Exception();
                    }
                }
            }
        }
    }
}