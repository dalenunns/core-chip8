using System;
using chip8.core;

namespace chip8.console
{
    public class ConsoleGraphics : IGraphics
    {
        private bool[,] screen = new bool[64, 32];
        private bool updateScreen = false;

        public void ClearScreen()
        {
            for (int x = 0; x < 64; x++)
                for (int y = 0; y < 32; y++)
                    screen[x, y] = false;

            Console.Clear();
        }

        public bool DrawSprite(byte x, byte y, byte spriteHeight, byte[] sprite)
        {
            bool pixelChanged = false;

            updateScreen = true;
            for (int height = 0; height < spriteHeight; height++)
            {
                byte spriteLine = sprite[height];
                        int posY = y + height;
                for (int width = 0; width < 8; width++)
                {
                    if ((spriteLine & (0x80 >> width)) != 0)
                    {
                        int posX = x + width;

                        if (screen[posX, posY]) { pixelChanged = true; }
                        screen[posX, posY] ^= true;
                    }
                }
            }

            return pixelChanged;
        }

        public void BlitScreen()
        {
            if (updateScreen)
            {
                for (int x = 0; x < 64; x++)
                    for (int y = 0; y < 32; y++)
                    {
                        Console.SetCursorPosition(x, y);
                        if (screen[x, y])
                        {
                            Console.Write("X");
                        }
                        else
                        {
                            Console.Write(" ");
                        }
                    }

                updateScreen = false;
            }
        }
    }

}