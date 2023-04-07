using SM_Audio_Player.Music;
using System;
using System.Collections.Generic;
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
    public partial class buttonVolume : UserControl
    {
        bool isMuted = false;
        private double lastVolumeValue = 100;
        public buttonVolume()
        {
            InitializeComponent();
            sldVolume.Value = lastVolumeValue;
        }

        /*Wycisz/Zmień poziom głośności*/
        private void btnVolume_Click(object sender, RoutedEventArgs e)
        {
            if (isMuted)
            {
                sldVolume.Value = lastVolumeValue;
                isMuted = false;
            }
            else
            {
                lastVolumeValue = sldVolume.Value;
                sldVolume.Value = 0;
                isMuted = true;
            }
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (TracksProperties.audioFileReader != null)
                {
                    double sliderValue = sldVolume.Value / 100.0; // Skalowanie wartości na zakres od 0 do 1
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
