using System;
using System.Collections.Generic;
using Gtk;

namespace chip8.gtkskia.Windows
{
    public class DebuggerWindow : Window
    {
        private byte[] Memory;

        private TextView disassemblyTextView;
        private TextBuffer disassemblyBuffer = new TextBuffer(new TextTagTable());

        private TextView watchTextView;
        private TextBuffer watchBuffer = new TextBuffer(new TextTagTable());

        
        public DebuggerWindow(string title) : base(title)
        {
            Gdk.RGBA black = new Gdk.RGBA();
            black.Parse("#000000");

            this.SetSizeRequest(640, 480);

            var hb = new HeaderBar();
            var hPaned = new HPaned();           
           
            #region Disassembly Text View Setup

            disassemblyTextView = new TextView(disassemblyBuffer);
            disassemblyTextView.Editable = false;
            disassemblyTextView.Monospace = true;
            var scrollWindow = new ScrolledWindow();
            scrollWindow.BorderWidth = 5;
            scrollWindow.ShadowType = ShadowType.In;
            scrollWindow.Add(disassemblyTextView);

            CreateTags(disassemblyBuffer);           
            
            //hPaned.Add1(scrollWindow);
            hPaned.Pack1(scrollWindow,true,false);
            
            #endregion

            var box = new Box(Orientation.Vertical,6);
            
            var buttonBox = new Box(Orientation.Horizontal, 6);
            var pauseButton = new Button();
            pauseButton.Label = "pause";
            var stepButton = new Button();
            stepButton.Label = "step";
            var playButton = new Button();
            playButton.Label = "play";

            buttonBox.PackStart(pauseButton,false,true,2);
            buttonBox.PackStart(stepButton,false,true,2);
            buttonBox.PackStart(playButton,false,true,2);

            box.PackStart(buttonBox,false,false,2);

            var watchLabel = new Label("Watch");

            box.PackStart(watchLabel, false,false,2);

            watchTextView = new TextView(watchBuffer);
            watchTextView.Editable = false;
            watchTextView.Monospace = true;

            CreateTags(watchBuffer);

            box.PackStart(watchTextView,true,true,2);

            hPaned.Pack2(box,false,false);

            Add(hPaned);
        }

        private void CreateTags(TextBuffer buffer)
        {
            Gdk.RGBA white = new Gdk.RGBA();
            white.Parse("#ffffff");

            Gdk.RGBA commentColour = new Gdk.RGBA();
            commentColour.Parse("#5b814c");

            Gdk.RGBA opCodeColour = new Gdk.RGBA();
            opCodeColour.Parse("#81331e");

            Gdk.RGBA keywordColour = new Gdk.RGBA();
            keywordColour.Parse("#F92672");
            Gdk.RGBA variableColour = new Gdk.RGBA();
            variableColour.Parse("#A6E22E");
            Gdk.RGBA constantColour = new Gdk.RGBA();
            constantColour.Parse("#AE81FF");
            Gdk.RGBA addressFgColour = new Gdk.RGBA();
            addressFgColour.Parse("#8F908A");
            Gdk.RGBA addressBgColour = new Gdk.RGBA();
            addressBgColour.Parse("#2F3129");
            Gdk.RGBA backgroundColour = new Gdk.RGBA();
            backgroundColour.Parse("#272822");




            TextTag tag = new TextTag("keyword");
            tag.Weight = Pango.Weight.Normal;
            tag.ForegroundRgba = keywordColour;
            tag.BackgroundRgba = backgroundColour;
            buffer.TagTable.Add(tag);

            tag = new TextTag("variable");
            tag.Weight = Pango.Weight.Normal;
            tag.ForegroundRgba = variableColour;
            tag.BackgroundRgba = backgroundColour;
            buffer.TagTable.Add(tag);

            tag = new TextTag("constant");
            tag.Weight = Pango.Weight.Normal;
            tag.ForegroundRgba = constantColour;
            tag.BackgroundRgba = backgroundColour;
            buffer.TagTable.Add(tag);

            tag = new TextTag("address");
            tag.Weight = Pango.Weight.Bold;
            tag.ForegroundRgba = addressFgColour;
            tag.BackgroundRgba = addressBgColour;
            buffer.TagTable.Add(tag);

            tag = new TextTag("opCode");
            tag.Weight = Pango.Weight.Normal;
            tag.ForegroundRgba = opCodeColour;
            tag.BackgroundRgba = addressBgColour;
            buffer.TagTable.Add(tag);

            tag = new TextTag("white");
            tag.Weight = Pango.Weight.Normal;
            tag.ForegroundRgba = white;
            tag.BackgroundRgba = backgroundColour;
            buffer.TagTable.Add(tag);


            tag = new TextTag("comment");
            tag.Weight = Pango.Weight.Normal;
            tag.Style = Pango.Style.Italic;
            tag.ForegroundRgba = commentColour;
            tag.BackgroundRgba = backgroundColour;
            buffer.TagTable.Add(tag);
        }

