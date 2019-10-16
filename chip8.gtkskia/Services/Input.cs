using System;
using chip8.core;

namespace chip8.gtkskia.Services
{
    public class Input : IInput
    {
        public bool KeyPressed(byte key)
        {
            //throw new NotImplementedException();
            return false;
        }

        public bool WaitForKey(out byte keyPressed)
        {
            //throw new NotImplementedException();
            keyPressed = 0;
            return false;
        }
    }
}