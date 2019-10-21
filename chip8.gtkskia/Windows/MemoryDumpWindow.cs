using System;
using System.Collections.Generic;
using Gtk;

namespace chip8.gtkskia.Windows
{
    public class MemoryDumpWindow : Window
    {
        private readonly TextView memoryTextView;
        private readonly TextBuffer memoryDumpBuffer = new TextBuffer(new TextTagTable());

        public MemoryDumpWindow(string title) : base(title)
        {
            this.SetSizeRequest(480,280);
            var scrollWindow = new ScrolledWindow
            {
                BorderWidth = 5,
                ShadowType = ShadowType.In
            };
            Add(scrollWindow);

            memoryTextView = new TextView(memoryDumpBuffer)
            {
                LeftMargin = 5,
                Editable = false,
                Monospace = true
            };                 
            CreateTags(memoryDumpBuffer);
            scrollWindow.Add(memoryTextView);      
        }      

        private void CreateTags(TextBuffer buffer)
        {
            Gdk.RGBA black = new Gdk.RGBA();
            black.Parse("#000000");
            Gdk.RGBA white = new Gdk.RGBA();
            white.Parse("#ffffff");

            TextTag tag = new TextTag("WhiteOnBlack")
            {
                Weight = Pango.Weight.Normal,
                BackgroundRgba = black,
                ForegroundRgba = white
            };
            buffer.TagTable.Add(tag);

            tag = new TextTag("BlackOnWhite")
            {
                Weight = Pango.Weight.Normal,
                BackgroundRgba = white,
                ForegroundRgba = black
            };
            buffer.TagTable.Add(tag);
        }

        public void SetMemory(byte[] memory)
        {
            //TODO: Would be cool to diff the current memory and show changes in a different colour? 
            //TODO: Highlight the different regions of the memory. Fonts, Program etc
            memoryDumpBuffer.Clear();
            TextIter position = memoryDumpBuffer.EndIter;

            string str = BitConverter.ToString(memory).Replace('-', ' ')+' ';
            int chunkLength = (16 * 2) + 16;
            int memoryAddress = 0;

            for (int i = 0; i < str.Length; i += chunkLength)
            {
                if (chunkLength + i > str.Length)
                    chunkLength = str.Length - i;

                var m = str.Substring(i, chunkLength) + "\n";              
                
                memoryDumpBuffer.InsertWithTagsByName(ref position, string.Format($"0x{(memoryAddress):x4} "),"BlackOnWhite");
                memoryDumpBuffer.InsertWithTagsByName(ref position, m, "WhiteOnBlack");
                memoryAddress += 16;
            }                                    
        }
    }
}