using System;
using System.Collections.Generic;
using Gtk;

namespace chip8.gtkskia.Windows
{
    public class GraphicsDumpWindow : Window
    {
        private readonly TextView graphicsTextView;
        private readonly TextBuffer graphicsDumpBuffer = new TextBuffer(new TextTagTable());

        public GraphicsDumpWindow(string title) : base(title)
        {
            this.SetSizeRequest(640,480);

            graphicsTextView = new TextView(graphicsDumpBuffer)
            {
                Editable = false,
                Monospace = true
            };

            CreateTags(graphicsDumpBuffer);
            Add(graphicsTextView);                       
        }      

        private void CreateTags(TextBuffer buffer)
        {
            Gdk.RGBA black = new Gdk.RGBA();
            black.Parse("#000000");
            Gdk.RGBA white = new Gdk.RGBA();
            white.Parse("#ffffff");

            TextTag tag = new TextTag("off")
            {
                Weight = Pango.Weight.Normal,
                BackgroundRgba = black,
                ForegroundRgba = white
            };
            buffer.TagTable.Add(tag);

            tag = new TextTag("on")
            {
                Weight = Pango.Weight.Normal,
                BackgroundRgba = white,
                ForegroundRgba = black
            };
            buffer.TagTable.Add(tag);
        }

        public void SetGraphics(bool[,] screen)
      {
            graphicsDumpBuffer.Clear();
            TextIter position = graphicsDumpBuffer.EndIter;

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (screen[x, y])
                    {
                        graphicsDumpBuffer.InsertWithTagsByName(ref position, "1 ", "on");
                    }
                    else
                    {
                        graphicsDumpBuffer.InsertWithTagsByName(ref position, "0 ", "off");
                    }
                }
                graphicsDumpBuffer.Insert(ref position, "\n");
            }
        }
    }
}