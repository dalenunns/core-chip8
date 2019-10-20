using System;
using chip8.core;

namespace chip8.gtkskia.Services
{
    public class Input : IInput
    {
        private bool keyPressed = false; //indicate if a key has been pressed
        private byte keyValue = 0x00; //which key was pressed.

        public bool KeyPressed(byte key)
        {
            if (keyPressed)
            {
                keyPressed = false;
                if (key == keyValue) {
                    return true;
                }
            }

            return false;
        }

        public bool WaitForKey(out byte key)
        {            
            if (keyPressed) {
                keyPressed = false;
                key = keyValue;
                return true;
            }

            key = 0;
            return false;
        }

        public void SendKeyPress(byte key) {
            keyPressed = true;
            keyValue = key;
        }

        public void ClearKeyPress() {
            keyPressed = false;
            keyValue = 0x00;
        }
    }
}