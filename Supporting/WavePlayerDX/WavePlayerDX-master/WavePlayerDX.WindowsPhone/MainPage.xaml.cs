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

namespace WavePlayerDX
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
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

            bool hrConsentGraned;

            switch (bandClient.SensorManager.Accelerometer.GetCurrentUserConsent()) //Go to the sensormanager under the bandclient, go to the heartrate sensor specifically and check the current consent setting
            {
                case UserConsent.Declined:
                    hrConsentGraned = false;
                    break;

                case UserConsent.Granted:
                    hrConsentGraned = true;
                    break;

                default:
                case UserConsent.NotSpecified:
                    hrConsentGraned = await bandClient.SensorManager.Accelerometer.RequestUserConsentAsync();
                    break;
            }

            if (hrConsentGraned) //If the user gives consent we want to set up an event handler
            {
                bandClient.SensorManager.Accelerometer.ReadingChanged +=
                    async (sender, args) => //Set up an async handler for the ReadingChanged event
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                                this.textBox1.Text =
                                    string.Format("{0} ({1})",
                                        //Go back to the main thread and format the HR text field using both the qualityand the heart rate number below
                                        args.SensorReading.AccelerationX * 100, args.SensorReading.AccelerationX * 100));

                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                                this.textBox2.Text =
                                    string.Format("{0} ({1})",
                                        //Go back to the main thread and format the HR text field using both the qualityand the heart rate number below
                                        args.SensorReading.AccelerationY * 100, args.SensorReading.AccelerationY * 100));

                        if (args.SensorReading.AccelerationY * 100 <= 50)
                        {
                            _wavePlayer.PlayWave("spacy");
                            
                        }

                        if (args.SensorReading.AccelerationY * 100 >= 50)
                        {
                            _wavePlayer.PlayWave("tiles");

                        }




                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                                this.textBox3.Text =
                                    string.Format("{0} ({1})",
                                        //Go back to the main thread and format the HR text field using both the qualityand the heart rate number below
                                        args.SensorReading.AccelerationZ * 100, args.SensorReading.AccelerationZ * 100));

                        /*await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                /* SoundEffect soundEffect;
                                 soundEffect = Content.Load<SoundEffect>("kaboom");
                                 soundEffect.Play();*/
                                //Kick.Play();


                            //});

                    };
                //Go back to the main thread and format the HR text field using both the qualityand the heart rate number below

            }
            else   //else if we dont get consent print below to te text field
                this.textBox.Text = "No consent for HR";

        }

        private WavePlayer _wavePlayer = new WavePlayer();

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _wavePlayer.AddWave("spacy", "spacy.wav");
            _wavePlayer.AddWave("tiles", "tiles.wav");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _wavePlayer.PlayWave("spacy");
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _wavePlayer.PlayWave("tiles");
        }

        //await client.SensorManager.Accelerometer.StartReadingsAsync();
        //client.SensorManager.Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
        void Accelerometer_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandAccelerometerReading> e)
        {
            var xAxis = e.SensorReading.AccelerationX * 100;
            var yAxis = e.SensorReading.AccelerationY * 100;
            var zAxis = e.SensorReading.AccelerationZ * 100;

            if (yAxis <= 50)
            {
                _wavePlayer.PlayWave("spacy");
                
            }

            if (yAxis >= 50)
            {
                _wavePlayer.PlayWave("tiles");
            }

            if (yAxis <= 50)
            {
                _wavePlayer.PlayWave("tiles");
            }

        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            await bandClient.SensorManager.Accelerometer.StartReadingsAsync(); //when we start reading the heart rate go to the HR sensor and startreadingasync
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            await bandClient.SensorManager.Accelerometer.StopReadingsAsync();  //when we stop reading the heart rate go to the HR sensor and stopreadingasync
        }
    }
}