using System;
using System.Collections;
using System.Collections.Generic;

namespace chip8.core
{
    public class Chip8
    {
        public const int MEMORY_SIZE = 4096; //Chip8 machines had 4096bytes of memory.
        public const int START_PROGRAM_MEMORY = 0x200; //The first 512bytes of memory contained the chip8 interpreter + fonts etc.
        public const int STACK_SIZE = 16;

        //Graphics, Audio and Input are abstracted out so that we can easily port this core emulation engine to other platforms.
        private IGraphics Graphics;
        private IAudio Audio;
        private IInput Input;

        //Chip8 Memory - 4096bytes in size.
        public byte[] Memory { get; set; } = new byte[MEMORY_SIZE];
        //CPU Registers V0-VF 
        // (VF doubles as a flag for some instructions. Carry flag in addition)
        public byte[] V { get; set; } = new byte[16];
        //Index Register
        public ushort I { get; set; } = 0;
        //Program counter
        public ushort PC { get; set; } = 0;
        //Stack
        public Stack<ushort> Stack { get; set; } = new Stack<ushort>(STACK_SIZE);
        public ushort StackPointer { get; set; }
        //Timers
        public byte DelayTimer { get; set; } = 0;
        public byte SoundTimer { get; set; } = 0;        
        private DateTime LastTimerEvent;
        //A struct to hold the Opcode + data from memory
        public struct OpCodeData
        {
            public ushort OpCode;
            public ushort NNN;
            public byte NN, X, Y, N;
            public ushort Instruction;

            public override string ToString()
            {
                return $"{OpCode:X4} Instruction: {Instruction:X2} (X: {X:X}, Y: {Y:X}, N: {N:X}, NN: {NN:X2}, NNN: {NNN:X3})";
            }
        }

        public Chip8(IGraphics graphics, IAudio audio, IInput input)
        {
            this.Graphics = graphics;
            this.Audio = audio;
            this.Input = input;
        }

        //Init routines to setup the default values for registers etc, set the program counter and load the fonts.
        //Needs to be called after loading a ROM image into memory
        private void Init()
        {
            I = 0;
            StackPointer = 0;
            PC = START_PROGRAM_MEMORY;

            DelayTimer = 0;
            SoundTimer = 0;
            LastTimerEvent = DateTime.Now;

            ApplyFontSet(); //Load the fonts into memory
        }

        //Load the ROM byte array into the memory starting at START_PROGRAM_MEMORY (512) and then init the registers etc.
        public void LoadROM(byte[] ROM)
        {

            Array.Copy(ROM, 0, Memory, START_PROGRAM_MEMORY, ROM.Length);

            Init();
        }

