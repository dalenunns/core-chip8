using System;
using chip8.core;

namespace chip8.console
{
    public class ConsoleAudio : IAudio
    {
        public void Beep()
        {
            Console.Beep();
        }
    }
}