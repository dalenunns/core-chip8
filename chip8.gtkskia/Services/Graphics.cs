using System;
using System.Text;
using chip8.core;

namespace chip8.gtkskia.Services
{
    public class Graphics : IGraphics
    {
        private bool[,] screen = new bool[64, 32];
        private bool updateScreen = false;

        public bool[,] Screen  {
            get => screen;
        }

        public bool ScreenChanged {get => updateScreen; set => updateScreen = value;}

        public void ClearScreen()
        {
            for (int x = 0; x < 64; x++)
                for (int y = 0; y < 32; y++)
                    screen[x, y] = false;

            updateScreen = true;
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
                Console.SetCursorPosition(0, 0);
                StringBuilder sb = new StringBuilder();
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        if (screen[x, y])
                        {
                            sb.Append("█");
                        }
                        else
                        {
                            sb.Append(" ");
                        }
                    }
                    sb.Append("\n");
                }
                Console.Write(sb.ToString());

                updateScreen = false;
            }
        }
    }

}