using System;
using Midi;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using GlobalHotKey;

namespace MidiDotNetTest {
    class Engine
    {
        public (int, int, int) defaultColour = (15, 161, 252);
        public bool idle = false;
        public bool viewKey = false;

        public static Pitch[,] notes = new Pitch[8, 8] {
            { Pitch.A5, Pitch.ASharp5, Pitch.B5, Pitch.C6, Pitch.CSharp6, Pitch.D6, Pitch.DSharp6, Pitch.E6 },
            { Pitch.B4, Pitch.C5, Pitch.CSharp5, Pitch.D5, Pitch.DSharp5, Pitch.E5, Pitch.F5, Pitch.FSharp5 },
            { Pitch.CSharp4, Pitch.D4, Pitch.DSharp4, Pitch.E4, Pitch.F4, Pitch.FSharp4, Pitch.G4, Pitch.GSharp4 },
            { Pitch.DSharp3, Pitch.E3, Pitch.F3, Pitch.FSharp3, Pitch.G3, Pitch.GSharp3, Pitch.A3, Pitch.ASharp3 },
            { Pitch.F2, Pitch.FSharp2, Pitch.G2, Pitch.GSharp2, Pitch.A2, Pitch.ASharp2, Pitch.B2, Pitch.C3 },
            { Pitch.G1, Pitch.GSharp1, Pitch.A1, Pitch.ASharp1, Pitch.B1, Pitch.C2, Pitch.CSharp2, Pitch.D2 },
            { Pitch.A0, Pitch.ASharp0, Pitch.B0, Pitch.C1, Pitch.CSharp1, Pitch.D1, Pitch.DSharp1, Pitch.E1 },
            { Pitch.BNeg1, Pitch.C0, Pitch.CSharp0, Pitch.D0, Pitch.DSharp0, Pitch.E0, Pitch.F0, Pitch.FSharp0 }
        };

        public static Pitch[] rightLEDnotes = new Pitch[] {
            Pitch.F6, Pitch.G5, Pitch.A4, Pitch.B3, Pitch.CSharp3, Pitch.DSharp2, Pitch.F1, Pitch.G0
        };

        public InputDevice inputDevice;
        public OutputDevice outputDevice;

        public Engine(InputDevice inputDevice, OutputDevice outputDevice) {


            this.inputDevice = inputDevice;
            this.outputDevice = outputDevice;

            this.outputDevice.Open();

            this.inputDevice.Open();
            Clear();

            DateTime now = DateTime.UtcNow;
        }

        public void ViewKey() {
            Clear();

            Console.WriteLine("Reading keys..");
            var key = Console.ReadKey(true).KeyChar;


            while (key.ToString() != "\r" && !viewKey) {
                PrintLetter(key.ToString().ToUpper());
                key = Console.ReadKey(true).KeyChar;
                Clear();
            }

        }



        public void Idle() {
            Clear();
            Random rnd = new Random();
            while (idle) {
                Thread.Sleep(150);
                for (int x = 0; x < notes.GetLength(0); x++) {
                    for (int y = 0; y < notes.GetLength(1); y++) {
                        if (idle)
                            SetNoteRGB((x, y), (5, Convert.ToInt32(rnd.Next(50, 175) * (y + 1) / 5), Convert.ToInt32(rnd.Next(100, 256) / (y + 1) * 2.3)));
                    }
                }
            }
        }



        public void ButtonPress(NoteOnMessage msg) {
            //outputDevice.SendNoteOn(Channel.Channel1, msg.Pitch, (int)msg.Pitch);
            int x = 0;
            int y = 0;
            for (int i = 0; i < notes.GetLength(0); i++) {
                for (int j = 0; j < notes.GetLength(1); j++) {
                    if (notes[i, j] == msg.Pitch && msg.Velocity == 127) {
                        x = j;
                        y = i;
                    }
                }
            }
        }

        public void PrintLetter(string letter) {
            Clear();
            JToken lightArray = LoadJson(letter);
            JArray jsonArray = (JArray)lightArray;
            if (jsonArray != null) {
                for (var i = 0; i < jsonArray.Count; i++) {

                    outputDevice.SendNoteOn(Channel.Channel1, notes[(int)lightArray[i][0], (int)lightArray[i][1]], 50);
                }
            }
        }

