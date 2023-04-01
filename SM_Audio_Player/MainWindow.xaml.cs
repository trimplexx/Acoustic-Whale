using NAudio.Wave;
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

namespace SM_Audio_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int variable = 0;
        WaveOutEvent outputDevice = new WaveOutEvent(); // zdarzenie audio
        AudioFileReader audioFile = null; // sciezka pliku audio

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PlayAudio_Click(object sender, RoutedEventArgs e)
        {
            if (variable == 0)
            {
                MessageBox.Show("Po kliknieciu ok rozpoczynie się odtwarzanie pliku audio");
                if (audioFile == null)
                {
                    audioFile = new AudioFileReader(@"E:\test.mp3");
                    outputDevice.Init(audioFile);
                }
                outputDevice.Play();
                variable++;
            }
            else if (variable == 1)
            {
                MessageBox.Show("Po kliknieciu ok zatrzynasz odtwarzanie pliku audio");
                outputDevice?.Stop();
                variable--;
            }
        }
    }


}

