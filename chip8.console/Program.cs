using System;
using chip8.core;
using System.IO;
using System.Collections.Generic;

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
            Chip8 chp8 = new Chip8(graphics, audio, input);

            byte[] rom = LoadROMFile(@"/home/chippy/src/dotnet/core-chip8/Fishie.ch8");
            // //LoadROM
            chp8.LoadROM(rom);

            Console.WriteLine("Starting Emulator");

            try
            {
                while (true)
                {
                    chp8.Tick();
                    graphics.BlitScreen();
              //      System.Diagnostics.Debug.WriteLine(chp8.ToString());
                }
            }
            catch (Exception e)
            {

            }
        }

        private static byte[] LoadROMFile(string filename)
        {
            List<byte> ROM = new List<byte>();

            using (BinaryReader binaryReader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                for (int i = 0; i < binaryReader.BaseStream.Length; i++)
                {
                    ROM.Add(binaryReader.ReadByte());
                }
            }

            return ROM.ToArray();
        }
    }
}
