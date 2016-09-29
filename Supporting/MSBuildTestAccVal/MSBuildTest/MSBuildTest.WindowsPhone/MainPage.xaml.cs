using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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

using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

using Microsoft.Band;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MSBuildTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private IBandClient bandClient; //Portal into the functionality of the band


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
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
                txtStatus.Text = "Band Noth Found";
                return;
            }

            this.txtStatus.Text = "Connecting..."; //Set status message to connecting
            this.bandClient = await BandClientManager.Instance.ConnectAsync(band);
            //go to the singleton instance on the band manager and say connect and pass it the band
            this.txtStatus.Text = "Connected!"; //Now change status to connected

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
                                this.TxtHr.Text =
                                    string.Format("{0} ({1})",
                                        //Go back to the main thread and format the HR text field using both the qualityand the heart rate number below
                                        args.SensorReading.AccelerationX, args.SensorReading.AccelerationX));

                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                                this.textBox.Text =
                                    string.Format("{0} ({1})",
                                        //Go back to the main thread and format the HR text field using both the qualityand the heart rate number below
                                        args.SensorReading.AccelerationY, args.SensorReading.AccelerationY));

                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                             () =>
                                this.textBox1.Text =
                                    string.Format("{0} ({1})",
                                        //Go back to the main thread and format the HR text field using both the qualityand the heart rate number below
                                        args.SensorReading.AccelerationZ, args.SensorReading.AccelerationZ));


                    };
            }
            else   //else if we dont get consent print below to te text field
                this.txtStatus.Text = "No consent for HR";

        }

        //Hndlers for the buttons
        private async void btnStartHr_Click(object sender, RoutedEventArgs e)
        {
            await bandClient.SensorManager.Accelerometer.StartReadingsAsync(); //when we start reading the heart rate go to the HR sensor and startreadingasync
        }

        private async void btnStopHR_Click(object sender, RoutedEventArgs e)
        {
            await bandClient.SensorManager.Accelerometer.StopReadingsAsync();  //when we stop reading the heart rate go to the HR sensor and stopreadingasync
        }

        

        private Task<BandIcon> LoadIcon(string msAppxAssetsSampletileiconlargePng)
        {
            throw new NotImplementedException();
        }


        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void textBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {

        }

        private void textBlock1_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}

