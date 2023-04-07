﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class buttonPlay : UserControl, INotifyPropertyChanged
    {
        
        private bool isPlaying = false;
        private Library _library;
        private long pausedPosition = 0; // zmienna pomocnicza do przechowywania miejsca pauzy
        //int currentTrackIndex = -1; // zmienna pomocnicza wskazujaca wybrany utwor

        public buttonPlay()
        {
            DataContext = this;
            PlayIcon = "M0 256a256 256 0 1 1 512 0A256 256 0 1 1 0 256zM188.3 147.1c-7.6 4.2-12.3 12.3-12.3 20.9V344c0 8.7 4.7 16.7 12.3 20.9s16.8 4.1 24.3-.5l144-88c7.1-4.4 11.5-12.1 11.5-20.5s-4.4-16.1-11.5-20.5l-144-88c-7.4-4.5-16.7-4.7-24.3-.5z";
            InitializeComponent();
        }

        private string playIcon;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string PlayIcon
        {
            get { return playIcon; }
            set
            {
                playIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PlayIcon"));
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TracksProperties.SelectedTrack != null)
                {
                    if (!isPlaying)
                    {
                        if (TracksProperties.audioFileReader == null) // create a new AudioFileReader if it doesn't exist yet
                        {
                            TracksProperties.audioFileReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                            //currentTrackIndex = TracksProperties.listViewSelectedIndex;
                            TracksProperties.waveOut.Init(TracksProperties.audioFileReader);
                            TracksProperties.waveOut.Play();
                        }
                        else // otherwise, simply resume playback
                        {
                            TracksProperties.audioFileReader.Position = pausedPosition;
                            TracksProperties.waveOut.Play();
                        }
                        PlayIcon = "M256 512A256 256 0 1 0 256 0a256 256 0 1 0 0 512zM192 160H320c17.7 0 32 14.3 32 32V320c0 17.7-14.3 32-32 32H192c-17.7 0-32-14.3-32-32V192c0-17.7 14.3-32 32-32z";
                        isPlaying = true;
                    }
                    else // if is playing pause
                    {
                        pausedPosition = TracksProperties.audioFileReader.Position;
                        TracksProperties.waveOut.Pause();
                        PlayIcon = "M0 256a256 256 0 1 1 512 0A256 256 0 1 1 0 256zM188.3 147.1c-7.6 4.2-12.3 12.3-12.3 20.9V344c0 8.7 4.7 16.7 12.3 20.9s16.8 4.1 24.3-.5l144-88c7.1-4.4 11.5-12.1 11.5-20.5s-4.4-16.1-11.5-20.5l-144-88c-7.4-4.5-16.7-4.7-24.3-.5z";
                        isPlaying = false;
                    }
                }

                // Subscribe to the PlaybackStopped event
                TracksProperties.waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Play/Pause music error");
            }
        }

        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            // Check if there is another track in the list and if so, start playing it
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
