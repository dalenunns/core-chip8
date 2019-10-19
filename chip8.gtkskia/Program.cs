using System;
using Gtk;
using System.IO;
using chip8.gtkskia.Services;
using chip8.core;
using System.Collections.Generic;
using System.Threading;

namespace chip8.gtkskia
{
    class Program
    {
        static bool KILL_EMULATION = false;

        static Audio audio;
        static Input input;
        static Graphics graphics;
        static Chip8 chp8;

        static CustomDrawing darea;

        static Windows.GraphicsDumpWindow graphicsDumpWindow;
        static Windows.MemoryDumpWindow memoryDumpWindow;
        static Windows.DebuggerWindow debuggerWindow;

        static void Main(string[] args)
        {
            //Graphics Buffer Dump Window
            //Memory Hex Dump (with colour highlighting)
            //Debug Window (hexdump, disassembly, registers etc)

            Gtk.Application.Init();

            audio = new Audio();
            input = new Input();
            graphics = new Graphics();

            chp8 = new Chip8(graphics, audio, input);

            byte[] rom = LoadROMFile(@"/home/chippy/src/dotnet/core-chip8/chip8.roms/IBMLogo.ch8");
            //LoadROM
            chp8.LoadROM(rom);
            
            graphicsDumpWindow = new Windows.GraphicsDumpWindow("Graphics Buffer Dump");
            graphicsDumpWindow.SetGraphics(graphics.Screen);
            graphicsDumpWindow.ShowAll();

            memoryDumpWindow = new Windows.MemoryDumpWindow("Memory Dump Window");
            memoryDumpWindow.SetMemory(chp8.Memory);
            memoryDumpWindow.ShowAll();            

            debuggerWindow = new Windows.DebuggerWindow("Debugger Window");
            debuggerWindow.SetMemory(chp8.Memory);
            debuggerWindow.ShowAll();            

            var window = new Window("Core-Chip8");
            window.SetDefaultSize(640, 320);
            window.SetPosition(WindowPosition.Center);
            window.DeleteEvent += delegate
            {
                KILL_EMULATION = true;
                Gtk.Application.Quit();
            };

            darea = new CustomDrawing();
            window.Add(darea);
            window.ShowAll();            

            Thread t = new Thread(new ThreadStart(BackgroundTick));
            t.Start();

            Gtk.Application.Run();
        }

        static void BackgroundTick()
        {
            while (!KILL_EMULATION)
            {
                chp8.Tick();
                Gtk.Application.Invoke(delegate
                {
                    darea.ScreenData = graphics.Screen;
                    if (graphics.ScreenChanged)
                    {
                        graphicsDumpWindow.SetGraphics(graphics.Screen);
                        memoryDumpWindow.SetMemory(chp8.Memory);
                        graphics.ScreenChanged = false;
                        debuggerWindow.SetWatchValues(chp8.PC,chp8.I,chp8.V,chp8.DelayTimer,chp8.SoundTimer);
                    }

                });

                //graphics.BlitScreen();

                Thread.Sleep(100);
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
