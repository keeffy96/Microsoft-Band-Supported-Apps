using System;
using Windows.UI.Xaml;

using Microsoft.Band;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;

using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Band.Sensors;
using Windows.Graphics.Display;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace App13
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Xylophone : Page
    {
        public Xylophone()
        {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            Loaded += Xylophone_Loaded;
        }
            double xVal;
            double yVal;
            double zVal;

        private IBandClient bandClient; //Portal into the functionality of the band

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.New)
                //in this case all of the initialization isdone on the first load of the page
                return;

            var bands = await BandClientManager.Instance.GetBandsAsync();
            //Going to the client manager, getting the singleton instance, get the bands, do this asynchronously(numerate all of the paired bands)
            var band = bands.FirstOrDefault(); //Get the first band
            
            bandClient.SensorManager.Accelerometer.ReadingChanged +=
                async (sender1, args) => //Set up an async handler for the ReadingChanged event
                 {

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                                xVal = args.SensorReading.AccelerationX * 100);

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                                yVal = Math.Abs(args.SensorReading.AccelerationY * 100));

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                                zVal = Math.Abs(args.SensorReading.AccelerationZ * 100));

                     while (args.SensorReading.AccelerationY * 100 < 10 && args.SensorReading.AccelerationY * 100 > 0 && args.SensorReading.AccelerationX * 100 < -15)
                     {
                         _wavePlayer.PlayWave("C3");
                     }
                     while (args.SensorReading.AccelerationY * 100 < 20 && args.SensorReading.AccelerationY * 100 > 10 && args.SensorReading.AccelerationX * 100 < -15)
                     {
                         _wavePlayer.PlayWave("B3");
                     }
                     while (args.SensorReading.AccelerationY * 100 < 30 && args.SensorReading.AccelerationY * 100 > 20 && args.SensorReading.AccelerationX * 100 < -15)
                     {
                         _wavePlayer.PlayWave("A3");
                     }
                     while (args.SensorReading.AccelerationY * 100 < 40 && args.SensorReading.AccelerationY * 100 > 30 && args.SensorReading.AccelerationX * 100 < -15)
                     {
                         _wavePlayer.PlayWave("G3");
                     }
                     while (args.SensorReading.AccelerationY * 100 < 50 && args.SensorReading.AccelerationY * 100 > 40 && args.SensorReading.AccelerationX * 100 < -15)
                     {
                         _wavePlayer.PlayWave("G3");
                     }
                     while (args.SensorReading.AccelerationY * 100 < 60 && args.SensorReading.AccelerationY * 100 > 50 && args.SensorReading.AccelerationX * 100 < -15)
                     {
                         _wavePlayer.PlayWave("F3");
                     }
                     while (args.SensorReading.AccelerationY * 100 < 70 && args.SensorReading.AccelerationY * 100 > 60 && args.SensorReading.AccelerationX * 100 < -15)
                     {
                         _wavePlayer.PlayWave("E3");
                     }
                 };
        }
        private WavePlayer _wavePlayer = new WavePlayer();

        public void Xylophone_Loaded(object sender, RoutedEventArgs e)
        {
            _wavePlayer.AddWave("C3", "C3.wav");
            _wavePlayer.AddWave("B3", "B3.wav");
            _wavePlayer.AddWave("A3", "A3.wav");
            _wavePlayer.AddWave("G3", "G3.wav");
            _wavePlayer.AddWave("F3", "F3.wav");
            _wavePlayer.AddWave("E3", "E3.wav");
            _wavePlayer.AddWave("D3", "D3.wav");
            // _wavePlayer.AddWave("C3", "C3.wav");
            //_wavePlayer.AddWave("tiles", "tiles.wav");
        }
    

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
       //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //}

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _wavePlayer.PlayWave("C3");
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            _wavePlayer.PlayWave("B3");
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            _wavePlayer.PlayWave("A3");
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            _wavePlayer.PlayWave("G3");
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            _wavePlayer.PlayWave("F3");
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            _wavePlayer.PlayWave("E3");
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            _wavePlayer.PlayWave("D3");
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            _wavePlayer.PlayWave("C3");
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BlankPage1));
        }
    }
}