        public void SetWatchValues(ushort PC, ushort I, byte[] V, byte DelayTimer, byte SoundTimer) {
            int LINE_LENGTH = 30;

            watchBuffer.Clear();
            TextIter position = watchBuffer.EndIter;

            watchBuffer.InsertWithTagsByName(ref position, $" PC", "variable");
            watchBuffer.InsertWithTagsByName(ref position, $" 0x{PC:x4}", "constant");
            watchBuffer.InsertWithTagsByName(ref position, $" //Program Counter", "comment");                
            watchBuffer.InsertWithTagsByName(ref position, "".PadLeft(LINE_LENGTH - position.BytesInLine, ' ') + "\n", "white");

            watchBuffer.InsertWithTagsByName(ref position, $" I", "variable");
            watchBuffer.InsertWithTagsByName(ref position, $" 0x{I:x4}", "constant");
            watchBuffer.InsertWithTagsByName(ref position, $" //b{Convert.ToString(I,2).PadLeft(8,'0')}", "comment");
            watchBuffer.InsertWithTagsByName(ref position, "".PadLeft(LINE_LENGTH - position.BytesInLine, ' ') + "\n", "white");
            watchBuffer.InsertWithTagsByName(ref position, "".PadLeft(LINE_LENGTH - position.BytesInLine, ' ') + "\n", "white");

            for (int i = 0; i < V.Length; i++)
            {
                watchBuffer.InsertWithTagsByName(ref position, $"  V{i:x1}", "variable");
                watchBuffer.InsertWithTagsByName(ref position, $" 0x{V[i]:x2}", "constant");                
                watchBuffer.InsertWithTagsByName(ref position, $" //b{Convert.ToString(V[i],2).PadLeft(8,'0')}", "comment");
                watchBuffer.InsertWithTagsByName(ref position, "".PadLeft(LINE_LENGTH - position.BytesInLine, ' ') + "\n", "white");
            }

            watchBuffer.InsertWithTagsByName(ref position, "".PadLeft(LINE_LENGTH - position.BytesInLine, ' ') + "\n", "white");
            watchBuffer.InsertWithTagsByName(ref position, $" DelayTimer", "variable");
            watchBuffer.InsertWithTagsByName(ref position, $" {DelayTimer}", "constant");
            watchBuffer.InsertWithTagsByName(ref position, $" //0x{DelayTimer:x2}", "comment");                
            watchBuffer.InsertWithTagsByName(ref position, "".PadLeft(LINE_LENGTH - position.BytesInLine, ' ') + "\n", "white");
            watchBuffer.InsertWithTagsByName(ref position, $" SoundTimer", "variable");
            watchBuffer.InsertWithTagsByName(ref position, $" {SoundTimer}", "constant");
            watchBuffer.InsertWithTagsByName(ref position, $" //0x{SoundTimer:x2}", "comment");                
            watchBuffer.InsertWithTagsByName(ref position, "".PadLeft(LINE_LENGTH - position.BytesInLine, ' ') + "\n", "white");
        }