        public void Clear() {
            foreach (var pitch in notes) {
                outputDevice.SendNoteOff(Channel.Channel1, pitch, 10);
            }
        }

        public void ScrollText(string text, int speed, int velocity) {
            byte[] sysStop = { 0xF7 };
            byte _speed = 5;
            _speed = (byte)speed;
            byte _velocity = (byte)velocity;
            //byte[] _text = { };

            List<byte> charList = new List<byte>();

            foreach(char n in text) {
                int unicode = n;
                charList.Add(Convert.ToByte(unicode));
            }

            IEnumerable<byte> _text = charList.AsEnumerable<byte>();

            byte[] args = { _velocity, 0};

            List<byte> command = new List<byte> { 240, 00, 32, 41, 2, 4, 20, _velocity, 0, _speed, 247 };
            command.InsertRange(command.Count-1,_text);

            outputDevice.SendSysEx(command.ToArray());
        }

        private JToken LoadJson(string letter) {
            string text = System.IO.File.ReadAllText(@"C:\Users\Zak Body\source\repos\Launchpad-Z\LaunchpadZ\Chars.json");
            //JsonTextReader reader = new JsonTextReader(new System.IO.StringReader(text));
            //while (reader.Read()) {

            //}
            JObject json = JObject.Parse(text);
            JToken character = json["letters"][letter];


            return character;
        }

        public void NoteOff((int, int) coord) {
            outputDevice.SendNoteOff(Channel.Channel1, notes[coord.Item2, coord.Item1], 1);
        }

        public void NoteOffRight(int coord) {
            outputDevice.SendNoteOff(Channel.Channel1, rightLEDnotes[coord], 1);

        }

        public void NoteOnRight(int coord, int velocity = 80) {
            outputDevice.SendNoteOn(Channel.Channel1, rightLEDnotes[coord], velocity);
        }

        public void RightRGB(int index, (int, int, int) rgb) {
            outputDevice.SendSysEx(new byte[] { 0xF0, 0x00, 0x20, 0x29, 0x02, 0x18, 0x0B,
                Convert.ToByte((int)rightLEDnotes[index]),
                Convert.ToByte(ByteMap(rgb.Item1)),
                Convert.ToByte(ByteMap(rgb.Item2)),
                Convert.ToByte(ByteMap(rgb.Item3)), 0xF7 });
        }

        public void DrawRect((int, int) coord1, (int, int) coord2, (int, int, int) colour, bool filled = false) {
            SetNoteRGB(coord1, (200, 5, 5));
            SetNoteRGB(coord2, (200, 5, 5));
            if (filled) {
                //Filled rect
                for (int x = coord1.Item1; x <= coord2.Item1; x++) {
                    for (int y = coord1.Item2; y <= coord2.Item2; y++) {
                        SetNoteRGB((x, y), colour);
                    }
                }

            }
            else {
                for (int x = coord1.Item1; x <= coord2.Item1; x++) {

                    SetNoteRGB((x, coord1.Item2), colour);

                    for (int y = coord1.Item2; y <= coord2.Item2; y++) {

                        SetNoteRGB((coord1.Item1, y), colour);
                    }
                }

                for (int x = coord2.Item1; x > coord1.Item1; x--) {
                    SetNoteRGB((x, coord2.Item2), colour);

                    for (int y = coord2.Item2; y > coord1.Item2; y--) {

                        SetNoteRGB((coord2.Item1, y), colour);
                    }
                }
            }
        }


        public void NoteOn((int, int) coord, int velocity = 80) {
            outputDevice.SendNoteOn(Channel.Channel1, notes[coord.Item2, coord.Item1], velocity);
        }

        public void SetNoteRGB((int,int) coord, (int,int,int) rgb) {
            
            outputDevice.SendSysEx(new byte[] { 0xF0, 0x00, 0x20, 0x29, 0x02, 0x18, 0x0B,
                Convert.ToByte((int)notes[coord.Item2, coord.Item1]),
                Convert.ToByte(ByteMap(rgb.Item1)),
                Convert.ToByte(ByteMap(rgb.Item2)),
                Convert.ToByte(ByteMap(rgb.Item3)), 0xF7 });

        }
        public static float Map(float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static int ByteMap(float value) {
            //Console.WriteLine((int)Math.Round(Map(value, 0, 255, 0, 63)));
            return (int)Math.Round(Map(value, 0, 255, 0, 63));
        }
    }
}
