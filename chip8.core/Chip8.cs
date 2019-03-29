using System;
using System.Collections;
using System.Collections.Generic;

namespace chip8.core
{
    public class Chip8
    {
        private const int MEMORY_SIZE = 4096; //Chip8 machines had 4096bytes of memory.
        private const int START_PROGRAM_MEMORY = 512; //The first 512bytes of memory contained the chip8 interpreter + fonts etc.
        private const int STACK_SIZE = 16;

        //Graphics, Audio and Input are abstracted out so that we can easily port this core emulation engine to other platforms.
        private IGraphics Graphics;
        private IAudio Audio;
        private IInput Input;

        //Chip8 Memory - 4096bytes in size.
        private byte[] Memory { get; set; } = new byte[MEMORY_SIZE];
        //CPU Registers V0-VF 
        // (VF doubles as a flag for some instructions. Carry flag in addition)
        private byte[] V { get; set; } = new byte[16];
        //Index Register
        private ushort I { get; set; } = 0;
        //Program counter
        private ushort PC { get; set; } = 0;
        private Stack<ushort> Stack { get; set; } = new Stack<ushort>(STACK_SIZE);
        private ushort StackPointer { get; set; }

        //A struct to hold the Opcode + data from memory
        private struct OpCodeData
        {
            public ushort OpCode;
            public ushort NNN;
            public byte NN, X, Y, N;
            public byte Instruction;

            public override string ToString()
            {
                return $"{OpCode:X4} (X: {X:X}, Y: {Y:X}, N: {N:X}, NN: {NN:X2}, NNN: {NNN:X3})";
            }
        }

        public Chip8(IGraphics graphics, IAudio audio, IInput input)
        {
            this.Graphics = graphics;
            this.Audio = audio;
            this.Input = input;            
        }

        //This needs to be called in a loop, it gets the next instruction from memory along with the associated data
        //and then figures out what todo with the data.
        public void Tick() {
            //Read two bytes from memory for the opCode and advance the program counter.
            var opCode = (ushort)(Memory[PC++] << 8 | Memory[PC++]);

            //process the opCode + data and split it up into the various variables that may be needed for the opcode.
            var op = new OpCodeData() {
                OpCode = opCode,
                Instruction = (byte)(opCode & 0xF000),
                NNN = (ushort)(opCode & 0x0FFF),
                NN = (byte)(opCode & 0x00FF),
                N = (byte)(opCode & 0x00F),
                X = (byte)((opCode & 0x0F00) >> 8),
				Y = (byte)((opCode & 0x00F0) >> 4)
            };

            //Decode and execute the op codes
            switch (op.Instruction) {
                case 0x0000: {
                    if (op.X == 0x0) {
                        if (op.N == 0x0) {
                            //Clear screen
                            Graphics.ClearScreen();
                        } else if (op.N == 0xE) {
                            //Return from sub
                            PC = Stack.Pop();
                        }
                    }
                    break;
                }
            }
        }


        public override string ToString()
        {
            return "Hello World";
        }

    }
}
