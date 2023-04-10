using Newtonsoft.Json;
using SM_Audio_Player.Music;
using System;

using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SM_Audio_Player.View.UserControls.buttons
{
    public partial class buttonVolume : INotifyPropertyChanged
    {
        bool _isMuted;
        private double _savedVolumeValue;
        private string _volumeIcon;
        
        private const String JsonPath = @"MusicVolumeJSON.json";
        public event PropertyChangedEventHandler? PropertyChanged;

        public buttonVolume()
        {
            DataContext = this;
            InitializeComponent();           
            if (File.Exists(JsonPath))
                TracksProperties.Volume = ReadVolumeFromJsonFile();
            else
                TracksProperties.Volume = 50;

            sldVolume.Value = TracksProperties.Volume;
            VolumeIcon = ValueIconChange(sldVolume.Value);
            buttonNext.NextButtonClicked += OnTrackSwitch;
            buttonPrevious.PreviousButtonClicked += OnTrackSwitch;
            buttonPlay.TrackEnd += OnTrackSwitch;
            Library.DoubleClickEvent += OnTrackSwitch;
        }

        public string VolumeIcon
        {
            get { return _volumeIcon; }
            set 
            { 
                _volumeIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VolumeIcon"));
            }
        }

        /*Metoda sprawdzająca aktualną wartość slidera i na jej podstawie ustawiająca ikonkę*/
        private string ValueIconChange(double volumeValue)
        {
            if (volumeValue == 0) 
            {
                VolumeIcon = Icons.GetVolumeIconZero();
                return Icons.GetVolumeIconZero();
            }
            else if (volumeValue > 0 && volumeValue <= 40)
            {
                VolumeIcon = Icons.GetVolumeIconLow();
                return Icons.GetVolumeIconLow();
            }
            else if (volumeValue > 40 && volumeValue <= 75) 
            {
                VolumeIcon = Icons.GetVolumeIconHalf();
                return Icons.GetVolumeIconHalf();
            }
            else if (volumeValue > 75)
            {
                VolumeIcon = Icons.GetVolumeIconMax();
                return Icons.GetVolumeIconMax();
            }
            return Icons.GetVolumeIconHalf();
        }

        /*Wycisz/Zmień poziom głośności*/
        private void btnVolume_Click(object sender, RoutedEventArgs e)
        {
            if (_isMuted && _savedVolumeValue != 0)
            {
                sldVolume.Value = _savedVolumeValue;
                ValueIconChange(TracksProperties.Volume);
                _isMuted = false;
            }
            else
            {
                _savedVolumeValue = sldVolume.Value;
                sldVolume.Value = 0;
                VolumeIcon = Icons.GetVolumeIconZero();
                _isMuted = true;
            }
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                /*Pobieranie aktualnej wartości slidera*/
                TracksProperties.Volume = e.NewValue;
                ValueIconChange(TracksProperties.Volume);

                if (TracksProperties.audioFileReader != null)
                {
                    double sliderValue = TracksProperties.Volume / 100.0; // Skalowanie wartości na zakres od 0 do 1
                    double newVolume = sliderValue; // Obliczamy nową wartość głośności
                    TracksProperties.audioFileReader.Volume = (float)newVolume; // Aktualizujemy głośność pliku audio
                    
                    // Zapis do pliku JSON w celu ponownego odpalenia aplikacji z zapisaną wartością głośności
                    string output = JsonConvert.SerializeObject(TracksProperties.Volume, Formatting.Indented);
                    File.WriteAllText(JsonPath, output);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Volume change error: {ex.Message}");
            }
        }

        public double ReadVolumeFromJsonFile()
        {
            try
            {
                double volume = 50;
                string json = File.ReadAllText(JsonPath);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);
                if (jsonObj != null)
                    volume = jsonObj;
                return volume;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ReadValueJson error: {ex.Message}");
                return 0;
            }
        }
        
        // Aktualizacja głośności po zmienionym tracku
        private void OnTrackSwitch(object sender, EventArgs e)
        {
            try
            {
                ValueIconChange(TracksProperties.Volume);

                if (TracksProperties.audioFileReader != null)
                {
                    double sliderValue = TracksProperties.Volume / 100.0; // Skalowanie wartości na zakres od 0 do 1
                    double newVolume = sliderValue; // Obliczamy nową wartość głośności
                    TracksProperties.audioFileReader.Volume = (float)newVolume; // Aktualizujemy głośność pliku audio
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Volume change error: {ex.Message}");
            }
        }
    }
}
