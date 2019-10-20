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
            //Graphics Buffer Dump Window
            //Memory Hex Dump (with colour highlighting)
            //Debug Window (hexdump, disassembly, registers etc)

            Gtk.Application.Init();

            audio = new Audio();
            input = new Input();
            graphics = new Graphics();

            chp8 = new Chip8(graphics, audio, input);

            //byte[] rom = LoadROMFile(@"/home/chippy/src/dotnet/core-chip8/chip8.roms/IBMLogo.ch8");
            byte[] rom = LoadROMFile(@"/home/chippy/src/dotnet/core-chip8/chip8.roms/PONG");
            //LoadROM
            chp8.LoadROM(rom);


            var hb = new HeaderBar();
            hb.ShowCloseButton = true;
            hb.Title = "Core-Chip8 GTK#";

            var gfxDumpBtn = new Button();
            gfxDumpBtn.Label = "Gfx";
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


            var hexDumpBtn = new Button();
            hexDumpBtn.Label = "Hex";
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

            var debuggerBtn = new Button();
            debuggerBtn.Label = "Debug";
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

            var window = new Window("Core-Chip8");
            window.SetDefaultSize(640, 320);

            window.SetPosition(WindowPosition.Center);
            window.Titlebar = hb;
            window.DeleteEvent += delegate
            {
                KILL_EMULATION = true;
                Gtk.Application.Quit();
            };

            window.KeyPressEvent += new KeyPressEventHandler(KeyPress);

            darea = new CustomDrawing();
            window.Add(darea);
            window.ShowAll();

            Thread t = new Thread(new ThreadStart(BackgroundTick));
            t.Start();

            Gtk.Application.Run();
        }

        [GLib.ConnectBefore]
        private static void KeyPress(object o, KeyPressEventArgs args)
        {
            Console.WriteLine(args.Event.Key);
             switch(args.Event.Key) {
                 case Gdk.Key.KP_1:
                    input.SendKeyPress(0x01);    
                    break;
                 case Gdk.Key.KP_2:
                    input.SendKeyPress(0x02);    
                    break;
                 case Gdk.Key.KP_3:
                    input.SendKeyPress(0x03);    
                    break;
                 case Gdk.Key.KP_4:
                    input.SendKeyPress(0x04);    
                    break;
                 case Gdk.Key.KP_5:
                    input.SendKeyPress(0x05);    
                    break;
                 case Gdk.Key.KP_6:
                    input.SendKeyPress(0x06);    
                    break;
                 case Gdk.Key.KP_7:
                    input.SendKeyPress(0x07);    
                    break;
                 case Gdk.Key.KP_8:
                    input.SendKeyPress(0x08);    
                    break;
                 case Gdk.Key.KP_9:
                    input.SendKeyPress(0x09);    
                    break;
             }


            if (args.Event.Key == Gdk.Key.Up)
            {
                input.SendKeyPress(0x01);
            }

            if (args.Event.Key == Gdk.Key.Down)
            {
                input.SendKeyPress(0x04);
            }

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

                Gtk.Application.Invoke(delegate
                {
                    darea.ScreenData = graphics.Screen;
                    if (graphics.ScreenChanged)
                    {
                        if (graphicsDumpWindow != null && graphicsDumpWindow.Visible)
                            graphicsDumpWindow.SetGraphics(graphics.Screen);

                        if (memoryDumpWindow != null && memoryDumpWindow.Visible)
                            memoryDumpWindow.SetMemory(chp8.Memory);

                        graphics.ScreenChanged = false;
                    }

                    if (debuggerWindow != null && debuggerWindow.Visible)
                        debuggerWindow.SetWatchValues(chp8.PC, chp8.I, chp8.V, chp8.DelayTimer, chp8.SoundTimer);
                });

                if (DebugState == DebugOptions.Step) { DebugState = DebugOptions.Pause; }

                Thread.Sleep(5);
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
