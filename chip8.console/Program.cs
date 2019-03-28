using System;
using chip8.core;

namespace chip8.console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Chip8 chp8 = new Chip8();
            Console.WriteLine(chp8.ToString());
        }
    }
}
