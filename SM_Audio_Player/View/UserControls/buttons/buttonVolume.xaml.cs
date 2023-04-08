using Newtonsoft.Json;
using SM_Audio_Player.Music;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SM_Audio_Player.View.UserControls.buttons
{
    public partial class buttonVolume : UserControl, INotifyPropertyChanged
    {
        bool isMuted = false;
        private double lastVolumeValue = 100;
        private const string SettingsFileName = "SettingsVolume.json";

        public buttonVolume()
        {
            DataContext = this;
            VolumeIcon = Icons.GetVolumeIconHalf();
            InitializeComponent();
            readFromJson();
            sldVolume.Value = lastVolumeValue;
            buttonNext.NextButtonClicked += OnTrackSwitch;
            buttonPrevious.PreviousButtonClicked += OnTrackSwitch;
            buttonPlay.TrackEnd += OnTrackSwitch;
            Library.DoubleClickEvent += OnTrackSwitch;
        }

        private string volumeIcon;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string VolumeIcon
        {
            get { return volumeIcon; }
            set 
            { 
                volumeIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VolumeIcon"));
            }
        }

        /*Metoda sprawdzająca aktualną wartość slidera i na jej podstawie ustawiająca ikonkę*/
        private void valueIconChange(double VolumeValue)
        {
            if (VolumeValue == 0) 
            {
                VolumeIcon = Icons.GetVolumeIconZero();
            }
            else if (VolumeValue > 0 && VolumeValue <= 50)
            {
                VolumeIcon = Icons.GetVolumeIconHalf();
            }
            else if (VolumeValue > 50) 
            {
                VolumeIcon = Icons.GetVolumeIconMax();
            }
        }

        /*Wycisz/Zmień poziom głośności*/
        private void btnVolume_Click(object sender, RoutedEventArgs e)
        {
            if (isMuted && lastVolumeValue != 0)
            {
                sldVolume.Value = lastVolumeValue;
                valueIconChange(lastVolumeValue);
                isMuted = false;
            }
            else
            {
                lastVolumeValue = sldVolume.Value;
                sldVolume.Value = 0;
                VolumeIcon = Icons.GetVolumeIconZero();
                isMuted = true;
            }
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                /*Pobieranie aktualnej wartości slidera*/
                double currentValue = e.NewValue;
                valueIconChange(currentValue);

                if (TracksProperties.audioFileReader != null)
                {
                    double sliderValue = sldVolume.Value / 100.0; // Skalowanie wartości na zakres od 0 do 1
                    double newVolume = sliderValue; // Obliczamy nową wartość głośności
                    TracksProperties.audioFileReader.Volume = (float)newVolume; // Aktualizujemy głośność pliku audio
                    writeToJson();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Volume change error: {ex.Message}");
            }
        }

        private void readFromJson()
        {
            // Odczytanie wartości suwaka z pliku
            try
            {
                if (File.Exists(SettingsFileName))
                {
                    string settingsJson = File.ReadAllText(SettingsFileName);
                    var settings = JsonConvert.DeserializeAnonymousType(settingsJson, new { LastVolumeValue = 0.0 });
                    lastVolumeValue = settings.LastVolumeValue;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd odczytu pliku: {ex.Message}");
            }
        }

        private void writeToJson()
        {
            // Zapisanie wartości suwaka do pliku
            try
            {
                // Zapisanie wartości suwaka do pliku
                var settings = new { LastVolumeValue = sldVolume.Value };
                string settingsJson = JsonConvert.SerializeObject(settings);
                File.WriteAllText(SettingsFileName, settingsJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd zapisu pliku: {ex.Message}");
            }
        }
        // Aktualizacja głośności po zmienionym tracku
        private void OnTrackSwitch(object sender, EventArgs e)
        {
            try
            {
                valueIconChange(sldVolume.Value);
                if (TracksProperties.audioFileReader != null)
                {
                    double sliderValue = sldVolume.Value / 100.0; // Skalowanie wartości na zakres od 0 do 1
                    double newVolume = sliderValue; // Obliczamy nową wartość głośności
                    TracksProperties.audioFileReader.Volume = (float)newVolume; // Aktualizujemy głośność pliku audio
                    writeToJson();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Volume change error: {ex.Message}");
            }
        }
    }
}
