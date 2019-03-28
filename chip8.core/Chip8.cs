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

        //Chip8 Memory - 4096bytes in size.
        private byte[] Memory {get;set;} = new byte[MEMORY_SIZE];
        //CPU Registers V0-VF 
        // (VF doubles as a flag for some instructions. Carry flag in addition)
        private byte[] V {get;set;} = new byte[16];
        //Index Register
        private ushort I {get; set;} =0;
        //Program counter
        private ushort PC {get;set;} = 0;
        private Stack<ushort> Stack {get;set;} = new Stack<ushort>(STACK_SIZE);
        private ushort StackPointer {get;set;}


        public override string ToString() {
            return "Hello World";
        }

    }
}
