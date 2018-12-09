using System;
using System.Collections.Generic;
using System.Text;
using MidiDotNetTest;
using LaundpadZ;
using Midi;
using System.Threading;
using System.Linq;

namespace LaundpadZ
{
    class Minesweeper
    {
        private Engine engine;
        public Pitch[,] notes = Engine.notes;
        public int[,] mineField = new int[8, 8];
        public int[,] flags = new int[8, 8];
        private int pressCount = 0;
        public Pitch[] rightLEDnotes = Engine.rightLEDnotes;
        private int rightNote = 0;
        public bool running = true;

        private int[,] clearMap = new int[8, 8];
        private int[,] cleared = new int[8, 8];

        private (int, int ,int)[] MINECOLOUR = new(int, int, int)[8] {
            (2, 99, 255),
            (10, 255, 14),
            (255, 15, 55),
            (139, 15, 255),
            (128,0,0),
            (64,224,208),
            (255, 0, 187),
            (30, 30, 30)
            };

        private Random rnd = new Random();


        public Minesweeper(Engine eng) {
            engine = eng;
            engine.inputDevice.NoteOn += new InputDevice.NoteOnHandler(ButtonPress);
            engine.inputDevice.StartReceiving(null);
        }

        public void Start() {
            engine.Clear();
            //engine.DrawRect((1, 1), (3, 3), (0, 0, 255));
            engine.DrawRect((0, 0), (7, 7), (100, 100, 100), filled: true);
            GenerateMines();
            GenerateClearMap();
            int count = 2;
            foreach (int button in rightLEDnotes) {
                if (count < 8)
                    engine.RightRGB(count, MINECOLOUR[count-2]);
                count++;

            }


        }

        private void GenerateMines() {
            int mineX;
            int mineY;
            for (int i = 0; i <= 9; i++) {
                mineX = rnd.Next(0,8);
                mineY = rnd.Next(0, 8);

                mineField[mineY, mineX] = 1;
                //engine.SetNoteRGB((mineX, mineY), (255, 0, 0));
            }
        }

        private int MineCount((int, int) coord) {
            int mineCount = 0;
            (int, int) coord1 = (coord.Item1 -1, coord.Item2 -1);

            (int, int) coord2 = (coord1.Item1 +2, coord1.Item2 +2);

            
            for (int y = coord1.Item2; y <= coord2.Item2; y++) {
                if (y >= 0 && coord1.Item1 >= 0 && y < 8 && coord1.Item1 < 8) {
                    if (mineField[y, coord1.Item1] == 1) {
                        mineCount++;
                    }
                }
            }

            for (int y = coord2.Item2; y >= coord1.Item2; y--) {
                if (y >= 0 && coord2.Item1 >= 0 && y < 8 && coord2.Item1 < 8) {

                    if (mineField[y, coord2.Item1] == 1) {

                        mineCount++;
                    }
                }
            }
            if (coord.Item2 + 1 < 8) {
                if (mineField[coord.Item2 + 1, coord.Item1] == 1) {
                    mineCount++;
                }
            }
            if (coord.Item2 - 1 >= 0) {
                if (mineField[coord.Item2 - 1, coord.Item1] == 1) {
                    mineCount++;

                }
            }
            

            return mineCount;
        }


        private void SetMineIndicator((int, int) coord, int mineCount) {
            mineCount -= 1;
            if (mineCount != -1 && mineField[coord.Item2, coord.Item1] == 0) {
                engine.SetNoteRGB(coord, MINECOLOUR[mineCount]);
            }
            else if (mineCount == -1){
                engine.NoteOff(coord);

            }

            if (mineField[coord.Item2, coord.Item1] == 1){
                engine.SetNoteRGB(coord, (100, 100, 100));
            }


        }

