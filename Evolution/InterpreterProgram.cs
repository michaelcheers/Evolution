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
        public byte[] programReg = new byte[0];
        public Game1 game;
        public byte[] registers => game.registers;
        public Action Eat;
        public Action Move;
        public Action<Direction> Turn;
        public Func<Direction, bool> StartBreed;
        public Action WriteProgramBreed;
        public Action Die;
        public Func<byte> GetVision;
        public Func<bool> OnFood;
        public delegate void EnergyFn(Direction dir, byte amount);
        public EnergyFn GiveEnergy;
        int pc;

        public InterpreterProgram(Game1 game, byte[] program, Action Eat, Action Move, Action<Direction> Turn, Func<Direction, bool> StartBreed, Action WriteProgramBreed, Action Die, Func<byte> GetVision, Func<bool> OnFood, EnergyFn GiveEnergy)
        {
            this.program = program;
            this.game = game;
            this.Eat = Eat;
            this.Move = Move;
            this.Turn = Turn;
            this.StartBreed = StartBreed;
            this.WriteProgramBreed = WriteProgramBreed;
            this.Die = Die;
            this.GetVision = GetVision;
            this.OnFood = OnFood;
            this.GiveEnergy = GiveEnergy;
            this.pc = 0;
        }

        public void Run(int cycles)
        {
            if (program.Length == 0)
                return;

            for (int ran = 0; ran < cycles; ran++)
            {
                if (pc >= program.Length || pc < 0)
                {
                    pc = 0;
                }
                Instruction instruction = (Instruction)program[pc];
                byte byte2 = (pc+1 < program.Length)? program[pc + 1]: (byte)0;
                byte byte3 = (pc+2 < program.Length)? program[pc + 2]: (byte)0;
                pc += 3;

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
                            pc = (registers[byte2] << 8) + registers[byte3] - 3;
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
                            if (programReg.Length > registers[byte2])
                                programReg[registers[byte2]] = registers[byte3];
                            else
                                Array.Resize(ref programReg, registers[byte2] + 1);
                            break;
                        case Instruction.Move:
                            Move();
                            return;
                        case Instruction.Turn:
                            Turn((Direction)registers[byte2]);
                            break;
                        case Instruction.TurnConstant:
                            Turn((Direction)byte2);
                            break;
                        case Instruction.StartBreed:
                            if (StartBreed((Direction)byte2))
                                return;
                            break;
                        case Instruction.WriteProgramBreed:
                            WriteProgramBreed();
                            return;
                        case Instruction.End:
                            return;
                        case Instruction.IfEqual0:
                            if (registers[byte2] != 0)
                                pc += 3;
                            break;
                        case Instruction.IfNotEqual0:
                            if (registers[byte2] == 0)
                                pc += 3;
                            break;
                        case Instruction.IfGreater:
                            if (registers[byte2] <= registers[byte3])
                                pc += 3;
                            break;
                        case Instruction.IfNotOnFood:
                            if (OnFood())
                                pc += 3;
                            break;
                        case Instruction.GetVision:
                            registers[byte2] = GetVision();
                            break;
                        case Instruction.GiveEnergy:
                            GiveEnergy((Direction)byte2, byte3);
                            return;
                        default:
                            pc++;
                            break;
                            //throw new Exception();
                    }
                }
            }
        }
    }
}