        public void SetMemory(byte[] memory)
        {
            this.Memory = memory;

            disassemblyBuffer.Clear();

            ushort PC = chip8.core.Chip8.START_PROGRAM_MEMORY;
            TextIter position = disassemblyBuffer.EndIter;

            while (PC < chip8.core.Chip8.MEMORY_SIZE)
            {
                var opCode = (ushort)(Memory[PC++] << 8 | Memory[PC++]);
                var op = new chip8.core.Chip8.OpCodeData()
                {
                    OpCode = opCode,
                    Instruction = (ushort)((opCode & 0xF000) >> 12),
                    NNN = (ushort)(opCode & 0x0FFF),
                    NN = (byte)(opCode & 0x00FF),
                    N = (byte)(opCode & 0x00F),
                    X = (byte)((opCode & 0x0F00) >> 8),
                    Y = (byte)((opCode & 0x00F0) >> 4)
                };

                DissasembleOpCode(disassemblyBuffer, ref position, op, PC);                
            }
        }


        private void DissasembleOpCode(TextBuffer buffer, ref TextIter position, chip8.core.Chip8.OpCodeData op, ushort PC)
        {
            buffer.InsertWithTagsByName(ref position, string.Format($"0x{(PC):x4} "), "address");
            buffer.InsertWithTagsByName(ref position, string.Format($"0x{(op.OpCode):x4} "), "opCode");
            buffer.InsertWithTagsByName(ref position, string.Format($" "), "keyword");

            //Decode the op codes
            switch (op.Instruction)
            {
                //Clear Screen / Return from subroutine
                case 0x0:
                    {
                        if (op.X == 0x0)
                        {
                            if (op.N == 0x0 && op.Y == 0xE)
                            {
                                //Clear screen
                                buffer.InsertWithTagsByName(ref position, "cls", "keyword");
                                break;
                            }
                            else if (op.N == 0xE)
                            {
                                //Return from sub
                                //PC = Stack.Pop();
                                buffer.InsertWithTagsByName(ref position, "return", "keyword");
                                break;
                            }
                            else if (op.NNN == 0x000)
                            {
                                break;
                            }
                        }
                        break;
                    }

                //0x1NNNN - Jumps to address NNN.
                case 0x1:
                    {
                        buffer.InsertWithTagsByName(ref position, "jump", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" 0x{((ushort)(op.NNN)):x3}", "constant");
                        //return $"jump {((ushort)(op.NNN)):x4}";
                        break;
                    }

                //0x2NNN - Calls subroutine at NNN.
                case 0x2:
                    {
                        //Stack.Push((ushort)(PC - 2)); //You need to subtract 2 from the PC as its incremented above to the next position already when its reached here.
                        //PC = (ushort)(op.NNN);
                        buffer.InsertWithTagsByName(ref position, "call", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" 0x{((ushort)(op.NNN)):x3}", "constant");
                        //return $"call {((ushort)(op.NNN)):x4}";
                        break;
                    }

                //0x3XNN - Skips the next instruction if VX equals NN. (Usually the next instruction is a jump to skip a code block)
                case 0x3:
                    {
                        //if (V[op.X] == op.NN) { PC += 2; }
                        buffer.InsertWithTagsByName(ref position, "sei", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                        buffer.InsertWithTagsByName(ref position, $" 0x{op.NN:x2}", "constant");
                        buffer.InsertWithTagsByName(ref position, $" //if (V{op.X:x} == 0x{op.NN:x2}) {{ PC += 2; }}", "comment");
                        break;
                        //return $"sei V{op.X} {op.NN:x2}";
                    }

                //0x4XNN - Skips the next instruction if VX doesn't equal NN. (Usually the next instruction is a jump to skip a code block)
                case 0x4:
                    {
                        buffer.InsertWithTagsByName(ref position, "snei", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                        buffer.InsertWithTagsByName(ref position, $" 0x{op.NN:x2}", "constant");
                        buffer.InsertWithTagsByName(ref position, $" //if (V{op.X:x} != 0x{op.NN:x2}) {{ PC += 2; }}", "comment");
                        break;
                        //if (V[op.X] != op.NN) { PC += 2; }
                        //return $"snei V{op.X} {op.NN:x2}";
                    }

                //0x5XY0 - 	Skips the next instruction if VX equals VY. (Usually the next instruction is a jump to skip a code block)
                case 0x5:
                    {
                        buffer.InsertWithTagsByName(ref position, "ser", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                        buffer.InsertWithTagsByName(ref position, $" V{op.Y:x}", "variable");
                        buffer.InsertWithTagsByName(ref position, $" //if (V{op.X:x} == V{op.Y:x}) {{ PC += 2; }}", "comment");
                        break;
                        //if (V[op.X] == V[op.Y]) { PC += 2; }
                        //return $"ser V{op.X} V{op.Y}";
                    }

                //0x6XNN - 	Sets VX to NN.
                case 0x6:
                    {
                        buffer.InsertWithTagsByName(ref position, "movi", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                        buffer.InsertWithTagsByName(ref position, $" 0x{op.NN:x2}", "constant");
                        buffer.InsertWithTagsByName(ref position, $" //V{op.X:x} = 0x{op.NN:x2}", "comment");
                        break;
                        //V[op.X] = op.NN;
                        //return $"movi {op.X} {op.NN:x2}";
                    }

                //0x7XNN - Adds NN to VX. (Carry flag is not changed)
                case 0x7:
                    {
                        buffer.InsertWithTagsByName(ref position, "addi", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                        buffer.InsertWithTagsByName(ref position, $" 0x{op.NN:x2}", "constant");
                        buffer.InsertWithTagsByName(ref position, $" //V{op.X:x} += 0x{op.NN:x2}", "comment");
                        break;

                        //V[op.X] += op.NN;
                        //return $"addi V{op.X} {op.NN:x2}";
                    }

                //0x8XY? - BitWise / Math operations based on ?
                case 0x8:
                    {
                        //Switch based on the last nibble of the opcode
                        switch (op.N)
                        {
                            case 0x0: // 8XY0 - Sets VX to the value of VY.
                                {
                                    buffer.InsertWithTagsByName(ref position, "movr", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.Y:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" //V{op.X:x} = V{op.Y:x}", "comment");
                                    break;

                                    //V[op.X] = V[op.Y];
                                    //return $"movr V{op.X} V{op.Y}";
                                }
                            case 0x1: // 8XY1 - Sets VX to VX or VY. (Bitwise OR operation)
                                {
                                    buffer.InsertWithTagsByName(ref position, "or", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.Y:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" //V{op.X:x} |= V{op.Y:x} (bitwise OR)", "comment");
                                    break;

                                    //V[op.X] |= V[op.Y];
                                    //return $"or V{op.X} V{op.Y} <i>//Set V{op.X} to V{op.X} OR V{op.Y} (Bitwise OR)</i>";
                                }
                            case 0x2: // 8XY2 - Sets VX to VX and VY. (Bitwise AND operation)
                                {
                                    buffer.InsertWithTagsByName(ref position, "and", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.Y:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" //V{op.X:x} &= V{op.Y:x} (bitwise AND)", "comment");
                                    break;

                                    //V[op.X] &= V[op.Y];
                                    //return $"and V{op.X} V{op.Y} <i>//Set V{op.X} to V{op.X} AND V{op.Y} (Bitwise AND)</i>";
                                }
                            case 0x3: // 8XY3 - Sets VX to VX xor VY.
                                {
                                    buffer.InsertWithTagsByName(ref position, "xor", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.Y:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" //V{op.X:x} ^= V{op.Y:x} (bitwise XOR)", "comment");
                                    break;
                                    //V[op.X] ^= V[op.Y];
                                    //return $"xor V{op.X} V{op.Y} <i>//Set V{op.X} to V{op.X} XOR V{op.Y} (Bitwise XOR)</i>";
                                }
                            case 0x4: // 8XY4 - Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
                                {
                                    //     if ((V[op.X] + V[op.Y]) > 255)
                                    //     {
                                    //         V[0xF] = 1;
                                    //     }
                                    //     else
                                    //     {
                                    //         V[0xF] = 0;
                                    //     }

                                    //     V[op.X] = (byte)(V[op.X] + V[op.Y]);
                                    buffer.InsertWithTagsByName(ref position, "addr", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.Y:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" //V{op.X} = V{op.X} + V{op.Y} Vf set to 1 if there's a carry", "comment");
                                    break;

                                    //return $"addr V{op.X} V{op.Y} <i>//V{op.X} = V{op.X} + V{op.Y} VF set to 1 if there's a carry</i>";
                                }
                            case 0x5: // 8XY5 - VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                                {
                                    // if (V[op.X] > V[op.Y])
                                    // {
                                    //     V[0xF] = 1;
                                    // }
                                    // else
                                    // {
                                    //     V[0xF] = 0;
                                    // }
                                    buffer.InsertWithTagsByName(ref position, "subr", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.Y:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" //V{op.X} = V{op.X} - V{op.Y} Vf set to 0 if there's a borrow", "comment");
                                    break;

                                    // V[op.X] = (byte)(V[op.X] - V[op.Y]);
                                    //return $"subr V{op.X} V{op.Y} <i>//V{op.X} = V{op.X} - V{op.Y} VF set to 0 if there's a borrow</i>";
                                }
                            case 0x6: // 8XY6 - Stores the least significant bit of VX in VF and then shifts VX to the right by 1.
                                {
                                    //V[0xF] = (byte)(V[op.X] & 0x1);
                                    //V[op.X] >>= 1;
                                    buffer.InsertWithTagsByName(ref position, "shr", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" //Stores the least significant bit of V{op.X:x} in Vf and then shifts V{op.X:x} to the right by 1", "comment");
                                    break;
                                    //return $"shr V{op.X}";
                                }
                            case 0x7: // 8XY7 - Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                                {
                                    // if (V[op.Y] > V[op.X])
                                    // {
                                    //     V[0xF] = 1;
                                    // }
                                    // else
                                    // {
                                    //     V[0xF] = 0;
                                    // }

                                    buffer.InsertWithTagsByName(ref position, "nsubr", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.Y:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" //V{op.X} = V{op.Y} - V{op.X} Vf set to 0 if there's a borrow", "comment");
                                    break;


                                    // V[op.X] = (byte)(V[op.Y] - V[op.X]);
                                    //return $"nsubr V{op.X} V{op.Y} </i>//V{op.X} = V{op.Y} - V{op.X} VF set to 0 if there's a borrow<i>";
                                }
                            case 0xE: // 8XYE - Stores the most significant bit of VX in VF and then shifts VX to the left by 1.
                                {
                                    // V[0xF] = (byte)(V[op.X] >> 7);
                                    // V[op.X] <<= 1;
                                    buffer.InsertWithTagsByName(ref position, "shl", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" //Stores the most significant bit of V{op.X:x} in Vf and then shifts V{op.X:x} to the left by 1", "comment");
                                    break;

                                    //return $"shl V{op.X}";
                                }
                        }
                        break;
                    }

                //0x9XY0 - Skips the next instruction if VX doesn't equal VY. (Usually the next instruction is a jump to skip a code block)
                case 0x9:
                    {
                        buffer.InsertWithTagsByName(ref position, "sner", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                        buffer.InsertWithTagsByName(ref position, $" V{op.Y:x}", "variable");
                        buffer.InsertWithTagsByName(ref position, $" //if (V{op.X:x} != V{op.Y:x}) {{ PC += 2; }}", "comment");
                        break;

                        //if (V[op.X] != V[op.Y]) { PC += 2; }
                        //return $"sner V{op.X} V{op.Y}";
                    }

                //0xANNN - Sets I to the address NNN.
                case 0xA:
                    {
                        buffer.InsertWithTagsByName(ref position, "imovi", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" I", "variable");
                        buffer.InsertWithTagsByName(ref position, $" 0x{op.NNN:x3}", "constant");
                        buffer.InsertWithTagsByName(ref position, $" //I = 0x{op.NNN:x3}", "comment");
                        break;
                        //I = op.NNN;
                        //return $"imovi 0x{((ushort)(op.NNN)):x3}";
                    }

                //0xBNNN - Jumps to the address NNN plus V0.
                case 0xB:
                    {
                        buffer.InsertWithTagsByName(ref position, "jumpoff", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" 0x{op.NNN:x3}", "constant");
                        buffer.InsertWithTagsByName(ref position, " + ", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" V0", "variable");
                        buffer.InsertWithTagsByName(ref position, $" //PC = (0x{op.NNN:x3} + V0);", "comment");
                        break;
                        //PC = (ushort)(op.NNN + V[0]);
                        //return $"jumpoff {((ushort)(op.NNN)):x4} + V0";
                    }

                //0xCXNN - Sets VX to the result of a bitwise and operation on a random number (Typically: 0 to 255) and NN.
                case 0xC:
                    {
                        buffer.InsertWithTagsByName(ref position, "rnd", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                        buffer.InsertWithTagsByName(ref position, $" 0x{op.NN:x2}", "constant");
                        buffer.InsertWithTagsByName(ref position, $" //V{op.X:x} = RND() & 0x{op.NN:x2}", "comment");
                        break;


                        //V[op.X] = (byte)(new Random().Next(255) & op.NN);
                        //return $"rnd V{op.X} {op.NN:x2} <i>//V{op.X} = RND() & {op.NN:x2}</i>";
                    }

                //0xDXYN - Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
                //         Each row of 8 pixels is read as bit-coded starting from memory location I; 
                //         I value doesn’t change after the execution of this instruction. 
                //         As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite 
                //         is drawn, and to 0 if that doesn’t happen
                case 0xD:
                    {
                        // byte[] sprite = new byte[op.N];
                        // Array.Copy(Memory, I, sprite, 0, op.N);

                        // bool pixelChanged = Graphics.DrawSprite(V[op.X], V[op.Y], op.N, sprite);

                        // if (pixelChanged)
                        // { V[0xF] = 1; }
                        // else { V[0xF] = 0; }
                        buffer.InsertWithTagsByName(ref position, "sprite", "keyword");
                        buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                        buffer.InsertWithTagsByName(ref position, $" V{op.Y:x}", "variable");
                        buffer.InsertWithTagsByName(ref position, $" 0x{op.N:x}", "constant");
                        buffer.InsertWithTagsByName(ref position, $" //VF = DrawSprite(V{op.X:x}, V{op.Y:x}, 0x{op.N:x}, I) DrawSprite(x,y,h,sprite_memaddress)", "comment");
                        break;

                        //return $"sprite V{op.X}, V{op.Y}, {op.N:x2} <i>//VF = DrawSprite(V{op.X}, V{op.Y}, {op.N:x2}, I) DrawSprite(x,y,h,sprite_memaddress)</i>";
                    }

                //0xEX?? - Handles key presses
                case 0xE:
                    {
                        //EX9E - Skips the next instruction if the key stored in VX is pressed. (Usually the next instruction is a jump to skip a code block)
                        if (op.N == 0xE)
                        {
                            // //Check to see if the key stored in VX was pressed
                            // if (Input.KeyPressed(V[op.X]))
                            // {
                            //     PC += 2; //Skip over the next instruction
                            // }
                            buffer.InsertWithTagsByName(ref position, "skr", "keyword");
                            buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                            buffer.InsertWithTagsByName(ref position, $" // if keypress(V{op.X:x}) skip", "comment");
                            break;

                            //return $"skr V{op.X} <i>//IF KEYPRESS(V{op.X}) SKIP</i>";
                        }
                        else
                        {
                            //EXA1	- Skips the next instruction if the key stored in VX isn't pressed. (Usually the next instruction is a jump to skip a code block)
                            //Check to see if the key stored in VX wasn't pressed
                            // if (!Input.KeyPressed(V[op.X]))
                            // {
                            //     PC += 2; //Skip over the next instruction
                            // }
                            buffer.InsertWithTagsByName(ref position, "snkr", "keyword");
                            buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                            buffer.InsertWithTagsByName(ref position, $" // if !keypress(V{op.X:x}) skip", "comment");
                            break;

                            //return $"snkr V{op.X} <i>//IF !KEYPRESS(V{op.X}) SKIP</i>";
                        }
                    }

                case 0xF:
                    {
                        switch (op.NN)
                        {
                            //0xFX07 - Sets VX to the value of the delay timer.
                            case 0x07:
                                {
                                    //V[op.X] = DelayTimer;
                                    buffer.InsertWithTagsByName(ref position, "rmovt", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" // V{op.X:x} = DelayTimer", "comment");
                                    break;
                                    //return $"rmovt V{op.X} <i>//V{op.X} = DelayTimer</i>";
                                }
                            //0xF0A - A key press is awaited, and then stored in VX. (Blocking Operation. All instruction halted until next key event)
                            case 0x0A:
                                {
                                    // byte keyPressed = 0x00;
                                    // //Check if a key has been pressed
                                    // if (Input.WaitForKey(out keyPressed))
                                    // {
                                    //     //If a key has been pressed put it in VX
                                    //     V[op.X] = keyPressed;
                                    // }
                                    // else
                                    // {
                                    //     PC -= 2; //If no key was pressed move the program counter one instruction back so it keep repeating this instruction.
                                    // }
                                    buffer.InsertWithTagsByName(ref position, "waitk", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" // V{op.X:x} = keypress() -- Block until key pressed", "comment");
                                    break;
                                    //return $"waitk V{op.X} <i>//V{op.X} = KEYPRESS() Block until keypressed</i>";
                                }
                            //0xFX15 - Sets the delay timer to VX.
                            case 0x15:
                                {
                                    //DelayTimer = V[op.X];
                                    buffer.InsertWithTagsByName(ref position, "movt", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" // DelayTimer = V{op.X:x}", "comment");
                                    break;
                                    //return $"movt V{op.X} <i>//DelayTimer = V{op.X}</i>";
                                }
                            //0xFX18 - Sets the sound timer to VX.
                            case 0x18:
                                {
                                    //SoundTimer = V[op.X];
                                    buffer.InsertWithTagsByName(ref position, "movs", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" // SoundTimer = V{op.X:x}", "comment");
                                    break;
                                    //return $"movs V{op.X} <i>//SoundTimer = V{op.X}</i>";
                                }
                            //0xFX1E - Adds VX to I
                            case 0x1E:
                                {
                                    //I += V[op.X];
                                    buffer.InsertWithTagsByName(ref position, "iaddr", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" // I += V{op.X:x}", "comment");
                                    break;
                                    //return $"iaddr V{op.X} <i>//I += V{op.X}</i>";
                                }
                            //0xFX29 - Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                            case 0x29:
                                {
                                    //I = (ushort)(V[op.X] * 5);
                                    buffer.InsertWithTagsByName(ref position, "digit", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    buffer.InsertWithTagsByName(ref position, $" // I is set to the address for the character (0-F) in V{op.X:x}", "comment");
                                    break;
                                    //return $"digit V{op.X} <i>//I is set to the address for the character (0-F) in V{op.X}</i>";
                                }
                            //0xFX33 - Stores the binary-coded decimal representation of VX, with the most significant of three digits at the address in I, the middle digit at I plus 1, and the least significant digit at I plus 2. 
                            case 0x33:
                                {
                                    // decimal num = V[op.X];
                                    // Memory[I] = (byte)(num / 100);
                                    // Memory[I + 1] = (byte)((num % 100) / 10);
                                    // Memory[I + 2] = (byte)((num % 100) % 10);
                                    buffer.InsertWithTagsByName(ref position, "bcd", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    break;

                                    //return $"bcd V{op.X}";
                                }
                            //0xF55 - Stores V0 to VX (including VX) in memory starting at address I. The offset from I is increased by 1 for each value written, but I itself is left unmodified.
                            case 0x55:
                                {
                                    // for (int i = 0; i <= op.X; i++)
                                    // {
                                    //     Memory[I + i] = V[i];
                                    // }
                                    buffer.InsertWithTagsByName(ref position, "store", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    break;

                                    //return $"store V{op.X}";
                                    //break;
                                }
                            //0xFX65 - Fills V0 to VX (including VX) with values from memory starting at address I. The offset from I is increased by 1 for each value written, but I itself is left unmodified.
                            case 0x65:
                                {
                                    // for (int i = 0; i <= op.X; i++)
                                    // {
                                    //     V[i] = Memory[I + i];
                                    // }
                                    buffer.InsertWithTagsByName(ref position, "load", "keyword");
                                    buffer.InsertWithTagsByName(ref position, $" V{op.X:x}", "variable");
                                    break;
                                    //return $"load V{op.X}";
                                }
                        }
                        break;
                    }
            }
            if (position.BytesInLine > 120)
            {
                buffer.InsertWithTagsByName(ref position, "\n", "white");
            }
            else
            {
                var txt = "".PadLeft(120 - position.BytesInLine, ' ') + "\n";
                buffer.InsertWithTagsByName(ref position, txt, "white");
            }

        }
    }
}