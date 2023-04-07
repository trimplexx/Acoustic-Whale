using System;
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
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic.Devices;
using NAudio.Utils;
using NAudio.Wave;
using Newtonsoft.Json;
using SM_Audio_Player.Music;
using SM_Audio_Player.View.UserControls;
using TagLib.Mpeg;

namespace SM_Audio_Player.View.UserControls.buttons
{
    public partial class buttonPlay : UserControl, INotifyPropertyChanged
    {
        
        private bool isPlaying = false;
        private Library _library = new Library();
        private long pausedPosition = 0; // zmienna pomocnicza do przechowywania miejsca pauzy
        
        private Random random = new Random();
        
        public int GetRandomNumber()
        {
            int index = random.Next(0, TracksProperties.availableNumbers.Count);
            int randomNumber = TracksProperties.availableNumbers[index];
            if (TracksProperties.availableNumbers.Count == 0)
                TracksProperties.availableNumbers = Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
            TracksProperties.availableNumbers.RemoveAt(index);
            return randomNumber;
        }

        public buttonPlay()
        {
            TracksProperties.availableNumbers = Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
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

        public void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (TracksProperties.SelectedTrack != null)
                {
                    AudioFileReader checkReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                    if (!isPlaying)
                    {
                        // Tworzenie nowego audioFileReadera, gdy ten nie został jeszcze zadeklarowany
                        if (TracksProperties.audioFileReader == null)
                        {
                            TracksProperties.availableNumbers.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
                            TracksProperties.waveOut = new WaveOutEvent();
                            TracksProperties.audioFileReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                            TracksProperties.waveOut.Init(TracksProperties.audioFileReader);
                            TracksProperties.waveOut.Play();
                            TracksProperties.firstPlayed = TracksProperties.SelectedTrack;
                            TracksProperties.waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                        }
                        // Sprawdzanie czy podany utwór różni się z tym wybranym z listy
                        else if (TracksProperties.audioFileReader.FileName != checkReader.FileName) 
                        {
                            PlayNewTrack();
                            TracksProperties.availableNumbers = Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
                            TracksProperties.availableNumbers.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
                            TracksProperties.firstPlayed = TracksProperties.SelectedTrack;
                        }
                        else // wznawianie odtwarzania, jeżeli utwór sie nie różni w pożądanym miejscu
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Play/Pause music error");
            }
        }

        // Kasowanie poprzedniego utworu oraz odtwarzanie nowego, zapisanego w TracksProperties.SelectedTrack
        public void PlayNewTrack()
        {
            try
            {
                if (TracksProperties.waveOut != null)
                {
                    TracksProperties.waveOut.Pause();
                    TracksProperties.waveOut.Dispose();
                    TracksProperties.audioFileReader.Dispose();
                }
                TracksProperties.waveOut = new WaveOutEvent();
                TracksProperties.audioFileReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                TracksProperties.waveOut.Init(TracksProperties.audioFileReader);
                TracksProperties.waveOut.Play();
                TracksProperties.waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PlayNextTrack method exception");
            }
        }

        public void SchuffleFun()
        {
            if (TracksProperties.availableNumbers.Count == 0)
            {
                TracksProperties.waveOut.Dispose();
                TracksProperties.SelectedTrack = TracksProperties.firstPlayed;
                TracksProperties.waveOut = new WaveOutEvent();
                TracksProperties.audioFileReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                TracksProperties.waveOut.Init(TracksProperties.audioFileReader);
                TracksProperties.waveOut.Pause();
                TracksProperties.waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                TracksProperties.availableNumbers = Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
                TracksProperties.availableNumbers.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
                TracksProperties.PrevTrack.Clear();
            }
            else
            {
                int randomTrack = GetRandomNumber();
                TracksProperties.PrevTrack.Add(TracksProperties.SelectedTrack);
                TracksProperties.SelectedTrack = TracksProperties.tracksList.ElementAt(randomTrack);
                PlayNewTrack();
            }
        }

        /*  Metoda wykonywująca się w momencie zatrzymania muzyki służąca w głównej mierze do odtworzenia następnego
         *   utowru, gdy dany się zakończył
         */
        public void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            try
            {
                /* Czas zakończenia się utworu z reguły różnił się zaledwie o 10/15 milisekund od ogólnego
                 * czasu trwania utworu, tak więc do zakończonego czasu dodawana zostaje wartość 20 milisekund, 
                 * kótra przewyższa TotalTime, więc przełączony zostanie dopierdo w tym momencie na kolejny utwór.
                 */
                TimeSpan ts = new TimeSpan(0, 0, 0,0,20);
                ts += TracksProperties.audioFileReader.CurrentTime;
                if (ts > TracksProperties.audioFileReader.TotalTime)
                {
                    if (TracksProperties.isLoopOn)
                        PlayNewTrack();
                    else if (TracksProperties.isSchuffleOn)
                    {
                        SchuffleFun();
                    }
                    else if (TracksProperties.SelectedTrack.Id != TracksProperties.tracksList.Count)
                    {
                        TracksProperties.SelectedTrack =
                            TracksProperties.tracksList.ElementAt(TracksProperties.SelectedTrack.Id);
                        PlayNewTrack();
                    }
                    else
                    {
                        // Przełącz na pierwszy utwór z listy, jeżeli ostatni z niej się zakonczył
                        TracksProperties.SelectedTrack = TracksProperties.tracksList.ElementAt(0);
                        PlayNewTrack();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"EventHandler after pause music Exception");
            }
        }
    }
}
