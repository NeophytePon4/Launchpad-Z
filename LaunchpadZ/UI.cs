using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
//using System.Timers;
using Midi;
using MidiDotNetTest;
using System.Text.RegularExpressions;

namespace LaundpadZ {
    class UI {
        private Engine engine;
        public Pitch[,] notes = Engine.notes;
        public Pitch[] rightLEDnotes = Engine.rightLEDnotes;
        private int rightNote;
        private ReactionGame game;
        private Minesweeper minesweeper;

        public int[,] buttonMap = new int[8, 8];
        public bool clock = false;
        private int pressCount = 0;
        Thread app;

        public UI(Engine eng) {
            engine = eng;
            
            engine.inputDevice.NoteOn += new InputDevice.NoteOnHandler(ButtonPress);
            engine.inputDevice.StartReceiving(null);
            engine.NoteOnRight(0, 6);
            CreateButtons();
            GetConfig();
            //foreach (object obj in GetConfig()) {
            //    Console.WriteLine(obj.ToString());
            //}
            //var test = GetConfig();
            //(int, int, int) tup = (ValueTuple<int, int, int>)test[1];
            
            //Console.WriteLine(tup.Item1);
            Console.ReadKey();
        }

        public void CreateButtons() {
            ArrayList config = GetConfig();
            (int, int, int) colour1 = (ValueTuple<int, int, int>)config[config.Count - 5];
            (int, int, int) colour2 = (ValueTuple<int, int, int>)config[config.Count - 4];
            (int, int, int) colour3 = (ValueTuple<int, int, int>)config[config.Count - 3];
            (int, int, int) colour4 = (ValueTuple<int, int, int>)config[config.Count - 2];
            (int, int, int) colour5 = (ValueTuple<int, int, int>)config[config.Count - 1];

            CreateButton((2, 2), (5, 5), colour1); //Idle display - middle
            CreateButton((6, 0), (7, 1), colour2, id: 2); //Keys pressed - top right
            CreateButton((0, 0), (1, 1), colour3, id: 3); //Clock  - top left
            CreateButton((2, 0), (5, 1), colour4, id: 4); //Reaction game - top middle
            CreateButton((2, 6), (5, 7), colour5, id: 5); //Minesweeper - bottom middle
        }

        public void DoubleClick(NoteOnMessage msg) {
            if (pressCount == 1) {
                Click(msg, false);
            } else if (pressCount > 1) {
                Click(msg, true);
            }

            pressCount = 0;

        }

        public void Click(NoteOnMessage msg, bool twice) {
            ArrayList config = GetConfig();
            bool pressed = false;
            int x = 0;
            int y = 0;
            int buttonId = 0;
            if (!rightLEDnotes.Contains(msg.Pitch)) {
                for (int i = 0; i < notes.GetLength(0); i++) {
                    for (int j = 0; j < notes.GetLength(1); j++) {
                        if (notes[i, j] == msg.Pitch) {
                            x = j;
                            y = i;
                            if (msg.Velocity == 127) {
                                pressed = true;
                                buttonId = buttonMap[y, x];
                            }
                            else {
                                pressed = false;
                                buttonId = 0;
                            }

                        }
                    }
                }
            }
            else {
                pressed = true;
            }
            
            if (pressed) {
                //Console.WriteLine("Button pressed at X:" + x + " Y:" + y + " Button Id:" + buttonId);

                if (rightNote == 0 && rightLEDnotes.Contains(msg.Pitch)) {

                    engine.idle = false;
                    engine.viewKey = true;
                    clock = false;
                    if (game != null)
                        game.running = false;
                    if (minesweeper != null) {
                        minesweeper.StopGame();
                        minesweeper.running = false;
                    }

                    Thread.Sleep(10);
                    engine.Clear();
                    CreateButtons();

                }
                else {

                }
                if (buttonId == 1 && twice) {
                    engine.ScrollText("Does a cool idle thingy", (int)config[1], 3);
                } else if (buttonId == 1 && !twice) {
                    ClearMap();
                    engine.idle = true;
                    app = new Thread(new ThreadStart(engine.Idle));
                    app.Start();
                }
                if (buttonId == 2 && twice) {
                    engine.ScrollText("Displays the key you pressed", (int)config[1], 3);

                }
                else if (buttonId == 2 && !twice) {
                    ClearMap();
                    engine.viewKey = false;
                    app = new Thread(new ThreadStart(engine.ViewKey));
                    app.Start();
                }

                if (buttonId == 3 && twice) {
                    engine.ScrollText("Shows the time", (int)config[1], 3);

                }
                else if (buttonId == 3 && !twice) {
                    ClearMap();
                    clock = true;
                    app = new Thread(new ThreadStart(Clock));
                    app.Start();
                }

                if (buttonId == 4 && twice) {
                    engine.ScrollText("Press the blue buttons as fast as you can!", (int)config[1], 3);

                }
                else if (buttonId == 4 && !twice) {
                    engine.inputDevice.StopReceiving();
                    game = new ReactionGame(engine);

                    ClearMap();
                    app = new Thread(new ThreadStart(game.Start));
                    app.Start();
                }

                if (buttonId == 5 && twice) {
                    engine.ScrollText("Minesweeper", (int)config[1], 3);

                }
                else if (buttonId == 5 && !twice) {
                    engine.inputDevice.StopReceiving();
                    minesweeper = new Minesweeper(engine);

                    ClearMap();
                    app = new Thread(new ThreadStart(minesweeper.Start));
                    app.Start();
                }

            }
        }

