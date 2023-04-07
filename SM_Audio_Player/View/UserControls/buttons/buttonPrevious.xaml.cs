﻿using NAudio.Wave;
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
        public buttonPrevious()
        {
            InitializeComponent();
        }

        /*Włącz pooprzedni utwór*/
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = TracksProperties.tracksList.IndexOf(TracksProperties.SelectedTrack);
            int nextIndex = (currentIndex - 1) % TracksProperties.tracksList.Count;
            if (nextIndex < 0)
            {
                nextIndex = TracksProperties.tracksList.Count - 1;
            }
            TracksProperties.SelectedTrack = TracksProperties.tracksList[nextIndex];
            TracksProperties.waveOut.Dispose();
            TracksProperties.audioFileReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
            TracksProperties.waveOut.Init(TracksProperties.audioFileReader);
            TracksProperties.waveOut.Play();
        }
    }
}
