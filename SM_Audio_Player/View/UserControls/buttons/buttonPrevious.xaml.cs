using NAudio.Wave;
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
    public partial class buttonPrevious : UserControl
    {
        private buttonPlay btnPlay = new buttonPlay();
        public buttonPrevious()
        {
            InitializeComponent();
        }

        /*Włącz pooprzedni utwór*/
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Sprawdzanie czy to pierwszy utwór na liście, jeżeli tak odtworzony zostanie od nowa.
                if (TracksProperties.SelectedTrack.Id == 1)
                    btnPlay.PlayNewTrack();
                else
                {
                    // W innym wypadku zostanie odtworzony poprzedni utwór.
                    TracksProperties.SelectedTrack = TracksProperties.tracksList.ElementAt(TracksProperties.SelectedTrack.Id-2);
                    btnPlay.PlayNewTrack();
                }
                // Przypisanie eventu, aby następny utwór po skończeniu przewiniętego został automatycznie odtworzony.
                TracksProperties.waveOut.PlaybackStopped += btnPlay.WaveOut_PlaybackStopped;
            }
                catch (Exception ex)
            {
                MessageBox.Show($"Previous button error!");
            }
        }
    }
}