        //This needs to be called in a loop, it gets the next instruction from memory along with the associated data
        //and then figures out what todo with the data.
        public void Tick()
        {
            //Read two bytes from memory for the opCode and advance the program counter.
            var opCode = (ushort)(Memory[PC++] << 8 | Memory[PC++]);

            //process the opCode + data and split it up into the various variables that may be needed for the opcode.
            var op = new OpCodeData()
            {
                OpCode = opCode,
                Instruction = (ushort)((opCode & 0xF000) >> 12),
                NNN = (ushort)(opCode & 0x0FFF),
                NN = (byte)(opCode & 0x00FF),
                N = (byte)(opCode & 0x00F),
                X = (byte)((opCode & 0x0F00) >> 8),
                Y = (byte)((opCode & 0x00F0) >> 4)
            };

            //Decode and execute the op codes
            switch (op.Instruction)
            {
                //Clear Screen / Return from subroutine
                case 0x0:
                    {
                        if (op.X == 0x0)
                        {
                            if (op.N == 0x0)
                            {
                                //Clear screen
                                Graphics.ClearScreen();
                            }
                            else if (op.N == 0xE)
                            {
                                //Return from sub
                                PC = Stack.Pop();
                            }
                        }
                        break;
                    }

                //0x1NNNN - Jumps to address NNN.
                case 0x1:
                    {
                        PC = (ushort)(op.NNN);
                        break;
                    }

                //0x2NNN - Calls subroutine at NNN.
                case 0x2:
                    {
                        Stack.Push((ushort)(PC )); //You need to subtract 2 from the PC as its incremented above to the next position already when its reached here.
                        PC = (ushort)(op.NNN);
                        break;
                    }

                //0x3XNN - Skips the next instruction if VX equals NN. (Usually the next instruction is a jump to skip a code block)
                case 0x3:
                    {
                        if (V[op.X] == op.NN) { PC += 2; }
                        break;
                    }

                //0x4XNN - Skips the next instruction if VX doesn't equal NN. (Usually the next instruction is a jump to skip a code block)
                case 0x4:
                    {
                        if (V[op.X] != op.NN) { PC += 2; }
                        break;
                    }

                //0x5XY0 - 	Skips the next instruction if VX equals VY. (Usually the next instruction is a jump to skip a code block)
                case 0x5:
                    {
                        if (V[op.X] == V[op.Y]) { PC += 2; }
                        break;
                    }

                //0x6XNN - 	Sets VX to NN.
                case 0x6:
                    {
                        V[op.X] = op.NN;
                        break;
                    }

                //0x7XNN - Adds NN to VX. (Carry flag is not changed)
                case 0x7:
                    {
                        V[op.X] += op.NN;
                        break;
                    }

                //0x8XY? - BitWise / Math operations based on ?
                case 0x8:
                    {
                        //Switch based on the last nibble of the opcode
                        switch (op.N)
                        {
                            case 0x0: // 8XY0 - Sets VX to the value of VY.
                                {
                                    V[op.X] = V[op.Y];
                                    break;
                                }
                            case 0x1: // 8XY1 - Sets VX to VX or VY. (Bitwise OR operation)
                                {
                                    V[op.X] |= V[op.Y];
                                    break;
                                }
                            case 0x2: // 8XY2 - Sets VX to VX and VY. (Bitwise AND operation)
                                {
                                    V[op.X] &= V[op.Y];
                                    break;
                                }
                            case 0x3: // 8XY3 - Sets VX to VX xor VY.
                                {
                                    V[op.X] ^= V[op.Y];
                                    break;
                                }
                            case 0x4: // 8XY4 - Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
                                {
                                    if ((V[op.X] + V[op.Y]) > 255)
                                    {
                                        V[0xF] = 1;
                                    }
                                    else
                                    {
                                        V[0xF] = 0;
                                    }

                                    V[op.X] = (byte)(V[op.X] + V[op.Y]);
                                    break;
                                }
                            case 0x5: // 8XY5 - VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                                {
                                    if (V[op.X] > V[op.Y])
                                    {
                                        V[0xF] = 1;
                                    }
                                    else
                                    {
                                        V[0xF] = 0;
                                    }

                                    V[op.X] = (byte)(V[op.X] - V[op.Y]);
                                    break;
                                }
                            case 0x6: // 8XY6 - Stores the least significant bit of VX in VF and then shifts VX to the right by 1.
                                {
                                    V[0xF] = (byte)(V[op.X] & 0x1);
                                    V[op.X] >>= 1;
                                    break;
                                }
                            case 0x7: // 8XY7 - Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                                {
                                    if (V[op.Y] > V[op.X])
                                    {
                                        V[0xF] = 1;
                                    }
                                    else
                                    {
                                        V[0xF] = 0;
                                    }

                                    V[op.X] = (byte)(V[op.Y] - V[op.X]);
                                    break;
                                }
                            case 0xE: // 8XYE - Stores the most significant bit of VX in VF and then shifts VX to the left by 1.
                                {
                                    V[0xF] = (byte)(V[op.X] >> 7);
                                    V[op.X] <<= 1;
                                    break;
                                }
                        }
                        break;
                    }

                //0x9XY0 - Skips the next instruction if VX doesn't equal VY. (Usually the next instruction is a jump to skip a code block)
                case 0x9:
                    {
                        if (V[op.X] != V[op.Y]) { PC += 2; }
                        break;
                    }

                //0xANNN - Sets I to the address NNN.
                case 0xA:
                    {
                        I = op.NNN;
                        break;
                    }

                //0xBNNN - Jumps to the address NNN plus V0.
                case 0xB:
                    {
                        PC = (ushort)(op.NNN + V[0]);
                        break;
                    }

                //0xCXNN - Sets VX to the result of a bitwise and operation on a random number (Typically: 0 to 255) and NN.
                case 0xC:
                    {
                        V[op.X] = (byte)(new Random().Next(255) & op.NN);
                        break;
                    }

                //0xDXYN - Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
                //         Each row of 8 pixels is read as bit-coded starting from memory location I; 
                //         I value doesn’t change after the execution of this instruction. 
                //         As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite 
                //         is drawn, and to 0 if that doesn’t happen
                case 0xD:
                    {
                        byte[] sprite = new byte[op.N];
                        Array.Copy(Memory, I, sprite, 0, op.N);

                        bool pixelChanged = Graphics.DrawSprite(V[op.X], V[op.Y], op.N, sprite);

                        if (pixelChanged)
                        { V[0xF] = 1; }
                        else { V[0xF] = 0; }

                        break;
                    }

