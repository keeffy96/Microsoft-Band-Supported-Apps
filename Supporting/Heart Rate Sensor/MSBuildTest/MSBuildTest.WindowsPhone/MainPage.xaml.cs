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

            switch (this.bandClient.SensorManager.HeartRate.GetCurrentUserConsent()) //Go to the sensormanager under the bandclient, go to the heartrate sensor specifically and check the current consent setting
            {
                case UserConsent.Declined:
                    hrConsentGraned = false;
                    break;

                case UserConsent.Granted:
                    hrConsentGraned = true;
                    break;

                default:
                    case UserConsent.NotSpecified:
                    hrConsentGraned = await this.bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
                    break;
            }

            if (hrConsentGraned) //If the user gives consent we want to set up an event handler
            {
                bandClient.SensorManager.HeartRate.ReadingChanged +=
                    async (sender, args) => //Set up an async handler for the ReadingChanged event
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                                this.TxtHr.Text =
                                    string.Format("{0} ({1})",
                                        //Go back to the main thread and format the HR text field using both the qualityand the heart rate number below
                                        args.SensorReading.Quality, args.SensorReading.HeartRate));
                    };
            }
            else   //else if we dont get consent print below to te text field
                this.txtStatus.Text = "No consent for HR";

            /*await this.CreateOrInitializeTile();

            //Event Handlers
            //Tile Opened event
            this.bandClient.TileManager.TileOpened += async (sender, args) =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => this.txtStatus.Text = "Open Tile");   //putting a message into the status area
            };
            //Tile closed event
            this.bandClient.TileManager.TileClosed += async (sender, args) =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => this.txtStatus.Text = "Close Tile");
            };
            //Tile button pressed event
            this.bandClient.TileManager.TileButtonPressed += async (sender, args) =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => this.txtStatus.Text = string.Format("Button Pressed ({0})", args.TileEvent.ElementId));   //reach into our args and ge the element id so we can see which button we are pressing
            };

            await this.bandClient.TileManager.StartReadingsAsync();*/
        }

        //Hndlers for the buttons
        private async void btnStartHr_Click(object sender, RoutedEventArgs e)
        {
            await this.bandClient.SensorManager.HeartRate.StartReadingsAsync(); //when we start reading the heart rate go to the HR sensor and startreadingasync
        }

        private async void btnStopHR_Click(object sender, RoutedEventArgs e)
        {
            await this.bandClient.SensorManager.HeartRate.StopReadingsAsync();  //when we stop reading the heart rate go to the HR sensor and stopreadingasync
        }

        /*private async Task CreateOrInitializeTile()
        {
            Guid tileGuid = Guid.Parse("{857C016D-8F57-4A30-A714-0A8EEC9B51BD}");   //Wstablish the Guids for the tile and the page
            Guid pageGuid = Guid.Parse("{76D26A35-534F-4267-A8FA-CC2DC67821F8}");

            var tiles = await this.bandClient.TileManager.GetTilesAsync();   //go to the tile manager and get our current set of tiles

            BandTile tile = tiles.FirstOrDefault(); //call first or default

            if (tile == null)   //If we get null back that meanswe have no tile yet, so we go construct the tile and send it to the band    
            {
                var panel = new FlowPanel   //Make a flow panel to put the buttons inside of
                {
                    ElementId = 0,  //Give it an element is 0
                    Rect = new PageRect(0, 0, 245, 106),    //setting the rectangle to use the full usable area on the tile
                    Orientation = FlowPanelOrientation.Horizontal,  //setting the orientation to horizontal
                    HorizontalAlignment = Microsoft.Band.Tiles.Pages.HorizontalAlignment.Center,    //Center the orientation
                    VerticalAlignment = Microsoft.Band.Tiles.Pages.VerticalAlignment.Center
                };

                panel.Elements.Add( //putting elements within the flow panel we just created
                    new TextButton  //adding a text button
                    {
                       ElementId = 1,   //give it a new element id 1
                       Rect = new PageRect(0, 0, 100,50),   //set the width
                       PressedColor = Colors.Red.ToBandColor(), //set the colour when pressed
                       Margins = new Margins(0, 0, 20, 0),  
                       HorizontalAlignment = Microsoft.Band.Tiles.Pages.HorizontalAlignment.Center
                    }
                );

                panel.Elements.Add(
                    new TextButton  //Adding a second button
                    {
                        ElementId = 2,
                        Rect = new PageRect(0, 0, 100, 50),
                        PressedColor = Colors.Green.ToBandColor(),
                        HorizontalAlignment = Microsoft.Band.Tiles.Pages.HorizontalAlignment.Center
                    }
                );

                var layout = new PageLayout(panel); //Make the layout and set as the root object on the layout the flowpanel we made bove

                tile = new BandTile(tileGuid)   //make a tile called BandTile and pass it our tileGuid
                {
                    Name = "Build Demo",    //Give the tile a name,   important as can be seen in the 3rd party tiles in the health app  
                    TileIcon = await LoadIcon("ms-appx:///Assets/SampleTileIconLarge.png"), //Setting the two icons we want to use
                    SmallIcon = await LoadIcon("ms-appx:///Assets/SampleTileIconSmall.png")
                };
                //We now have our tile object made with its layout etc

                tile.PageLayouts.Add(layout);   //Adding the layout

                await bandClient.TileManager.AddTileAsync(tile);    //Go to the tile manager and say add tile
            }

            //Above was work that we wanted to do the very first time when the tile wasnt there yet
            //Now every time we run through this code we want to go set up that page and ptovide the page with its content, hence why in the layout no labels on buttons etc.  

            await this.bandClient.TileManager.SetPagesAsync(    //Go to the tile manager again and call SetPagesAsync   
               tileGuid,    //get the tileGuid
               new PageData(    //Make a pageData object
                   pageGuid,    //Give it the guid("Which page we are talking about")
                   0,   //This 0 says we want to use the first layout, in this case we only have one layout
                   new TextButtonData(1, "off"),//pass objects that provide the content for the elements, in this case the buttons(setting the labels)
                   new TextButtonData(2, "on"))
             );
        }

        private async Task<BandIcon> LoadIcon(string uri)
        {
            StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                WriteableBitmap bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return bitmap.ToBandIcon();
            }
        }*/

        private Task<BandIcon> LoadIcon(string msAppxAssetsSampletileiconlargePng)
        {
            throw new NotImplementedException();
        }


        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

