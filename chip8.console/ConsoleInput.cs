using System;
using chip8.core;

namespace chip8.console
{
    public class ConsoleInput : IInput
    {
        public bool KeyPressed(byte key)
        {
            throw new NotImplementedException();
        }

        public bool WaitForKey(out byte keyPressed)
        {
            throw new NotImplementedException();
        }
    }
}