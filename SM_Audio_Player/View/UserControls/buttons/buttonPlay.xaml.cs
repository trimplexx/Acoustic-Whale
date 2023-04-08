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
        
        // Zmienna sprawdzająca, czy muzyka już gra.
        private bool isPlaying = false;
        private Library _library = new Library();
        // zmienna pomocnicza do przechowywania miejsca pauzy
        private long pausedPosition = 0; 
        
        private Random random = new Random();
        
        // Funkcja losująca randomowy numer z dostępnego zakresu, używana do odtwarzania Schuffle.
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
            DataContext = this;
            PlayIcon = "M0 256a256 256 0 1 1 512 0A256 256 0 1 1 0 256zM188.3 147.1c-7.6 4.2-12.3 12.3-12.3 20.9V344c0 8.7 4.7 16.7 12.3 20.9s16.8 4.1 24.3-.5l144-88c7.1-4.4 11.5-12.1 11.5-20.5s-4.4-16.1-11.5-20.5l-144-88c-7.4-4.5-16.7-4.7-24.3-.5z";
            InitializeComponent();
        }

        private string playIcon;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Zmiana ikony Play.
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
                    // Obiekt do sprawdzania, czy poprzedni grany utwór, zgadza się z obecnym wybranym.
                    AudioFileReader checkReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                    
                    if (!isPlaying)
                    {
                        // Tworzenie nowego audioFileReadera, gdy ten nie został jeszcze zadeklarowany
                        if (TracksProperties.audioFileReader == null)
                        {
                            /*
                             * W momencie, gdyby uzytkownik włączył pierw funkcje Schuffle, następnie odpalił pierwszy
                             * utwór, resetuje liste, a następnie ustanawia track jako pierwszy.
                             */
                            TracksProperties.availableNumbers = Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
                            TracksProperties.availableNumbers.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
                            TracksProperties.firstPlayed = TracksProperties.SelectedTrack;
                            
                            // Tworzenie nowego obiektu waveOut, oraz włączanie go.
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
                            /*
                             * Resetowanie dostępnych numerów dla użycia funkcji schuffle oraz ustanowienie uttworu
                             * pierwszym na liście, do kolejnego losowego odtwarzania.
                             */
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
                    // Jezżeli muzyka jest zapauzowana,zapamięta zostaje pozycja i zmieniona ikona przycisku.
                    else 
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
                // Czyszczenie zasobów, jeżeli są one wypełnione.
                if (TracksProperties.waveOut != null)
                {
                    TracksProperties.waveOut.Pause();
                    TracksProperties.waveOut.Dispose();
                    TracksProperties.audioFileReader.Dispose();
                }
                // Utworzenie nowego obiektu waveOut, w celu otworzenia utworu
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
            /*
             * Kiedy użytkownik przejdzie przez wszystkie możliwe piosenki w losowej kolejności,
             * zostanie zagrany, zapamiętany pierwszy utwór w momencie, którego użył funkcji schuffle, zostanie on
             * zatrzymany, a lista dostępnych następnych numerów zostanie zresetowana.
             */
            if (TracksProperties.availableNumbers.Count == 0)
            {
                TracksProperties.waveOut.Dispose();
                // Odtworzenie pierwszego tracku, zapamiętanego w momencie użycia opcji Schuffle.
                TracksProperties.SelectedTrack = TracksProperties.firstPlayed;
                TracksProperties.waveOut = new WaveOutEvent();
                TracksProperties.audioFileReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                TracksProperties.waveOut.Init(TracksProperties.audioFileReader);
                // Zapauzwanie oznaczające, że skończyły się dostępne utwory
                TracksProperties.waveOut.Pause();
                TracksProperties.waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                // Reset listy dostępnych numerów utworów
                TracksProperties.availableNumbers = Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
                // Usunięcie numeru odtwarzanego utworu z listy, aby ten się nie powtórzył w momencie losowania
                TracksProperties.availableNumbers.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
                // Wyczyszczenie listy poprzednich utworów
                TracksProperties.PrevTrack.Clear();
            }
            else
            {
                /*
                 * Funckja getRandomNumber wylosuje dowolny dostępny numer utworu oraz zapamięta poprzedni w
                 * liście PrevTrack, aby użytkownik mógł wrócić nielosowo do poprzedniego utworu.
                 */
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
                    // sprawdzanie czy następny utwór ma być loopem odtwarzanego
                    if (TracksProperties.isLoopOn)
                        PlayNewTrack();
                    // Sprawdzanie czy użyta została funkcja Schuffle.
                    else if (TracksProperties.isSchuffleOn)
                        SchuffleFun();
                    // W przeciwnym wypadku włącz następny track dostępny z listy.
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
