using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Midi;
using MidiDotNetTest;
using Newtonsoft.Json.Linq;


namespace LaundpadZ
{
    class Drawer
    {
        public Engine engine = new Engine(InputDevice.InstalledDevices[0], OutputDevice.InstalledDevices[1]);
        public void Start() {
            engine.Clear();
        }

        public void ShowImage(string img) {
            ValueTuple<int, int, int>[,] lightArray = LoadJson(img);

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    engine.SetNoteRGB((j, i), lightArray[i, j]);
                }
            }
        }

        public ValueTuple<int, int, int>[,] LoadJson(string img) {
            Console.WriteLine("Loading Json");
            (int, int, int) tup;
            ValueTuple<int, int, int>[,] rgbValues = new ValueTuple<int, int, int>[8, 8];
            string path = System.IO.File.ReadAllText(@"C:\Users\Zak Body\source\repos\Launchpad-Z\LaunchpadZ\LPImages.json");

            JObject json = JObject.Parse(path);
            JToken data = json[img]["data"];
            Console.WriteLine(data[0][0][0]);
            for (int i = 0; i < data.Count(); i++) {
                for (int j = 0; j < data[i].Count(); j++) {
                    tup.Item1 = (int)data[i][j][0];
                    tup.Item2 = (int)data[i][j][1];
                    tup.Item3 = (int)data[i][j][2];
                    rgbValues[i, j] = tup;

                }
            }
            return rgbValues;
        }

        public void End() {
            engine.Clear();
        }
    }
}
