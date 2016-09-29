using Windows.UI.Xaml;

namespace WavePlayerDX
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private WavePlayer _wavePlayer = new WavePlayer();

        void MainPage_Loaded(object sender, RoutedEventArgs e) {
            _wavePlayer.AddWave("spacy", "spacy.wav");
            _wavePlayer.AddWave("tiles", "tiles.wav");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            _wavePlayer.PlayWave("spacy");
        }
        private void Button_Click_2(object sender, RoutedEventArgs e) {
            _wavePlayer.PlayWave("tiles");
        }
    }
}
