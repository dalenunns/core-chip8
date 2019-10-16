using System;
using chip8.core;

namespace chip8.gtkskia.Services
{
    public class Audio : IAudio
    {
        public void Beep()
        {
            Console.Beep();
        }
    }
}