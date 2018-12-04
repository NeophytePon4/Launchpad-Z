using System;
using System.Collections.Generic;
using System.Text;
using LaundpadZ;
using MidiDotNetTest;
using Midi;
using System.Linq;
using System.Timers;
using System.Threading;
namespace LaundpadZ
{
    class ReactionGame
    {
        public Pitch[,] notes = Engine.notes;
        private Random rnd = new Random();
        public int score = 0;
        private DateTime lastPressed = DateTime.Now;
        private int combo = 1;
        private Engine engine;
        private System.Timers.Timer game;
        private bool red = false;

        public const int COMBO_TIME = 250;

        private int rightNote;

        public int totalScore = 0;

        private List<int[]> nodes = new List<int[]>() {
            new int[] { 0, 0 },
            new int[] { 0, 0 },
            new int[] { 0, 0 }
        };
        public bool running = true;

        public ReactionGame(Engine eng) {
            engine = eng;
            engine.inputDevice.NoteOn += new InputDevice.NoteOnHandler(ButtonPress);
            engine.inputDevice.StartReceiving(null);
        }

        public void Start() {
            
            engine.Clear();
            engine.PrintLetter("3");
            Thread.Sleep(1000);
            engine.PrintLetter("2");
            Thread.Sleep(1000);
            engine.PrintLetter("1");
            Thread.Sleep(1000);

            running = true;

            game = new System.Timers.Timer(30000);
            game.Elapsed += StopGameTimer;
            game.Enabled = true;
            
            Init();

        }

        public void StopGameTimer(Object o, ElapsedEventArgs e) {
            if (running) {
                totalScore = score;
                Console.WriteLine("Game Over");
                engine.ScrollText("You scored " + score, 5, 20);
                StopGame();
            }
            
        }

        public void StopGame() {
            engine.NoteOffRight(1);

            running = false;
            game.Close();
            engine.Clear();
            if (engine.inputDevice.IsReceiving)
                engine.inputDevice.StopReceiving();
            if (!engine.inputDevice.IsReceiving)
                engine.inputDevice.StartReceiving(null);
        }
        

        public void Init() {
            engine.Clear();
            engine.DrawRect((1,1),(6,6),(30, 30, 30));
            engine.DrawRect((0, 0), (7, 7), (30, 30, 30));

            NewNode(0);
            NewNode(1);
            NewNode(2);
            

            Console.ReadKey();
        }

        private void NewNode(int index) {
            if (nodes[index][0] != 0)
                engine.NoteOff((nodes[index][0], nodes[index][1]));
            int x = rnd.Next(2, 6);
            int y = rnd.Next(2, 6);

            if (!nodes.Any(p => p.SequenceEqual(new int[] { x, y }))) {
                engine.SetNoteRGB((x, y), (5, 5, 175));

                nodes[index] = new int[] { x, y };
            }
            else {
                NewNode(index);
            }
        }

        private void ButtonPress(NoteOnMessage msg) {
            if (running) {
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

                int ButtonHit = nodes.FindIndex(l => Enumerable.SequenceEqual(new int[] { x, y }, l));
                if (DateTime.Now > lastPressed.AddMilliseconds(COMBO_TIME) & pressed) {
                    combo = 1;
                    lastPressed = DateTime.Now;
                }
                else if (DateTime.Now < lastPressed.AddMilliseconds(COMBO_TIME) && !(combo >= 5) & pressed) {
                    combo++;
                    lastPressed = DateTime.Now;
                }
                else if (pressed) {
                    lastPressed = DateTime.Now;
                }

                if (ButtonHit != -1 && pressed) {
                    NewNode(ButtonHit);
                    score += combo;

                    if (red)
                        engine.DrawRect((1, 1), (6,6),(30, 30, 30));
                    red = false;

                }
                else if (ButtonHit == -1 && pressed) {
                    combo = 1;
                    score -= 5;
                    red = true;
                    engine.DrawRect((1, 1), (6, 6), (100, 0, 0));

                }
            }


        }
    }
}
