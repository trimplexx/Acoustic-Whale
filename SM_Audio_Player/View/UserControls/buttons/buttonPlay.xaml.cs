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


        private bool isPlaying = false; // Zmienna sprawdzająca, czy muzyka już gra.
        private Library _library = new Library();
        private long pausedPosition = 0; // zmienna pomocnicza do przechowywania miejsca pauzy

        public delegate void NextButtonClickedEventHandler(object sender, EventArgs e);

        public static event NextButtonClickedEventHandler TrackEnd;

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
            PlayIcon = Icons.GetPlayIcon();
            InitializeComponent();
            buttonNext.NextButtonClicked += OnTrackSwitch;
            buttonPrevious.PreviousButtonClicked += PreviousTrackEvent;
            Library.DoubleClickEvent += OnTrackSwitch;
            /*
             * Gdy aplikacja zostanie odpalona, użyty zostaje if, który to sprawdza przy inicjalizacji, aby po
             *  kliknięciu przycisku play od razu włączyła się pierwsza piosenka z listy bez potrzeby wybierania utworu ręcznie
             */
            if (TracksProperties.tracksList.Count != 0 && TracksProperties.SelectedTrack == null)
            {
                TracksProperties.SelectedTrack = TracksProperties.tracksList.ElementAt(0);
            }
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
                            TracksProperties.availableNumbers =
                                Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
                            TracksProperties.availableNumbers.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
                            TracksProperties.firstPlayed = TracksProperties.SelectedTrack;

                            // Tworzenie nowego obiektu waveOut, oraz włączanie go.
                            TracksProperties.waveOut = new WaveOutEvent();
                            TracksProperties.audioFileReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                            TracksProperties.waveOut.Init(TracksProperties.audioFileReader);
                            TracksProperties.waveOut.Play();
                            TracksProperties.firstPlayed = TracksProperties.SelectedTrack;
                            TracksProperties.waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                            TrackEnd?.Invoke(this, EventArgs.Empty);
                        }
                        // Sprawdzanie czy podany utwór różni się z tym wybranym z listy
                        else if (TracksProperties.audioFileReader.FileName != checkReader.FileName)
                        {
                            PlayNewTrack();
                            /*
                             * Resetowanie dostępnych numerów dla użycia funkcji schuffle oraz ustanowienie uttworu
                             * pierwszym na liście, do kolejnego losowego odtwarzania.
                             */
                            TracksProperties.availableNumbers =
                                Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
                            TracksProperties.availableNumbers.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
                            TracksProperties.firstPlayed = TracksProperties.SelectedTrack;
                        }
                        else // wznawianie odtwarzania, jeżeli utwór sie nie różni w pożądanym miejscu
                        {
                            TracksProperties.audioFileReader.Position = pausedPosition;
                            TracksProperties.waveOut.Play();
                        }

                        PlayIcon = Icons.GetStopIcon();
                        isPlaying = true;
                    }
                    // Jezżeli muzyka jest zapauzowana,zapamięta zostaje pozycja i zmieniona ikona przycisku.
                    else
                    {
                        pausedPosition = TracksProperties.audioFileReader.Position;
                        TracksProperties.waveOut.Pause();
                        PlayIcon = Icons.GetPlayIcon();
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
                TimeSpan ts = new TimeSpan(0, 0, 0, 0, 20);
                ts += TracksProperties.audioFileReader.CurrentTime;
                if (ts > TracksProperties.audioFileReader.TotalTime)
                {
                    // Sprawdzenie czy jest włączona opcja schuffle, ponieważ ona zmienia następny track
                    if (TracksProperties.isSchuffleOn)
                    {
                        SchuffleFun();
                        if (TracksProperties.isLoopOn)
                        {
                            if (TracksProperties.SelectedTrack == TracksProperties.firstPlayed)
                            {
                                TracksProperties.waveOut.Play();
                            }
                        }
                        else
                        {
                            if (TracksProperties.SelectedTrack == TracksProperties.firstPlayed)
                            {
                                PlayIcon = Icons.GetPlayIcon();
                                isPlaying = false;
                            }
                        }
                    }
                    else
                    {
                        if (TracksProperties.SelectedTrack.Id == TracksProperties.tracksList.Count)
                        {
                            // Przełączenie na 1 utwór z listy po zakończeniu ostatniego
                            TracksProperties.SelectedTrack = TracksProperties.tracksList.ElementAt(0);
                            PlayNewTrack(); 
                            if (!TracksProperties.isLoopOn)
                            {
                                TracksProperties.waveOut.Pause();
                                PlayIcon = Icons.GetPlayIcon();
                                isPlaying = false;
                            }
                        }
                        else
                        {
                            TracksProperties.SelectedTrack =
                                TracksProperties.tracksList.ElementAt(TracksProperties.SelectedTrack.Id);
                            PlayNewTrack();
                        }
                    }
                    
                    TrackEnd?.Invoke(this, EventArgs.Empty);
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"EventHandler after pause music Exception");
            }

        }

        private void OnTrackSwitch(object sender, EventArgs e)
        {
            PlayIcon = Icons.GetStopIcon();
            isPlaying = true;
            
            // Jednoczesne schufflowanie i zapętlanie, aby muzyka leciała schufflowana bez przerwy
            if (TracksProperties.isSchuffleOn && TracksProperties.isLoopOn && TracksProperties.SelectedTrack == 
                TracksProperties.firstPlayed)
            {
                TracksProperties.waveOut.Play();
            }
            // Sprawdzenie, czy bez użycia loopa pierwszy element z listy został włącozny.
            else if (!TracksProperties.isLoopOn && !TracksProperties.isSchuffleOn
                                                && TracksProperties.SelectedTrack == TracksProperties.tracksList.ElementAt(0))
            {
                PlayIcon = Icons.GetPlayIcon();
                isPlaying = false;
            }
            // Sprawdzenie czy ostatni track schuffli nie został zagrany, aby na pierwszym się zatrzymać
            else if (TracksProperties.isSchuffleOn && TracksProperties.SelectedTrack == TracksProperties.firstPlayed)
            {
                TracksProperties.waveOut.Pause();
                PlayIcon = Icons.GetPlayIcon();
                isPlaying = false;
            }
        }

        // Event dla poprzedniego utworu zagranego
        private void PreviousTrackEvent(object sender, EventArgs e)
        {
            PlayIcon = Icons.GetStopIcon();
            isPlaying = true;
            if (TracksProperties.isLoopOn && TracksProperties.SelectedTrack.Id == 1)
            {
                PlayIcon = Icons.GetPlayIcon();
                isPlaying = false;
            }
        }
    }
}

