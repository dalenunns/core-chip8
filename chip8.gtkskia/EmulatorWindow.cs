using System;
using System.Collections.Generic;
using Gtk;

namespace chip8.gtkskia
{
    public class EmulatorWindow : Window
    {
        private readonly CustomDrawing customDrawArea = new CustomDrawing();
        public EmulatorWindow(string title) : base(title)
        {
            SetDefaultSize(640, 320);
            SetPosition(WindowPosition.Center);
            Add(customDrawArea);
        }

        public void UpdateScreen(bool[,] screen)
        {
            Gtk.Application.Invoke(delegate
                {
                    customDrawArea.ScreenData = screen;
                });
        }
    }
}