using System;
using chip8.core;

namespace chip8.console
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleAudio audio = new ConsoleAudio();
            ConsoleInput input = new ConsoleInput();
            ConsoleGraphics graphics = new ConsoleGraphics();

            graphics.ClearScreen();
            Chip8 chp8 = new Chip8(graphics,audio,input);

            // //LoadROM

            // while(true) {
            //     chp8.Tick();
            graphics.BlitScreen();
            // }
        }
    }
}
