using System;
using chip8.core;
using System.Collections.Generic;

namespace chip8.console
{
    public class ConsoleInput : IInput
    {
        private List<ConsoleKey> _validKeyPresses = new List<ConsoleKey>(); //A collection holding the valid console keys for the console (0-F)
        public ConsoleInput()
        {
            //Setup the default allowed keys
            _validKeyPresses.Add(ConsoleKey.D0);
            _validKeyPresses.Add(ConsoleKey.D1);
            _validKeyPresses.Add(ConsoleKey.D2);
            _validKeyPresses.Add(ConsoleKey.D3);
            _validKeyPresses.Add(ConsoleKey.D4);
            _validKeyPresses.Add(ConsoleKey.D5);
            _validKeyPresses.Add(ConsoleKey.D6);
            _validKeyPresses.Add(ConsoleKey.D7);
            _validKeyPresses.Add(ConsoleKey.D8);
            _validKeyPresses.Add(ConsoleKey.D9);
            _validKeyPresses.Add(ConsoleKey.A);
            _validKeyPresses.Add(ConsoleKey.B);
            _validKeyPresses.Add(ConsoleKey.C);
            _validKeyPresses.Add(ConsoleKey.D);
            _validKeyPresses.Add(ConsoleKey.E);
            _validKeyPresses.Add(ConsoleKey.F);
        }

        private byte GetByteValueOfConsoleKey(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.D0:
                    {
                        return 0x0;
                    }
                case ConsoleKey.D1:
                    {
                        return 0x1;
                    }
                case ConsoleKey.D2:
                    {
                        return 0x2;
                    }
                case ConsoleKey.D3:
                    {
                        return 0x3;
                    }
                case ConsoleKey.D4:
                    {
                        return 0x4;
                    }
                case ConsoleKey.D5:
                    {
                        return 0x5;
                    }
                case ConsoleKey.D6:
                    {
                        return 0x6;
                    }
                case ConsoleKey.D7:
                    {
                        return 0x7;
                    }
                case ConsoleKey.D8:
                    {
                        return 0x8;
                    }
                case ConsoleKey.D9:
                    {
                        return 0x9;
                    }
                case ConsoleKey.A:
                    {
                        return 0xA;
                    }
                case ConsoleKey.B:
                    {
                        return 0xB;
                    }
                case ConsoleKey.C:
                    {
                        return 0xC;
                    }
                case ConsoleKey.D:
                    {
                        return 0xD;
                    }
                case ConsoleKey.E:
                    {
                        return 0xE;
                    }
                case ConsoleKey.F:
                    {
                        return 0xF;
                    }
            }
            return 0;
        }
        public bool KeyPressed(byte key)
        {
            if (Console.KeyAvailable)
            {
                var keyPressed = Console.ReadKey(true);
                if (_validKeyPresses.Contains(keyPressed.Key))
                {
                    if (GetByteValueOfConsoleKey(keyPressed.Key) == key)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool WaitForKey(out byte key)
        {
            if (Console.KeyAvailable)
            {
                var keyPressed = Console.ReadKey(true);
                if (_validKeyPresses.Contains(keyPressed.Key))
                {
                    key = GetByteValueOfConsoleKey(keyPressed.Key);
                    return true;

                }
            }

            key = 255;
            return false;
        }
    }
}