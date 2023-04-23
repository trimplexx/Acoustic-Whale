using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using NAudio.Utils;
using NAudio.Wave;
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class ButtonPlay : INotifyPropertyChanged
{
    /*
     * Eventy służące odświeżaniu listy, aby wyrzucić piosenkę przed jej odtworzeniem, gdy jego ścieżka uległaby zmianie
     * w trakcie odtwarzania. 
     */
    public delegate void RefreshListEventHandler(object sender, EventArgs e);

    /*
    * Akcja odpowiadająca za resetowanie danych w momencie, gdy odświeżona lista będzie zawierać mniej elementów
    * niż ta wartość, która została zapisana przed jej odświeżeniem (Przykładowo, gdy ktoś zmieni ścieżkę do pliku
    * w trakcie używania aplikacji mogła by ona wyrzucić wyjątek)
    */
    public delegate void ResetEverythingEventHandler(object sender, EventArgs e);

    /*
    * Zdarzenie odnoszące się do skończenia się granego utworu samoczynnie, dzięki któremu w innych miejscach
    * kodu wyniknie reakcja. Utworzone zostało aby aktualizować poszczególne dane innych klas. 
    */
    public delegate void TrackEndEventHandler(object sender, EventArgs e);
    public delegate void ButtonPlayEventToEqualizer(object sender, EventArgs e);


    // Obiekt do losowania liczb do Schufflowania utworów 
    private readonly Random _random = new();

    // Zmienna sprawdzająca, czy muzyka już gra.
    private bool _isPlaying;

    // Ikona buttouPlay
    private string? _playIcon;

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
            TrackEnd += NextTrackEvent;
            ButtonPrevious.PreviousButtonClicked += PreviousTrackEvent;
            Library.DoubleClickEvent += OnTrackDoubleClickSwitch;
            Library.ResetEverything += NextTrackEvent;
            ResetEverything += NextTrackEvent;
            ButtonNext.ResetEverything += NextTrackEvent;
            ButtonPrevious.ResetEverything += NextTrackEvent;
            //Equalizer.FadeOffOn += NextTrackEvent;
            Equalizer.FadeInEvent += NextTrackEvent;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    public static event TrackEndEventHandler? TrackEnd;

    public static event RefreshListEventHandler? RefreshList;

    public static event ResetEverythingEventHandler? ResetEverything;
    public static event ButtonPlayEventToEqualizer? ButtonPlayEvent;


    public void btnPlay_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            /*
             * Walidacja odświeżania listy, zapisuje bieżącą wartość posiadanych utworów na liście, a następnie
             * wykonane zostanie jej odświeżenie poprzez wywołanie 'RefreshList', następnie porównywana jest wartość
             * odświeżonej listy oraz zapisanej, w celu sprawdzenia czy ścieżka, któreś z piosenek nie uległa zmianie.
             * Jeżeli wartość piosenek uległa zmianie, następuje wyczyszczenie wszelkich danych związanych z piosenką
             * zarówno tych w widoku poprzez wywołanie zdarzenia ResetEverything.
             */
            if (TracksProperties.TracksList != null && TracksProperties.TracksList.Count != 0)
            {
                if (TracksProperties.SecWaveOut != null && TracksProperties.SecWaveOut.PlaybackState == PlaybackState.Playing)
                {
                    if (TracksProperties.SelectedTrack?.Path == TracksProperties.SecAudioFileReader?.FileName)
                    {
                        TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;
                    }
                    TracksProperties._timer.Stop();
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut?.Init(TracksProperties.AudioFileReader);
                    TracksProperties.SecWaveOut.Stop();
                    TracksProperties.SecWaveOut.Dispose();
                    TracksProperties.SecAudioFileReader = null;
                }
                
                var trackListBeforeRefresh = TracksProperties.TracksList.Count;
                RefreshList?.Invoke(this, EventArgs.Empty);
                if (trackListBeforeRefresh != TracksProperties.TracksList.Count)
                {
                    if (TracksProperties.WaveOut != null && TracksProperties.AudioFileReader != null)
                    {
                        TracksProperties.WaveOut.Pause();
                        TracksProperties.WaveOut.Dispose();
                        TracksProperties.AudioFileReader = null;
                        TracksProperties.SelectedTrack = null;
                    }
                    MessageBox.Show($"Ups! Któryś z odtwarzanych utworów zmienił swoją ścieżkę do pliku :(");
                    ResetEverything?.Invoke(this, EventArgs.Empty);
                }
                else
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

                            TracksProperties.AvailableNumbers = Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                            Random random = new Random();
                            TracksProperties.AvailableNumbers = TracksProperties.AvailableNumbers.OrderBy(x => random.Next()).ToList();
                            TracksProperties.AvailableNumbers.Remove(TracksProperties.SelectedTrack.Id - 1);
                            
                            TracksProperties.FirstPlayed = TracksProperties.SelectedTrack;

                            // Tworzenie nowego obiektu waveOut, oraz włączanie go.
                            TracksProperties.WaveOut = new WaveOutEvent();
                            TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                            TracksProperties.WaveOut.Init(TracksProperties.AudioFileReader);
                            TracksProperties.WaveOut.Play();
                            _isPlaying = true;
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
                            TracksProperties.AvailableNumbers = Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                            Random random = new Random();
                            TracksProperties.AvailableNumbers = TracksProperties.AvailableNumbers.OrderBy(x => random.Next()).ToList();
                            TracksProperties.AvailableNumbers.Remove(TracksProperties.SelectedTrack.Id - 1);
                            TracksProperties.FirstPlayed = TracksProperties.SelectedTrack;
                        }
                        else // wznawianie odtwarzania, jeżeli utwór sie nie różni w pożądanym miejscu
                        {
                            TracksProperties.WaveOut?.Play();
                            TracksProperties._timer.Start();
                        }

                        PlayIcon = Icons.GetStopIcon();
                        _isPlaying = true;
                    }
                    // Jezżeli muzyka jest zapauzowana,zapamięta zostaje pozycja i zmieniona ikona przycisku.
                    else
                    {
                        TracksProperties.WaveOut?.Pause();
                        PlayIcon = Icons.GetPlayIcon();
                        _isPlaying = false;
                    }
                }
                if(_isPlaying == true)
                    ButtonPlayEvent?.Invoke(this, EventArgs.Empty);
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
                _isPlaying = false;
                TracksProperties.WaveOut.Dispose();
                TracksProperties.AudioFileReader?.Dispose();
            }

            // Utworzenie nowego obiektu waveOut, w celu otworzenia utworu
            if (TracksProperties.IsFadeOn && TracksProperties.SecAudioFileReader != null)
            {
                TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;
                TracksProperties.SecAudioFileReader = null;
            }
            else
            {
                TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack?.Path);
            }
            TracksProperties.WaveOut = new WaveOutEvent();
            TracksProperties.WaveOut.Init(TracksProperties.AudioFileReader);
            TracksProperties.WaveOut.Play();
            TracksProperties._timer.Start();
            _isPlaying = true;
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
                {
                    TracksProperties.WaveOut.Play();
                    _isPlaying = true;
                }

                else
                {
                    TracksProperties.WaveOut.Pause();
                    _isPlaying = false;
                }

                TracksProperties.WaveOut.PlaybackStopped += WaveOut_PlaybackStopped;

                // Reset listy dostępnych numerów utworów
                if (TracksProperties.TracksList != null)
                {
                    TracksProperties.AvailableNumbers = Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                    Random random = new Random();
                    TracksProperties.AvailableNumbers =
                        TracksProperties.AvailableNumbers.OrderBy(x => random.Next()).ToList();
                }

                // Usunięcie numeru odtwarzanego utworu z listy, aby ten się nie powtórzył w momencie losowania
                if (TracksProperties.SelectedTrack != null)
                    TracksProperties.AvailableNumbers.Remove(TracksProperties.SelectedTrack.Id - 1);

                // Wyczyszczenie listy poprzednich utworów
                TracksProperties.PrevTrack.Clear();
            }
            else
            {
                var isSchuffleFunWithNextButtonFirst = false;
                if (TracksProperties.AvailableNumbers == null && TracksProperties.TracksList != null)
                {
                    TracksProperties.AvailableNumbers = Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                    Random random = new Random();
                    TracksProperties.AvailableNumbers =
                        TracksProperties.AvailableNumbers.OrderBy(x => random.Next()).ToList();
                    isSchuffleFunWithNextButtonFirst = true;
                }

                /*
                 * Funckja getRandomNumber wylosuje dowolny dostępny numer utworu oraz zapamięta poprzedni w
                 * liście PrevTrack, aby użytkownik mógł wrócić nielosowo do poprzedniego utworu.
                 */
                TracksProperties.PrevTrack.Add(TracksProperties.SelectedTrack);
                if (TracksProperties.AvailableNumbers != null)
                {
                    TracksProperties.SelectedTrack =
                        TracksProperties.TracksList?.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0));
                    TracksProperties.AvailableNumbers.RemoveAt(0);
                }
                    
                if (isSchuffleFunWithNextButtonFirst)
                    TracksProperties.FirstPlayed = TracksProperties.SelectedTrack;

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
            if (TracksProperties.AudioFileReader != null && !TracksProperties.IsFadeOn)
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
                            if (TracksProperties.SelectedTrack == TracksProperties.FirstPlayed)
                                if (TracksProperties.WaveOut != null)
                                    TracksProperties.WaveOut.Play();
                    }
                    else
                    {
                        if (TracksProperties.SelectedTrack?.Id == TracksProperties.TracksList?.Count)
                        {
                            // Przełączenie na 1 utwór z listy po zakończeniu ostatniego
                            TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(0);
                            PlayNewTrack();
                            // Sprawdzenie czy jest loop, jeżeli nie to zatrzymaj muzykę i zmień ikone
                            if (TracksProperties.IsLoopOn == 0) TracksProperties.WaveOut?.Pause();
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
            if (TracksProperties.WaveOut != null && TracksProperties.WaveOut.PlaybackState == PlaybackState.Playing)
            {
                PlayIcon = Icons.GetStopIcon();
                _isPlaying = true;
            }
            else
            {
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