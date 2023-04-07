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
    public partial class buttonNext : UserControl
    {
        public buttonNext()
        {
            InitializeComponent();
        }

        /*Włącz następny utwór*/
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (TracksProperties.tracksList.Count > 1)
            {
                int currentIndex = TracksProperties.tracksList.IndexOf(TracksProperties.SelectedTrack);
                int nextIndex = (currentIndex + 1) % TracksProperties.tracksList.Count;
                TracksProperties.SelectedTrack = TracksProperties.tracksList[nextIndex];
                TracksProperties.waveOut.Dispose();
                TracksProperties.audioFileReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                TracksProperties.waveOut.Init(TracksProperties.audioFileReader);
                TracksProperties.waveOut.Play();
            }
        }
    }
}
