using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Midi;
using MidiDotNetTest;
using Newtonsoft.Json.Linq;


namespace LaundpadZ
{
    class Drawer
    {
        public Engine engine = new Engine(InputDevice.InstalledDevices[0], OutputDevice.InstalledDevices[1]);
        public int fps = 83;
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

        public void Display(string img) {
            if (GetType(img) == "gif") {
                PlayGif(img);
            } else if (GetType(img) == "img") {
                ShowImage(img);
            }
            else {
                Console.WriteLine("Invalid type. Did you edit the LPImages.json?");
            }
        }

        public string GetType(string img) {
            string path = System.IO.File.ReadAllText(@"C:\Users\Zak Body\source\repos\Launchpad-Z\LaunchpadZ\LPImages.json");
            JObject json = JObject.Parse(path);
            
            return json[img]["type"].ToString();
        }

        public void PlayGif(string gif) {
            ValueTuple<int, int, int>[][,] lightArray = LoadJsonGif(gif);

            for (int frame = 0; frame < lightArray.Length; frame++) {
                for (int i = 0; i < 8; i++) {
                    for (int j = 0; j < 8; j++) {
                        engine.SetNoteRGB((j, i), lightArray[frame][i, j]);
                    }
                }
                Thread.Sleep(fps);
            }
        }
        public List<string> GetImages() {
            string path = System.IO.File.ReadAllText(@"C:\Users\Zak Body\source\repos\Launchpad-Z\LaunchpadZ\LPImages.json");
            JObject json = JObject.Parse(path);

            return json.Properties().Select(p => p.Name).ToList();
        }

        public ValueTuple<int, int, int>[,] LoadJson(string img) {
            Console.WriteLine("Loading Json");
            (int, int, int) tup;
            ValueTuple<int, int, int>[,] rgbValues = new ValueTuple<int, int, int>[8, 8];
            string path = System.IO.File.ReadAllText(@"C:\Users\Zak Body\source\repos\Launchpad-Z\LaunchpadZ\LPImages.json");
            if (GetImages().Contains(img)) {
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
            else {
                return rgbValues;
            }
            
        }

        public ValueTuple<int, int, int>[][,] LoadJsonGif(string gif) {
            Console.WriteLine("Loading Json");
            (int, int, int) tup;
            string path = System.IO.File.ReadAllText(@"C:\Users\Zak Body\source\repos\Launchpad-Z\LaunchpadZ\LPImages.json");
            if (GetImages().Contains(gif)) {
                JObject json = JObject.Parse(path);
                JToken data = json[gif]["data"];
                ValueTuple<int, int, int>[][,] rgbValues = new ValueTuple<int, int, int>[json[gif]["data"].Count()][,];

                for (int i = 0; i < json[gif]["data"].Count(); i++) {
                    rgbValues[i] = new ValueTuple<int, int, int>[8, 8];
                }


                //ValueTuple<int, int, int>[][,] rgbValues = new ValueTuple<int, int, int>[2][,];

                for (int frame = 0; frame < rgbValues.Count(); frame++) {
                    for (int i = 0; i < data[frame].Count(); i++) {
                        for (int j = 0; j < data[frame][i].Count(); j++) {

                            tup.Item1 = (int)data[frame][i][j][0];
                            tup.Item2 = (int)data[frame][i][j][1];
                            tup.Item3 = (int)data[frame][i][j][2];
                            rgbValues[frame][i, j] = tup;
                        }
                    }
                }
                return rgbValues;
            }
            else {
                ValueTuple<int, int, int>[][,] rgbValues = new ValueTuple<int, int, int>[0][,];
                return rgbValues;
            }
        }

        public void End() {
            engine.Clear();
        }
    }
}
