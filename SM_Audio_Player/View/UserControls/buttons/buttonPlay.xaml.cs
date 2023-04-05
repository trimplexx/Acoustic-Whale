using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
using NAudio.Wave;
using Newtonsoft.Json;
using SM_Audio_Player.Music;
using SM_Audio_Player.View.UserControls;

namespace SM_Audio_Player.View.UserControls.buttons
{
    public partial class buttonPlay : UserControl
    {
        private WaveOut waveOut = new WaveOut();
        private AudioFileReader audioFileReader;
        private bool isPlaying = false;

        public buttonPlay()
        {
            InitializeComponent();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewSelectedItemModel.selectedTrack != null)
                {
                    if (!isPlaying)
                    {
                        audioFileReader = new AudioFileReader(listViewSelectedItemModel.selectedTrack.Path);
                        waveOut.Init(audioFileReader);
                        waveOut.Play();
                        isPlaying = true;
                    }
                    else
                    {
                        waveOut.Pause();
                        isPlaying = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Play/Pause music error");
            }
        }
    }
}
