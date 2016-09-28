using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution
{
    public enum Instruction : byte
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        AddConstant,
        SubtractConstant,
        MultiplyConstant,
        DivideConstant,
        Negate,
        Jump,
        RegisterCopy,
        RegisterSet,
        CopyRegisterPointer,
        Move,
        Eat,
        SetProgramToRegister,
        SetProgramRegisterAtIndex,
        Turn,
        StartBreed,
        WriteProgramBreed,
        IfEqual0,
        IfNotEqual0,
        IfGreater,
        End,
        Die,
        GetVision,
        TurnConstant
    }
}
