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
            VolumeIcon = "M301.1 34.8C312.6 40 320 51.4 320 64V448c0 12.6-7.4 24-18.9 29.2s-25 3.1-34.4-5.3L131.8 352H64c-35.3 0-64-28.7-64-64V224c0-35.3 28.7-64 64-64h67.8L266.7 40.1c9.4-8.4 22.9-10.4 34.4-5.3zM412.6 181.5C434.1 199.1 448 225.9 448 256s-13.9 56.9-35.4 74.5c-10.3 8.4-25.4 6.8-33.8-3.5s-6.8-25.4 3.5-33.8C393.1 284.4 400 271 400 256s-6.9-28.4-17.7-37.3c-10.3-8.4-11.8-23.5-3.5-33.8s23.5-11.8 33.8-3.5z";
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
                VolumeIcon = "M301.1 34.8C312.6 40 320 51.4 320 64V448c0 12.6-7.4 24-18.9 29.2s-25 3.1-34.4-5.3L131.8 352H64c-35.3 0-64-28.7-64-64V224c0-35.3 28.7-64 64-64h67.8L266.7 40.1c9.4-8.4 22.9-10.4 34.4-5.3zM425 167l55 55 55-55c9.4-9.4 24.6-9.4 33.9 0s9.4 24.6 0 33.9l-55 55 55 55c9.4 9.4 9.4 24.6 0 33.9s-24.6 9.4-33.9 0l-55-55-55 55c-9.4 9.4-24.6 9.4-33.9 0s-9.4-24.6 0-33.9l55-55-55-55c-9.4-9.4-9.4-24.6 0-33.9s24.6-9.4 33.9 0z";
            }
            else if (VolumeValue > 0 && VolumeValue <= 50)
            {
                VolumeIcon = "M301.1 34.8C312.6 40 320 51.4 320 64V448c0 12.6-7.4 24-18.9 29.2s-25 3.1-34.4-5.3L131.8 352H64c-35.3 0-64-28.7-64-64V224c0-35.3 28.7-64 64-64h67.8L266.7 40.1c9.4-8.4 22.9-10.4 34.4-5.3zM412.6 181.5C434.1 199.1 448 225.9 448 256s-13.9 56.9-35.4 74.5c-10.3 8.4-25.4 6.8-33.8-3.5s-6.8-25.4 3.5-33.8C393.1 284.4 400 271 400 256s-6.9-28.4-17.7-37.3c-10.3-8.4-11.8-23.5-3.5-33.8s23.5-11.8 33.8-3.5z";
            }
            else if (VolumeValue > 50) 
            {
                VolumeIcon = "M533.6 32.5C598.5 85.3 640 165.8 640 256s-41.5 170.8-106.4 223.5c-10.3 8.4-25.4 6.8-33.8-3.5s-6.8-25.4 3.5-33.8C557.5 398.2 592 331.2 592 256s-34.5-142.2-88.7-186.3c-10.3-8.4-11.8-23.5-3.5-33.8s23.5-11.8 33.8-3.5zM473.1 107c43.2 35.2 70.9 88.9 70.9 149s-27.7 113.8-70.9 149c-10.3 8.4-25.4 6.8-33.8-3.5s-6.8-25.4 3.5-33.8C475.3 341.3 496 301.1 496 256s-20.7-85.3-53.2-111.8c-10.3-8.4-11.8-23.5-3.5-33.8s23.5-11.8 33.8-3.5zm-60.5 74.5C434.1 199.1 448 225.9 448 256s-13.9 56.9-35.4 74.5c-10.3 8.4-25.4 6.8-33.8-3.5s-6.8-25.4 3.5-33.8C393.1 284.4 400 271 400 256s-6.9-28.4-17.7-37.3c-10.3-8.4-11.8-23.5-3.5-33.8s23.5-11.8 33.8-3.5zM301.1 34.8C312.6 40 320 51.4 320 64V448c0 12.6-7.4 24-18.9 29.2s-25 3.1-34.4-5.3L131.8 352H64c-35.3 0-64-28.7-64-64V224c0-35.3 28.7-64 64-64h67.8L266.7 40.1c9.4-8.4 22.9-10.4 34.4-5.3z";
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
                VolumeIcon = "M301.1 34.8C312.6 40 320 51.4 320 64V448c0 12.6-7.4 24-18.9 29.2s-25 3.1-34.4-5.3L131.8 352H64c-35.3 0-64-28.7-64-64V224c0-35.3 28.7-64 64-64h67.8L266.7 40.1c9.4-8.4 22.9-10.4 34.4-5.3zM425 167l55 55 55-55c9.4-9.4 24.6-9.4 33.9 0s9.4 24.6 0 33.9l-55 55 55 55c9.4 9.4 9.4 24.6 0 33.9s-24.6 9.4-33.9 0l-55-55-55 55c-9.4 9.4-24.6 9.4-33.9 0s-9.4-24.6 0-33.9l55-55-55-55c-9.4-9.4-9.4-24.6 0-33.9s24.6-9.4 33.9 0z";
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
