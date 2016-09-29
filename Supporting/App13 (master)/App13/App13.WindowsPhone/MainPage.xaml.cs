using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

using Microsoft.Band;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;
using Microsoft.Band.Sensors;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace App13
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        double xVal;
        double yVal;
        double zVal;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

         private IBandClient bandClient; //Portal into the functionality of the band

         protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.New)
                 //in this case all of the initialization isdone on the first load of the page
                 return;

             var bands = await BandClientManager.Instance.GetBandsAsync();
             //Going to the client manager, getting the singleton instance, get the bands, do this asynchronously(numerate all of the paired bands)
             var band = bands.FirstOrDefault(); //Get the first band


             if (band == null) //If no bands found print error and bail out
             {
                 textBox.Text = "Band Noth Found";
                 return;
             }

             this.textBox.Text = "Connecting..."; //Set status message to connecting
             this.bandClient = await BandClientManager.Instance.ConnectAsync(band);
             //go to the singleton instance on the band manager and say connect and pass it the band
             this.textBox.Text = "Connected!"; //Now change status to connected


             bandClient.SensorManager.Accelerometer.ReadingChanged +=
                 async (sender1, args) => //Set up an async handler for the ReadingChanged event
                 {

                     await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                         () =>
                                 xVal = args.SensorReading.AccelerationX * 100);

                     await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                         () =>
                                 yVal = Math.Abs(args.SensorReading.AccelerationY * 1000));

                     await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                         () =>
                                 zVal = Math.Abs(args.SensorReading.AccelerationZ * 100));

     };
         }

        

        public void FillBufferSquare(short[] buffer, int sampleRate, double frequency)
        {
            // double ph2 = 2*Math.PI/(sampleRate * 40);
            //double curPh2 = 0.0;
            double fr2 = yVal;

            int totalTime = 0;

            ADSR adsr = new ADSR();

            double enval = 0;

            for (int i = 0; i < buffer.Length - 1; i += 2)
            {
                double time = (double)totalTime / (double)sampleRate;
                enval = adsr.envGenNew(totalTime, 3.3, 5.5, 8.8, 0.03, sampleRate);
                double t = frequency * time + sampleRate;
                // short currentSample = (short)(Math.Sin(2 * Math.PI * frequency * time) * Math.Sin(fr2 * time) * (double)short.MaxValue); // ampli modu
                short currentSample = (short)((double)enval * (Math.Sin((2 * Math.PI * frequency * time) + 2.0 * Math.Sin(fr2 * time))) * (double)short.MaxValue);// freq modulation
                buffer[i] = currentSample; //(short)(currentSample & 0xFF);
                buffer[i + 1] = currentSample; //(short)(currentSample >> 8);

                totalTime++; //= 2;
                             // curPh2 += ph2;
            }

        }

        public void FillBuffer(short[] buffer, int sampleRate, double frequency)
        {
            // double ph2 = 2*Math.PI/(sampleRate * 40);
            //double curPh2 = 0.0;
            double fr2 = yVal;

            int totalTime = 0;

            ADSR adsr = new ADSR();

            double enval = 0;

            for (int i = 0; i < buffer.Length - 1; i += 2)
            {
                double time = (double)totalTime / (double)sampleRate;
                enval = adsr.envGenNew(totalTime, 3.3, 5.5, 8.8, 0.03, sampleRate);
                // short currentSample = (short)(Math.Sin(2 * Math.PI * frequency * time) * Math.Sin(fr2 * time) * (double)short.MaxValue); // ampli modu
                short currentSample = (short)((double)enval * (Math.Sin((2 * Math.PI * frequency * time) + 2.0 * Math.Sin(fr2 * time))) * (double)short.MaxValue);// freq modulation
                buffer[i] = currentSample; //(short)(currentSample & 0xFF);
                buffer[i + 1] = currentSample; //(short)(currentSample >> 8);

                totalTime++; //= 2;
                             // curPh2 += ph2;
            }

        }

        public void FillBufferSaw(short[] buffer, int sampleRate, double frequency)
        {
            double totalTime = 0;
            double amp = 0;
            int K = (int)Math.Floor(sampleRate * 0.5 / frequency);

            for (int i = 0; i < buffer.Length - 1; i += 2)
            {
                double time = (double)totalTime / (double)sampleRate;
                double csamp = 0;
                for (int k = 1; k < K; k++)
                {
                    amp = Math.Pow(k, -1);
                    amp = amp / Math.PI;
                    csamp += amp * (Math.Sin(2 * Math.PI * k * frequency * time) * (double)short.MaxValue);
                    //short currentSample = (short)(Math.Sin(2*Math.PI*k*frequency*time)*(double) short.MaxValue);

                }

                short currentSample2 = (short)csamp;
                buffer[i] = currentSample2; //(short)(currentSample & 0xFF);
                buffer[i + 1] = currentSample2; //(short)(currentSample >> 8);
                totalTime++; //= 2;
            }

        }

        public void FillBufferTri(short[] buffer, int sampleRate, double frequency)
        {
            int totalTime = 0;

            ADSR adsr = new ADSR();

            double enval = 0;

            for (int i = 0; i < buffer.Length - 1; i += 2)
            {
                double time = (double)totalTime / (double)sampleRate;
                enval = adsr.envGenNew(totalTime, 3.3, 5.5, 8.8, 0.03, sampleRate);
                double t = frequency * time + sampleRate;
                short currentSample = (short)((double)enval * (1f - 4f * (float)Math.Abs(Math.Round(t - 0.25f) - (t - 0.25f))));

                buffer[i] = currentSample; //(short)(currentSample & 0xFF);
                buffer[i + 1] = currentSample; //(short)(currentSample >> 8);

                totalTime++;//= 2;
            }
        }

        /* public void sine()
         {
             // Initialization phase, keep this buffer during the life of your application
             // Allocate 10s at 44.1Khz of stereo 16bit signals
             var myBufferOfSamples = new short[44100 * 10 * 2];

             // Create a DataStream with pinned managed buffer
             var dataStream = DataStream.Create(myBufferOfSamples, true, true);

             var buffer = new AudioBuffer
             {
                 Stream = dataStream,
                 AudioBytes = (int)dataStream.Length,
                 Flags = BufferFlags.EndOfStream
             };

             // Fill myBufferOfSamples
             FillBuffer(myBufferOfSamples, 1000, 500);

         // PCM 44.1Khz stereo 16 bit format
         var waveFormat = new WaveFormat();

             XAudio2 xaudio = new XAudio2();
             xaudio.StartEngine();
             MasteringVoice masteringVoice = new MasteringVoice(xaudio);
             masteringVoice.SetVolume(1);
             var sourceVoice = new SourceVoice(xaudio, waveFormat, true);


             // Submit the buffer
             sourceVoice.SubmitSourceBuffer(buffer, null);
             sourceVoice.Start();
         } */

        /* private void button_Click(object sender, RoutedEventArgs e)
         {
             // Initialization phase, keep this buffer during the life of your application
             // Allocate 10s at 44.1Khz of stereo 16bit signals
             var myBufferOfSamples = new short[44100 * 10 * 2];

             // Create a DataStream with pinned managed buffer
             var dataStream = DataStream.Create(myBufferOfSamples, true, true);

             var buffer = new AudioBuffer
             {
                 Stream = dataStream,
                 AudioBytes = (int)dataStream.Length,
                 Flags = BufferFlags.EndOfStream
             };

             // Fill myBufferOfSamples
             FillBuffer(myBufferOfSamples, 500, 1000);

             // PCM 44.1Khz stereo 16 bit format
             var waveFormat = new WaveFormat();

             XAudio2 xaudio = new XAudio2();
             xaudio.StartEngine();
             MasteringVoice masteringVoice = new MasteringVoice(xaudio);
             masteringVoice.SetVolume(1);
             var sourceVoice = new SourceVoice(xaudio, waveFormat, true);


             // Submit the buffer
             sourceVoice.SubmitSourceBuffer(buffer, null);
             sourceVoice.Start();
         }*/

        private void toggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // Initialization phase, keep this buffer during the life of your application
            // Allocate 10s at 44.1Khz of stereo 16bit signals
            var myBufferOfSamples = new short[44100 * 10 * 2];

            // Create a DataStream with pinned managed buffer
            var dataStream = DataStream.Create(myBufferOfSamples, true, true);

            var buffer = new AudioBuffer
            {
                Stream = dataStream,
                AudioBytes = (int)dataStream.Length,
                Flags = BufferFlags.EndOfStream
            };

            // Fill myBufferOfSamples
            FillBuffer(myBufferOfSamples, 44100, 1000);

            // PCM 44.1Khz stereo 16 bit format
            var waveFormat = new WaveFormat();

            XAudio2 xaudio = new XAudio2();
            xaudio.StartEngine();
            MasteringVoice masteringVoice = new MasteringVoice(xaudio);
            masteringVoice.SetVolume(1);
            var sourceVoice = new SourceVoice(xaudio, waveFormat, true);


            // Submit the buffer
            sourceVoice.SubmitSourceBuffer(buffer, null);
            sourceVoice.Start();
        }

        private void toggleButton1_Checked(object sender, RoutedEventArgs e)
        {
            // Initialization phase, keep this buffer during the life of your application
            // Allocate 10s at 44.1Khz of stereo 16bit signals
            var myBufferOfSamples1 = new short[44100 * 10 * 2];

            // Create a DataStream with pinned managed buffer
            var dataStream = DataStream.Create(myBufferOfSamples1, true, true);

            var buffer = new AudioBuffer
            {
                Stream = dataStream,
                AudioBytes = (int)dataStream.Length,
                Flags = BufferFlags.EndOfStream
            };

            // Fill myBufferOfSamples
            FillBufferSaw(myBufferOfSamples1, 44100, 1000);

            // PCM 44.1Khz stereo 16 bit format
            var waveFormat = new WaveFormat();

            XAudio2 xaudio = new XAudio2();
            xaudio.StartEngine();
            MasteringVoice masteringVoice = new MasteringVoice(xaudio);
            masteringVoice.SetVolume(1);
            var sourceVoice = new SourceVoice(xaudio, waveFormat, true);


            // Submit the buffer
            sourceVoice.SubmitSourceBuffer(buffer, null);
            sourceVoice.Start();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Uri("/Xylophone.xaml", UriKind.Relative));
            this.Frame.Navigate(typeof(BlankPage1));
        }

        private void toggleButton2_Checked(object sender, RoutedEventArgs e)
        {
            // Initialization phase, keep this buffer during the life of your application
            // Allocate 10s at 44.1Khz of stereo 16bit signals
            var myBufferOfSamples1 = new short[44100 * 10 * 2];

            // Create a DataStream with pinned managed buffer
            var dataStream = DataStream.Create(myBufferOfSamples1, true, true);

            var buffer = new AudioBuffer
            {
                Stream = dataStream,
                AudioBytes = (int)dataStream.Length,
                Flags = BufferFlags.EndOfStream
            };

            // Fill myBufferOfSamples
            FillBufferTri(myBufferOfSamples1, 44100, 1000);

            // PCM 44.1Khz stereo 16 bit format
            var waveFormat = new WaveFormat();

            XAudio2 xaudio = new XAudio2();
            xaudio.StartEngine();
            MasteringVoice masteringVoice = new MasteringVoice(xaudio);
            masteringVoice.SetVolume(1);
            var sourceVoice = new SourceVoice(xaudio, waveFormat, true);


            // Submit the buffer
            sourceVoice.SubmitSourceBuffer(buffer, null);
            sourceVoice.Start();
        }

        private void toggleButton3_Checked(object sender, RoutedEventArgs e)
        {
            // Initialization phase, keep this buffer during the life of your application
            // Allocate 10s at 44.1Khz of stereo 16bit signals
            var myBufferOfSamples1 = new short[44100 * 10 * 2];

            // Create a DataStream with pinned managed buffer
            var dataStream = DataStream.Create(myBufferOfSamples1, true, true);

            var buffer = new AudioBuffer
            {
                Stream = dataStream,
                AudioBytes = (int)dataStream.Length,
                Flags = BufferFlags.EndOfStream
            };

            // Fill myBufferOfSamples
            FillBufferSquare(myBufferOfSamples1, 44100, 1000);

            // PCM 44.1Khz stereo 16 bit format
            var waveFormat = new WaveFormat();

            XAudio2 xaudio = new XAudio2();
            xaudio.StartEngine();
            MasteringVoice masteringVoice = new MasteringVoice(xaudio);
            masteringVoice.SetVolume(1);
            var sourceVoice = new SourceVoice(xaudio, waveFormat, true);


            // Submit the buffer
            sourceVoice.SubmitSourceBuffer(buffer, null);
            sourceVoice.Start();
        }




        /* private void toggleButton1_Checked(object sender, RoutedEventArgs e)
         {
             // Initialization phase, keep this buffer during the life of your application
             // Allocate 10s at 44.1Khz of stereo 16bit signals
             var myBufferOfSamples = new short[44100 * 10 * 2];

             // Create a DataStream with pinned managed buffer
             var dataStream = DataStream.Create(myBufferOfSamples, true, true);

             var buffer = new AudioBuffer
             {
                 Stream = dataStream,
                 AudioBytes = (int)dataStream.Length,
                 Flags = BufferFlags.EndOfStream
             };

             // Fill myBufferOfSamples
             FillBufferSaw(myBufferOfSamples, 44100, 3000);

             // PCM 44.1Khz stereo 16 bit format
             var waveFormat = new WaveFormat();

             XAudio2 xaudio = new XAudio2();
             xaudio.StartEngine();
             MasteringVoice masteringVoice = new MasteringVoice(xaudio);
             masteringVoice.SetVolume(1);
             var sourceVoice = new SourceVoice(xaudio, waveFormat, true);


             // Submit the buffer
             sourceVoice.SubmitSourceBuffer(buffer, null);
             sourceVoice.Start();
         }*/
    }
 
    }