        private void GenerateClearMap() {
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    clearMap[x,y] = MineCount((x, y));
                }
            }
        }

        //Don't look, it's so awful
        //But it gets the job done (Kinda)
        private void ClearSpace((int, int) coord) {
            (int, int) coord1 = (coord.Item1 - 1, coord.Item2 - 1);

            (int, int) coord2 = (coord1.Item1 + 2, coord1.Item2 + 2);

            SetMineIndicator(coord, MineCount(coord));

            for (int y = coord1.Item2; y <= coord2.Item2; y++) {
                if (y >= 0 && coord1.Item1 >= 0 && y < 8 && coord1.Item1 < 8) {
                    if (clearMap[coord1.Item1, y] == 0 && cleared[coord1.Item1, y] == 0) {
                        cleared[coord1.Item1, y] = 1;
                        ClearSpace((coord1.Item1, y));
                    }
                    if (mineField[coord1.Item1, y] == 0)
                        SetMineIndicator((coord1.Item1, y), clearMap[coord1.Item1, y]);

                }
            }

            for (int y = coord2.Item2; y >= coord1.Item2; y--) {
                if (y >= 0 && coord2.Item1 >= 0 && y < 8 && coord2.Item1 < 8) {

                    if (clearMap[coord2.Item1, y] == 0 && cleared[coord2.Item1, y] == 0) {
                        cleared[coord2.Item1, y] = 1;
                        ClearSpace((coord2.Item1, y));

                    }
                    if (mineField[coord2.Item1, y] == 0)
                        SetMineIndicator((coord2.Item1, y), clearMap[coord2.Item1, y]);

                }
            }
            if (coord.Item2 + 1 < 8) {
                if (clearMap[coord.Item1, coord.Item2 + 1] == 0 && cleared[coord.Item1, coord.Item2] == 0) {
                    cleared[coord.Item1, coord.Item2 + 1] = 1;
                    ClearSpace((coord.Item1, coord.Item2 + 1));

                }

                if (mineField[coord.Item1, coord.Item2 + 1] == 0)
                    SetMineIndicator((coord.Item1, coord.Item2 + 1), clearMap[coord.Item1, coord.Item2 + 1]);

            }
            if (coord.Item2 - 1 >= 0) {
                if (clearMap[coord.Item1, coord.Item2 - 1] == 0 && cleared[coord.Item1, coord.Item2 - 1] == 0) {
                    cleared[coord.Item1, coord.Item2 - 1] = 1;
                    SetMineIndicator((coord.Item1, coord.Item2 - 1), clearMap[coord.Item1, coord.Item2 - 1]);
                    ClearSpace((coord.Item1, coord.Item2 - 1));

                }
                if (mineField[coord.Item1, coord.Item2 - 1] == 0)
                    SetMineIndicator((coord.Item1, coord.Item2 - 1), clearMap[coord.Item1, coord.Item2 - 1]);

            }
        }

        private void CreateFlag((int, int) coord) {
            if (flags[coord.Item2, coord.Item1] == 0) {
                flags[coord.Item2, coord.Item1] = 1;
                engine.SetNoteRGB(coord, ((255, 238, 0)));

            } else if (flags[coord.Item2, coord.Item1] == 1) {
                flags[coord.Item2, coord.Item1] = 0;
                engine.SetNoteRGB(coord, ((100, 100, 100)));

            }

        }

        private void DoubleClick(NoteOnMessage msg) {
            if (pressCount == 1) {
                Click(msg, false);
            }
            else if (pressCount > 1) {
                Click(msg, true);
            }
            pressCount = 0;
        }

        private void Click(NoteOnMessage msg, bool twice) {
            int x = 0;
            int y = 0;
            bool pressed = false;
            for (int i = 0; i < notes.GetLength(0); i++) {
                for (int j = 0; j < notes.GetLength(1); j++) {
                    if (notes[i, j] == msg.Pitch) {
                        x = j;
                        y = i;
                        if (msg.Velocity == 127) {
                            pressed = true;
                        }
                        else {
                            pressed = false;
                        }

                    }
                }
            }

            if (pressed) {
                if (mineField[y, x] == 1 && !twice && flags[y, x] == 0) {
                    Console.WriteLine("Boom you are dead");
                    engine.SetNoteRGB((x, y),(255,0, 0));
                    Thread.Sleep(100);
                    engine.SetNoteRGB((x, y), (0, 0, 0));
                    Thread.Sleep(100);
                    engine.SetNoteRGB((x, y), (255, 0, 0));
                    StopGame();
                }
                else if (mineField[y, x] == 0 && !twice && flags[y, x] == 0 && clearMap[x, y] == 0) {
                    ClearSpace((x, y));
                }
                else if (mineField[y, x] == 0 && !twice && flags[y, x] == 0) {
                    SetMineIndicator((x, y), MineCount((x, y)));

                }
                else if (twice) {
                    CreateFlag((x, y));
                } 
            }
        }


        private void ButtonPress(NoteOnMessage msg) {
            if (running) {
                if (msg.Velocity == 127 && !rightLEDnotes.Contains(msg.Pitch)) {
                    pressCount++;
                    Timer doubleClick = new Timer(state => DoubleClick(msg), 0, 250, 0);

                }
                else if (msg.Velocity == 127 && rightLEDnotes.Contains(msg.Pitch)) {
                    rightNote = Array.IndexOf(rightLEDnotes, msg.Pitch);
                    Click(msg, false);
                }
            }
            
        }

        public void StopGame() {

            int count = 2;
            foreach (int button in rightLEDnotes) {
                if (count < 8)
                    engine.NoteOffRight(count);
                count++;

            }

            engine.Clear();
            
        }
    }
}