                //0xEX?? - Handles key presses
                case 0xE:
                    {
                        //EX9E - Skips the next instruction if the key stored in VX is pressed. (Usually the next instruction is a jump to skip a code block)
                        if (op.N == 0xE)
                        {
                            //Check to see if the key stored in VX was pressed
                            if (Input.KeyPressed(V[op.X]))
                            {
                                PC += 2; //Skip over the next instruction
                            }
                        }
                        else
                        {
                            //EXA1	- Skips the next instruction if the key stored in VX isn't pressed. (Usually the next instruction is a jump to skip a code block)
                            //Check to see if the key stored in VX wasn't pressed
                            if (!Input.KeyPressed(V[op.X]))
                            {
                                PC += 2; //Skip over the next instruction
                            }
                        }
                        break;
                    }

                case 0xF:
                    {
                        switch (op.NN)
                        {
                            //0xFX07 - Sets VX to the value of the delay timer.
                            case 0x07:
                                {
                                    V[op.X] = DelayTimer;
                                    break;
                                }
                            //0xF0A - A key press is awaited, and then stored in VX. (Blocking Operation. All instruction halted until next key event)
                            case 0x0A:
                                {
                                    byte keyPressed = 0x00;
                                    //Check if a key has been pressed
                                    if (Input.WaitForKey(out keyPressed))
                                    {
                                        //If a key has been pressed put it in VX
                                        V[op.X] = keyPressed;
                                    }
                                    else
                                    {
                                        PC -= 2; //If no key was pressed move the program counter one instruction back so it keep repeating this instruction.
                                    }
                                    break;
                                }
                            //0xFX15 - Sets the delay timer to VX.
                            case 0x15:
                                {
                                    DelayTimer = V[op.X];
                                    break;
                                }
                            //0xFX18 - Sets the sound timer to VX.
                            case 0x18:
                                {
                                    SoundTimer = V[op.X];
                                    break;
                                }
                            //0xFX1E - Adds VX to I
                            case 0x1E:
                                {
                                    I += V[op.X];
                                    break;
                                }
                            //0xFX29 - Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                            case 0x29:
                                {
                                    I = (ushort)(V[op.X] * 5);
                                    break;
                                }
                            //0xFX33 - Stores the binary-coded decimal representation of VX, with the most significant of three digits at the address in I, the middle digit at I plus 1, and the least significant digit at I plus 2. 
                            case 0x33:
                                {
                                    decimal num = V[op.X];
                                    Memory[I] = (byte)(num / 100);
                                    Memory[I + 1] = (byte)((num % 100) / 10);
                                    Memory[I + 2] = (byte)((num % 100) % 10);
                                    break;
                                }
                            //0xF55 - Stores V0 to VX (including VX) in memory starting at address I. The offset from I is increased by 1 for each value written, but I itself is left unmodified.
                            case 0x55:
                                {
                                    for (int i = 0; i <= op.X; i++)
                                    {
                                        Memory[I + i] = V[i];
                                    }
                                    break;
                                }
                            //0xFX65 - Fills V0 to VX (including VX) with values from memory starting at address I. The offset from I is increased by 1 for each value written, but I itself is left unmodified.
                            case 0x65:
                                {
                                    for (int i = 0; i <= op.X; i++)
                                    {
                                        V[i] = Memory[I + i];
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception("Unknown OpCode: " + opCode.ToString());
                    }
            }

            //Run the countdown timer code every 60hz or every 16ms +/- 
            if (DateTime.Now.Subtract(LastTimerEvent).Milliseconds >= 16)
            {
                if (DelayTimer > 0) { DelayTimer--; }
                if (SoundTimer > 0)
                {
                    SoundTimer--;
                    if (SoundTimer == 0) { Audio.Beep(); }
                }
                LastTimerEvent = DateTime.Now;
            }
        }

        //Load the fonts into memory at address 0.
        private void ApplyFontSet()
        {
            byte[] fontset = new byte[80]
            {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80  // F
            };

            for (int i = 0; i < fontset.Length; ++i)
            {
                Memory[i] = fontset[i];
            }
        }

        public override string ToString()
        {
            string s = $"PC:{PC} I:{I} ";
            for (int i = 0; i < 16; i++)
            {
                s += $"v{i:X}={V[i]:X} ";
            }

            return s;
        }

    }
}
