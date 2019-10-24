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

        //static CustomDrawing darea;

        static Windows.GraphicsDumpWindow graphicsDumpWindow;
        static Windows.MemoryDumpWindow memoryDumpWindow;
        static Windows.DebuggerWindow debuggerWindow;
        static EmulatorWindow emulatorWindow;

        public enum DebugOptions
        {
            Pause,
            Step,
            Play,
            Breakpoint
        }

        public static DebugOptions DebugState = DebugOptions.Play;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error no ROM file specified");
                return;
            }
            else
            {
                bool debugMode = false;
                int filePathIndex = 0;

                if (args[0].ToUpper() == "-DEBUG")
                {
                    debugMode = true;
                    filePathIndex = 1;
                }

                if (!File.Exists(args[filePathIndex]))
                {
                    Console.WriteLine($"Error ROM file ({args[filePathIndex]}) not found.");
                }
                else
                {
                    audio = new Audio();
                    input = new Input();
                    graphics = new Graphics();

                    chp8 = new Chip8(graphics, audio, input);

                    //byte[] rom = LoadROMFile(@"/home/chippy/src/dotnet/core-chip8/chip8.roms/PONG");
                    byte[] rom = LoadROMFile(args[filePathIndex]);
                    //LoadROM
                    chp8.LoadROM(rom);

                    try
                    {
                        Gtk.Application.Init();
                        emulatorWindow = new EmulatorWindow("Core Chip8");
                        emulatorWindow.DeleteEvent += delegate
                        {
                            KILL_EMULATION = true;
                            Gtk.Application.Quit();
                        };

                        if (debugMode)
                        {
                            LoadDebugEnvironment();
                        }

                        emulatorWindow.KeyPressEvent += new KeyPressEventHandler(input.KeyPressEventHandler);
                        emulatorWindow.ShowAll();

                        Thread t = new Thread(new ThreadStart(BackgroundTick));
                        t.Start();

                        Gtk.Application.Run();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                }
            }
        }

        private static void LoadDebugEnvironment()
        {
            var hb = new HeaderBar
            {
                ShowCloseButton = true,
                Title = "Core-Chip8 - DEBUG MODE"
            };

            emulatorWindow.KeyPressEvent += new KeyPressEventHandler(DebugKeyPressEventHandler);

            var gfxDumpBtn = new Button { Label = "Gfx" };

            gfxDumpBtn.Clicked += delegate (object obj, EventArgs ars)
            {
                if (graphicsDumpWindow == null || !graphicsDumpWindow.Visible)
                {
                    graphicsDumpWindow = new Windows.GraphicsDumpWindow("Graphics Buffer Dump");
                    graphicsDumpWindow.SetGraphics(graphics.Screen);
                    graphicsDumpWindow.ShowAll();
                }
            };
            hb.PackEnd(gfxDumpBtn);

            var hexDumpBtn = new Button { Label = "Hex" };

            hexDumpBtn.Clicked += delegate (object obj, EventArgs ars)
            {
                if (memoryDumpWindow == null || !memoryDumpWindow.Visible)
                {
                    memoryDumpWindow = new Windows.MemoryDumpWindow("Memory Dump Window");
                    memoryDumpWindow.SetMemory(chp8.Memory);
                    memoryDumpWindow.ShowAll();
                }
            };
            hb.PackEnd(hexDumpBtn);

            var debuggerBtn = new Button { Label = "Debug" };

            debuggerBtn.Clicked += delegate (object obj, EventArgs ars)
            {
                if (debuggerWindow == null || !debuggerWindow.Visible)
                {
                    debuggerWindow = new Windows.DebuggerWindow("Debugger Window");
                    debuggerWindow.SetMemory(chp8.Memory);
                    debuggerWindow.ShowAll();
                }
            };
            hb.PackEnd(debuggerBtn);

            emulatorWindow.Titlebar = hb;
        }

        private static void DebugKeyPressEventHandler(object o, Gtk.KeyPressEventArgs args)
        {
            switch (args.Event.Key)
            {
                case Gdk.Key.F10: //Step
                    DebugControl(DebugOptions.Step);
                    break;
                case Gdk.Key.F5: //Play
                    DebugControl(DebugOptions.Play);
                    break;

            }
        }

        public static void Reset()
        {
            graphics.ClearScreen();
            chp8.Init();
        }
        public static void DebugControl(DebugOptions option)
        {
            DebugState = option;
        }

        static void BackgroundTick()
        {
            while (!KILL_EMULATION)
            {
                if ((DebugState == DebugOptions.Play) || DebugState == DebugOptions.Step)
                {
                    chp8.Tick();
                }

                emulatorWindow.UpdateScreen(graphics.Screen);


                Gtk.Application.Invoke(delegate
                {
                    if (graphics.ScreenChanged)
                    {
                        if (graphicsDumpWindow != null && graphicsDumpWindow.Visible)
                            graphicsDumpWindow.SetGraphics(graphics.Screen);

                        graphics.ScreenChanged = false;
                    }

                    if (debuggerWindow != null && debuggerWindow.Visible)
                    {
                        debuggerWindow.SetWatchValues(chp8.PC, chp8.I, chp8.V, chp8.DelayTimer, chp8.SoundTimer);
                    }
                });

                if (DebugState == DebugOptions.Step) { DebugState = DebugOptions.Pause; }

                Thread.Sleep(4);
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
