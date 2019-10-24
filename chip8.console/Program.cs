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
            if (args.Length == 0)
            {
                Console.WriteLine("Error no ROM file specified");
                return;
            }
            else
            {
                if (!File.Exists(args[0]))
                {
                    Console.WriteLine($"Error ROM file ({args[0]}) not found.");
                }
                else
                {
                    ConsoleAudio audio = new ConsoleAudio();
                    ConsoleInput input = new ConsoleInput();
                    ConsoleGraphics graphics = new ConsoleGraphics();

                    graphics.ClearScreen();
                    Chip8 chp8 = new Chip8(graphics, audio, input);

                    byte[] rom = LoadROMFile(args[0]);
                    //LoadROM
                    chp8.LoadROM(rom);

                    Console.WriteLine("Starting Emulator");

                    try
                    {
                        while (true)
                        {
                            chp8.Tick();
                            graphics.BlitScreen();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                }
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
