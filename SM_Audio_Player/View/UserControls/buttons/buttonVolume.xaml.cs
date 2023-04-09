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
        private double savedVolumeValue = 0;
        private string volumeIcon;
        public event PropertyChangedEventHandler? PropertyChanged;

        public buttonVolume()
        {
            DataContext = this;
            InitializeComponent();
            sldVolume.Value = TracksProperties.Volume;
            VolumeIcon = valueIconChange(sldVolume.Value);
            buttonNext.NextButtonClicked += OnTrackSwitch;
            buttonPrevious.PreviousButtonClicked += OnTrackSwitch;
            buttonPlay.TrackEnd += OnTrackSwitch;
            Library.DoubleClickEvent += OnTrackSwitch;
        }

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
        private string valueIconChange(double VolumeValue)
        {
            if (VolumeValue == 0) 
            {
                VolumeIcon = Icons.GetVolumeIconZero();
                return Icons.GetVolumeIconZero();
            }
            else if (VolumeValue > 0 && VolumeValue <= 40)
            {
                VolumeIcon = Icons.GetVolumeIconLow();
                return Icons.GetVolumeIconLow();
            }
            else if (VolumeValue > 40 && VolumeValue <= 75) 
            {
                VolumeIcon = Icons.GetVolumeIconHalf();
                return Icons.GetVolumeIconHalf();
            }
            else if (VolumeValue > 75)
            {
                VolumeIcon = Icons.GetVolumeIconMax();
                return Icons.GetVolumeIconMax();
            }
            return Icons.GetVolumeIconHalf();
        }

        /*Wycisz/Zmień poziom głośności*/
        private void btnVolume_Click(object sender, RoutedEventArgs e)
        {
            if (isMuted && savedVolumeValue != 0)
            {
                sldVolume.Value = savedVolumeValue;
                valueIconChange(TracksProperties.Volume);
                isMuted = false;
            }
            else
            {
                savedVolumeValue = sldVolume.Value;
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
                TracksProperties.Volume = e.NewValue;
                valueIconChange(TracksProperties.Volume);

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
        
        // Aktualizacja głośności po zmienionym tracku
        private void OnTrackSwitch(object sender, EventArgs e)
        {
            try
            {
                valueIconChange(TracksProperties.Volume);

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