        public static ArrayList GetConfig() {
            Regex regex = new Regex(@"(?<=:)[^\]]+(?<=;)");
            Match match;
            string value;
            string[] stringArr;
            (int, int, int) tup;
            ArrayList config = new ArrayList { };
            string[] lines = System.IO.File.ReadAllLines(@"C:\Users\Zak Body\source\repos\Launchpad-Z\LaunchpadZ\config.txt");

            int count = 0;
            foreach(string line in lines) {
                match = regex.Match(lines[count]);
                value = match.Value.TrimEnd(';');


                if (value[0] == '(') {
                    stringArr = value.ToString().Replace("(", "").Replace(")", "").Split(",");
                    int[] intArr = Array.ConvertAll(stringArr, s => int.Parse(s));
                    tup.Item1 = intArr[0];
                    tup.Item2 = intArr[1];
                    tup.Item3 = intArr[2];
                    config.Add(tup);
                }
                else {
                    config.Add(Convert.ToInt32(value));
                }
                count++;
            }

            return config;
        }

        public void ButtonPress(NoteOnMessage msg) {
            ArrayList config = GetConfig();
            if (msg.Velocity == 127 && !rightLEDnotes.Contains(msg.Pitch)) {
                pressCount++;
                Timer doubleClick = new Timer(state => DoubleClick(msg), 0, (int)config[0], 0);

            } else if (msg.Velocity == 127 && rightLEDnotes.Contains(msg.Pitch)) {
                rightNote = Array.IndexOf(rightLEDnotes, msg.Pitch);
                Click(msg, false);
            }
        }

        public void CreateButton((int, int) coord1, (int, int) coord2, (int, int, int) colour, bool filled = true, int id = 1) {
            engine.SetNoteRGB(coord1, (200, 5, 5));
            engine.SetNoteRGB(coord2, (200, 5, 5));
            if (filled) {
                //Filled rect
                for (int x = coord1.Item1; x <= coord2.Item1; x++) {
                    for (int y = coord1.Item2; y <= coord2.Item2; y++) {
                        engine.SetNoteRGB((x, y), colour);
                        buttonMap[y, x] = id;
                    }
                }

            }
            else {
                for (int x = coord1.Item1; x <= coord2.Item1; x++) {
                    Console.WriteLine(x);
                    engine.SetNoteRGB((x, coord1.Item2), colour);

                    for (int y = coord1.Item2; y <= coord2.Item2; y++) {
                        engine.SetNoteRGB((coord1.Item1, y), colour);
                    }
                }

                for (int x = coord2.Item1; x >= coord1.Item1; x--) {
                    Console.WriteLine(x);
                    engine.SetNoteRGB((x, coord2.Item2), colour);

                    for (int y = coord2.Item2; y >= coord1.Item2; y--) {
                        engine.SetNoteRGB((coord2.Item1, y), colour);
                    }
                }
            }
        }

        public void ClearMap() {
            Array.Clear(buttonMap, 0, buttonMap.Length);
        }

        public void Clock() {
            DateTime current = DateTime.UtcNow;
            while (clock) {
                current = DateTime.UtcNow;
                engine.ScrollText(current.ToShortTimeString(), 5, 3);
                Thread.Sleep(3500);
            }
        }
    }
}
