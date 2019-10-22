using System;
using chip8.core;

namespace chip8.gtkskia.Services
{
    public class Input : IInput
    {
        private bool keyPressed = false; //indicate if a key has been pressed
        private byte keyValue = 0x00; //which key was pressed.

        [GLib.ConnectBefore]
        public void KeyPressEventHandler(object o, Gtk.KeyPressEventArgs args)
        {
            switch (args.Event.Key)
            {
                case Gdk.Key.Up:
                case Gdk.Key.KP_1:
                    SendKeyPress(0x01);
                    break;
                case Gdk.Key.KP_2:
                    SendKeyPress(0x02);
                    break;
                case Gdk.Key.KP_3:
                    SendKeyPress(0x03);
                    break;
                case Gdk.Key.Down:
                case Gdk.Key.KP_4:
                    SendKeyPress(0x04);
                    break;
                case Gdk.Key.KP_5:
                    SendKeyPress(0x05);
                    break;
                case Gdk.Key.KP_6:
                    SendKeyPress(0x06);
                    break;
                case Gdk.Key.KP_7:
                    SendKeyPress(0x07);
                    break;
                case Gdk.Key.KP_8:
                    SendKeyPress(0x08);
                    break;
                case Gdk.Key.KP_9:
                    SendKeyPress(0x09);
                    break;
                default:
                    //Console.WriteLine(args.Event.Key);
                    break;
            }
        }

        public bool KeyPressed(byte key)
        {
            if (keyPressed)
            {
                if (key == keyValue) {
                    keyPressed = false;
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

        private void SendKeyPress(byte key) {
            keyPressed = true;
            keyValue = key;
        }
    }
}