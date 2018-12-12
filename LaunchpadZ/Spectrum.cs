using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Un4seen.Bass;

namespace LaundpadZ
{
    class Spectrum
    {
        private int channel = 0;
        public void Start() {

            Console.WriteLine("Starting");
            if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)) {
                // create a stream channel from a file 
                int stream = Bass.BASS_StreamCreateFile(@"C:\Users\Zak Body\Desktop\LPSongs\Gypsy thing.wav", 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);
                if (stream != 0) {
                    // play the stream channel 
                    Bass.BASS_ChannelPlay(stream, false);
                    
                }
                else {
                    // error creating the stream 
                    Console.WriteLine("Stream error: {0}", Bass.BASS_ErrorGetCode());
                }

                // wait for a key 
                var key = Console.ReadKey(true).KeyChar;
                float[] test = new float[256];
                Console.WriteLine("Press any key to exit");

                while (key.ToString() != "s") {
                    float[] fft = new float[2048];
                    Bass.BASS_ChannelGetData(channel, fft, (int)BASSData.BASS_DATA_FFT4096);
                    // assuming the channel's samplerate is 44.1kHz,
                    // this will return the frequency represented by bucket 51
                    int hz = Utils.FFTIndex2Frequency(20, 4096, 44100);
                    Console.WriteLine(hz);
                    key = Console.ReadKey(true).KeyChar;
                }
                

                // free the stream 
                Bass.BASS_StreamFree(stream);
                // free BASS 
                Bass.BASS_Free();
            }
        }

        public double[] FFT(double[] data) {
            double[] fft = new double[data.Length];
            System.Numerics.Complex[] fftComplex = new System.Numerics.Complex[data.Length];
            for (int i = 0; i < data.Length; i++)
                fftComplex[i] = new System.Numerics.Complex(data[i], 0.0);
            for (int i = 0; i < data.Length; i++)
                fft[i] = fftComplex[i].Magnitude;
            return fft;
        }
    }
}
