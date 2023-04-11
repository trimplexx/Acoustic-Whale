using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using NAudio.Wave;
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class ButtonPlay : INotifyPropertyChanged
{
    // Zmienna sprawdzająca, czy muzyka już gra.
    private bool _isPlaying;

    // zmienna pomocnicza do przechowywania miejsca pauzy
    private long _pausedPosition;

    /*
    * Zdarzenie odnoszące się do skończenia się granego utworu samoczynnie, dzięki któremu w innych miejscach
    * kodu wyniknie reakcja. Utworzone zostało aby aktualizować poszczególne dane innych klas. 
    */
    public delegate void TrackEndEventHandler(object sender, EventArgs e);

    public static event TrackEndEventHandler? TrackEnd;

    /*
     * Eventy służące odświeżaniu listy, aby wyrzucić piosenkę przed jej odtworzeniem, gdy jego ścieżka uległaby zmianie
     * w trakcie odtwarzania. 
     */
    public delegate void RefreshListEventHandler(object sender, EventArgs e);

    public static event RefreshListEventHandler? RefreshList;

    // Obiekt do losowania liczb do Schufflowania utworów 
    private readonly Random _random = new();

    // Ikona buttouPlay
    private string? _playIcon;
    public event PropertyChangedEventHandler? PropertyChanged;

    // Funkcja losująca randomowy numer z dostępnego zakresu, używana do odtwarzania Schuffle.
    private int GetRandomNumber()
    {
        try
        {
            if (TracksProperties.AvailableNumbers != null)
            {
                var index = _random.Next(0, TracksProperties.AvailableNumbers.Count);
                var randomNumber = TracksProperties.AvailableNumbers[index];
                if (TracksProperties.AvailableNumbers.Count == 0)
                    if (TracksProperties.TracksList != null)
                        TracksProperties.AvailableNumbers =
                            Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                TracksProperties.AvailableNumbers.RemoveAt(index);
                return randomNumber;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Get random numer exception: {ex.Message}");
            throw;
        }

        return -1;
    }

    public ButtonPlay()
    {
        try
        {
            DataContext = this;
            PlayIcon = Icons.GetPlayIcon();
            InitializeComponent();
            /*
             * Przypisanie reakcji na event wywołany w innych klasach do odpowiednich metod
             */
            ButtonNext.NextButtonClicked += NextTrackEvent;
            ButtonPrevious.PreviousButtonClicked += PreviousTrackEvent;
            Library.DoubleClickEvent += OnTrackDoubleClickSwitch;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonPlay constructor exception: {ex.Message}");
            throw;
        }
    }

    // Zmiana ikony Play.
    public string? PlayIcon
    {
        get => _playIcon;
        set
        {
            _playIcon = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PlayIcon"));
        }
    }

    public void btnPlay_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            RefreshList?.Invoke(this, EventArgs.Empty);
            if (TracksProperties.TracksList.Count != 0)
            {
                /*
                * Brak wybranego tracku powoduje odpalenie pierwszego dostępnego z listy.
                */
                if (TracksProperties.SelectedTrack == null)
                    TracksProperties.SelectedTrack = TracksProperties.TracksList.ElementAt(0);

                // Obiekt do sprawdzania, czy poprzedni grany utwór, zgadza się z obecnym wybranym.
                var checkReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);

                if (!_isPlaying)
                {
                    // Tworzenie nowego audioFileReadera, gdy ten nie został jeszcze zadeklarowany
                    if (TracksProperties.AudioFileReader == null)
                    {
                        /*
                         * W momencie, gdyby uzytkownik włączył pierw funkcje Schuffle, następnie odpalił pierwszy
                         * utwór, resetuje liste, a następnie ustanawia track jako pierwszy.
                         */
                        TracksProperties.AvailableNumbers =
                                Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                        TracksProperties.AvailableNumbers?.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
                        TracksProperties.FirstPlayed = TracksProperties.SelectedTrack;

                        // Tworzenie nowego obiektu waveOut, oraz włączanie go.
                        TracksProperties.WaveOut = new WaveOutEvent();
                        TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                        TracksProperties.WaveOut.Init(TracksProperties.AudioFileReader);
                        TracksProperties.WaveOut.Play();
                        TracksProperties.FirstPlayed = TracksProperties.SelectedTrack;
                        TracksProperties.WaveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                        TrackEnd?.Invoke(this, EventArgs.Empty);
                    }
                    // Sprawdzanie czy podany utwór różni się z tym wybranym z listy
                    else if (TracksProperties.AudioFileReader.FileName != checkReader.FileName)
                    {
                        PlayNewTrack();
                        /*
                         * Resetowanie dostępnych numerów dla użycia funkcji schuffle oraz ustanowienie uttworu
                         * pierwszym na liście, do kolejnego losowego odtwarzania.
                         */
                        TracksProperties.AvailableNumbers =
                                Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                        TracksProperties.AvailableNumbers?.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
                        TracksProperties.FirstPlayed = TracksProperties.SelectedTrack;
                    }
                    else // wznawianie odtwarzania, jeżeli utwór sie nie różni w pożądanym miejscu
                    {
                        TracksProperties.WaveOut?.Play();
                    }

                    PlayIcon = Icons.GetStopIcon();
                    _isPlaying = true;
                }
                // Jezżeli muzyka jest zapauzowana,zapamięta zostaje pozycja i zmieniona ikona przycisku.
                else
                {
                    if (TracksProperties.AudioFileReader != null)
                        _pausedPosition = TracksProperties.AudioFileReader.Position;
                    TracksProperties.WaveOut?.Pause();
                    PlayIcon = Icons.GetPlayIcon();
                    _isPlaying = false;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Play/Pause music buttonClick {ex.Message} ");
            throw;
        }
    }

    // Kasowanie poprzedniego utworu oraz odtwarzanie nowego, zapisanego w TracksProperties.SelectedTrack
    public void PlayNewTrack()
    {
        try
        {
            // Czyszczenie zasobów, jeżeli są one wypełnione.
            if (TracksProperties.WaveOut != null)
            {
                TracksProperties.WaveOut.Pause();
                TracksProperties.WaveOut.Dispose();
                TracksProperties.AudioFileReader?.Dispose();
            }

            // Utworzenie nowego obiektu waveOut, w celu otworzenia utworu
            TracksProperties.WaveOut = new WaveOutEvent();
            TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack?.Path);
            TracksProperties.WaveOut.Init(TracksProperties.AudioFileReader);
            TracksProperties.WaveOut.Play();
            TracksProperties.WaveOut.PlaybackStopped += WaveOut_PlaybackStopped;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PlayNextTrack method exception {ex.Message}");
            throw;
        }
    }

    public void SchuffleFun()
    {
        try
        {
            /*
             * Kiedy użytkownik przejdzie przez wszystkie możliwe piosenki w losowej kolejności,
             * zostanie zagrany, zapamiętany pierwszy utwór w momencie, którego użył funkcji schuffle, zostanie on
             * zatrzymany, a lista dostępnych następnych numerów zostanie zresetowana.
             */
            if (TracksProperties.AvailableNumbers?.Count == 0)
            {
                TracksProperties.WaveOut?.Dispose();
                // Odtworzenie pierwszego tracku, zapamiętanego w momencie użycia opcji Schuffle.
                TracksProperties.SelectedTrack = TracksProperties.FirstPlayed;
                TracksProperties.WaveOut = new WaveOutEvent();
                TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack?.Path);
                TracksProperties.WaveOut.Init(TracksProperties.AudioFileReader);

                // Jeżeli schuffle i loop występują razem to nie pauzuje muzyki na pierwszym utworze
                if (TracksProperties.IsLoopOn == 1)
                    TracksProperties.WaveOut.Play();
                else
                    TracksProperties.WaveOut.Pause();

                TracksProperties.WaveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                // Reset listy dostępnych numerów utworów
                if (TracksProperties.TracksList != null)
                    TracksProperties.AvailableNumbers = Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();

                // Usunięcie numeru odtwarzanego utworu z listy, aby ten się nie powtórzył w momencie losowania
                if (TracksProperties.SelectedTrack != null)
                    TracksProperties.AvailableNumbers.RemoveAt(TracksProperties.SelectedTrack.Id - 1);

                // Wyczyszczenie listy poprzednich utworów
                TracksProperties.PrevTrack.Clear();
            }
            else
            {
                /*
                 * Funckja getRandomNumber wylosuje dowolny dostępny numer utworu oraz zapamięta poprzedni w
                 * liście PrevTrack, aby użytkownik mógł wrócić nielosowo do poprzedniego utworu.
                 */
                var randomTrack = GetRandomNumber();
                TracksProperties.PrevTrack.Add(TracksProperties.SelectedTrack);
                TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(randomTrack);
                PlayNewTrack();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"SchuffleFun method exception {ex.Message}");
            throw;
        }
    }

    /*  Metoda wykonywująca się w momencie zatrzymania muzyki służąca w głównej mierze do odtworzenia następnego
     *   utowru, gdy dany się zakończył
     */
    public void WaveOut_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        try
        {
            RefreshList?.Invoke(this, EventArgs.Empty);
            /* Czas zakończenia się utworu z reguły różnił się zaledwie o 10/15 milisekund od ogólnego
             * czasu trwania utworu, tak więc do zakończonego czasu dodawana zostaje wartość 20 milisekund, 
             * kótra przewyższa TotalTime, więc przełączony zostanie dopierdo w tym momencie na kolejny utwór.
             */
            var ts = new TimeSpan(0, 0, 0, 0, 20);
            if (TracksProperties.AudioFileReader != null)
            {
                ts += TracksProperties.AudioFileReader.CurrentTime;
                if (ts > TracksProperties.AudioFileReader.TotalTime)
                {
                    if (TracksProperties.IsLoopOn == 2)
                    {
                        PlayNewTrack();
                    }
                    // Sprawdzenie czy jest włączona opcja schuffle, ponieważ ona zmienia następny track
                    else if (TracksProperties.IsSchuffleOn)
                    {
                        SchuffleFun();
                        if (TracksProperties.IsLoopOn == 1)
                        {
                            if (TracksProperties.SelectedTrack == TracksProperties.FirstPlayed)
                                if (TracksProperties.WaveOut != null)
                                    TracksProperties.WaveOut.Play();
                        }
                        else
                        {
                            if (TracksProperties.SelectedTrack == TracksProperties.FirstPlayed)
                            {
                                PlayIcon = Icons.GetPlayIcon();
                                _isPlaying = false;
                            }
                        }
                    }
                    else
                    {
                        if (TracksProperties.SelectedTrack?.Id == TracksProperties.TracksList?.Count)
                        {
                            // Przełączenie na 1 utwór z listy po zakończeniu ostatniego
                            TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(0);
                            PlayNewTrack();
                            // Sprawdzenie czy jest loop, jeżeli nie to zatrzymaj muzykę i zmień ikone
                            if (TracksProperties.IsLoopOn == 0)
                            {
                                TracksProperties.WaveOut?.Pause();
                                PlayIcon = Icons.GetPlayIcon();
                                _isPlaying = false;
                            }
                            else
                            {
                                // Nie zatrzymuj muzyki oraz zmień ikone z powrotem na grającą
                                PlayIcon = Icons.GetStopIcon();
                                _isPlaying = true;
                            }
                        }
                        else
                        {
                            // Jeżeli track nie był ostatnim na liście odtwórz nastepny 
                            if (TracksProperties.SelectedTrack != null)
                                TracksProperties.SelectedTrack =
                                    TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id);
                            PlayNewTrack();
                        }
                    }

                    TrackEnd?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"EventHandler after pause music Exception {ex.Message}");
            throw;
        }
    }

    /*
     * Event wykonany w momencie wykonania dwuklika na dany utwór, zmieni ona tylko i wyłącznie ikone odtwarzania
     * a sama piosenką zostanie odtworzona poprzez wywołanie buttonPlay_click
     */
    private void OnTrackDoubleClickSwitch(object sender, EventArgs e)
    {
        try
        {
            PlayIcon = Icons.GetStopIcon();
            _isPlaying = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OnDoubleClickSwitch exception {ex.Message}");
            throw;
        }
    }

    /*
     * Metoda odnosząca sie do wykonania akcji kliknięcia przycisku następnej piosenki.
     */
    private void NextTrackEvent(object sender, EventArgs e)
    {
        try
        {
            PlayIcon = Icons.GetStopIcon();
            _isPlaying = true;

            // Sprawdzenie, czy bez użycia loopa pierwszy element z listy został włącozny.
            if (TracksProperties.IsLoopOn == 0 && !TracksProperties.IsSchuffleOn &&
                TracksProperties.SelectedTrack == TracksProperties.TracksList?.ElementAt(0))
            {
                PlayIcon = Icons.GetPlayIcon();
                _isPlaying = false;
            }

            // Sprawdzenie czy ostatni track schuffli nie został zagrany, aby na pierwszym się zatrzymać
            if (TracksProperties.IsSchuffleOn && TracksProperties.IsLoopOn != 1 &&
                TracksProperties.SelectedTrack == TracksProperties.FirstPlayed)
            {
                TracksProperties.WaveOut?.Pause();
                PlayIcon = Icons.GetPlayIcon();
                _isPlaying = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OnTrackSwitch by nextButton exception {ex.Message}");
            throw;
        }
    }

    /*
    * Metoda odnosząca sie do wykonania akcji kliknięcia przycisku poprzedniej piosenki.
    */
    private void PreviousTrackEvent(object sender, EventArgs e)
    {
        try
        {
            PlayIcon = Icons.GetStopIcon();
            _isPlaying = true;
            if (TracksProperties.IsLoopOn == 1 && TracksProperties.SelectedTrack?.Id == 1)
            {
                PlayIcon = Icons.GetPlayIcon();
                _isPlaying = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OnTrackSwitch by previousButton exception {ex.Message}");
            throw;
        }
    }
}