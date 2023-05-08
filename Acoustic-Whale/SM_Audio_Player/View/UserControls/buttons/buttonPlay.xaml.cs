using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using NAudio.Wave;
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons;

///<summary>
/// Klasa ButtonPlay obsługuje przycisk odtwarzania muzyki w odtwarzaczu. Zawiera metody i eventy służące odświeżaniu
/// listy, resetowaniu danych, zmianie ikony przycisku, obsłudze zdarzenia skończenia się granego utworu, czy też
/// reakcji na kliknięcie przycisku odtwarzania.
/// </summary>
public partial class ButtonPlay : INotifyPropertyChanged
{
    public delegate void ButtonPlayEventToEqualizer(object sender, EventArgs e);

    /// <summary>
    /// Delegat służący odświeżaniu listy, aby wyrzucić piosenkę przed jej odtworzeniem, gdy jego ścieżka uległaby zmianie w trakcie odtwarzania. 
    /// </summary>
    public delegate void RefreshListEventHandler(object sender, EventArgs e);


    /// <summary>
    /// Delegat eventu informujący o potrzebie resetu wszystkich danych w przypadku, gdy lista odtwarzania będzie zawierać mniej elementów
    /// niż ta, która była zapamiętana przed odświeżeniem.
    /// </summary>
    public delegate void ResetEverythingEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Delegat eventu informujący o skończeniu się granego utworu.
    /// </summary>
    public delegate void TrackEndEventHandler(object sender, EventArgs e);


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
            Equalizer.FadeInEvent += NextTrackEvent;
            Equalizer.FadeOffOn += NextTrackEvent;
            Library.OnDeleteTrack += NextTrackEvent;
            MainWindow.PlayMusic += btnPlay_Click;
            MainWindow.PlayMusic += NextTrackEvent;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonPlay constructor exception: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Zmiana ikony buttona Play
    /// </summary>
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

    /// <summary>
    /// Event handler dla przycisku Play.
    /// <code>Sprawdza czy lista utworów istnieje, czy nie jest pusta oraz czy nie trwa już
    /// żadne odtwarzanie utworu z innych źródeł dźwięku. Następnie dokonuje przepisywania na bazowy obiekt WaveOut, gdy
    /// odtwarzany jest utwór z innego źródła dźwięku niż bazowy.
    /// </code>
    /// <code>Odświeża listę, sprawdza czy lista się zmieniła i porównuje ją z
    /// poprzednią w celu zabezpieczenia przed zmianą ścieżki pliku jednego z utworów. Jeśli nastąpiła zmiana, to
    /// czyszczone są wszelkie dane związane z poprzednim utworem i wyświetlany jest komunikat. Jeśli lista nie uległa
    /// zmianie, to sprawdza czy wybrany jest już utwór, jeśli nie to odpala pierwszy utwór, jeśli tak to sprawdza czy
    /// wybrany utwór różni się od tego, który jest aktualnie odtwarzany i odpowiednio go przestawia.
    /// </code>
    /// </summary>
    public void btnPlay_Click(object sender, EventArgs e)
    {
        try
        {
            if (TracksProperties.TracksList != null && TracksProperties.TracksList.Count != 0 &&
                TracksProperties.SpaceFlag)
            {
                /*
                 * Przepiswanie w momencie zatrzymania muzyki utworu odtwarzanego z drugiego źródła dźwięku, na
                 * bazowy obiekt WaveOut.
                 */
                if (TracksProperties.SecWaveOut != null &&
                    TracksProperties.SecWaveOut.PlaybackState == PlaybackState.Playing)
                {
                    if (TracksProperties.SelectedTrack?.Path == TracksProperties.SecAudioFileReader?.FileName)
                        TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;
                    TracksProperties.Timer.Stop();
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut?.Init(TracksProperties.AudioFileReader);
                    TracksProperties.SecWaveOut.Stop();
                    TracksProperties.SecWaveOut.Dispose();
                    TracksProperties.SecAudioFileReader = null;
                }

                /*
                * Walidacja odświeżania listy, zapisuje bieżącą wartość posiadanych utworów na liście, a następnie
                * wykonane zostanie jej odświeżenie poprzez wywołanie 'RefreshList', następnie porównywana jest wartość
                * odświeżonej listy oraz zapisanej, w celu sprawdzenia czy ścieżka, któreś z piosenek nie uległa zmianie.
                * Jeżeli wartość piosenek uległa zmianie, następuje wyczyszczenie wszelkich danych związanych z piosenką
                * zarówno tych w widoku poprzez wywołanie zdarzenia ResetEverything.
                */
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

                    MessageBox.Show("Ups! Któryś z odtwarzanych utworów zmienił swoją ścieżkę do pliku :(");
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
                            TracksProperties.AvailableNumbers =
                                Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                            var random = new Random();
                            TracksProperties.AvailableNumbers =
                                TracksProperties.AvailableNumbers.OrderBy(_ => random.Next()).ToList();
                            TracksProperties.AvailableNumbers.Remove(TracksProperties.SelectedTrack.Id - 1);
                            TracksProperties.FirstPlayed = TracksProperties.SelectedTrack;
                            PlayNewTrack();
                        }
                        // Sprawdzanie czy podany utwór różni się z tym wybranym z listy
                        else if (TracksProperties.AudioFileReader.FileName != checkReader.FileName)
                        {
                            TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack.Path);
                            /*
                            * Resetowanie dostępnych numerów dla użycia funkcji schuffle oraz ustanowienie uttworu
                            * pierwszym na liście, do kolejnego losowego odtwarzania.
                            */
                            TracksProperties.AvailableNumbers =
                                Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                            var random = new Random();
                            TracksProperties.AvailableNumbers =
                                TracksProperties.AvailableNumbers.OrderBy(_ => random.Next()).ToList();
                            TracksProperties.AvailableNumbers.Remove(TracksProperties.SelectedTrack.Id - 1);
                            TracksProperties.FirstPlayed = TracksProperties.SelectedTrack;
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

                if (_isPlaying) ButtonPlayEvent?.Invoke(this, EventArgs.Empty);

                TracksProperties.SpaceFlag = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Play/Pause music buttonClick {ex.Message} ");
            throw;
        }
    }

    ///<summary>
    /// Metoda PlayNewTrack odtwarza nowy utwór muzyczny.
    /// <code>
    /// Jeśli opcja zanikania dźwięku jest włączona i istnieje drugi plik audio, to przypisuje go do głównego pliku audio.
    /// W przeciwnym razie tworzy nowy obiekt AudioFileReader dla wybranego utworu.
    /// </code>
    /// <code>
    /// Jeśli obiekt WaveOut istnieje, zatrzymuje odtwarzanie i dodaje zdarzenie PlaybackStopped.
    /// W przeciwnym razie tworzy nowy obiekt WaveOutEvent i AudioFileReader dla wybranego utworu oraz dodaje zdarzenie PlaybackStopped.
    /// </code>
    /// </summary>
    public void PlayNewTrack()
    {
        try
        {
            if (TracksProperties.IsFadeOn && TracksProperties.SecAudioFileReader != null)
            {
                TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;
                TracksProperties.SecAudioFileReader = null;
            }
            else
            {
                TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack?.Path);
            }

            if (TracksProperties.WaveOut != null)
            {
                TracksProperties.WaveOut.Stop();
                // Przypisanie eventu odpowiadającego za zatrzymanie się utworu, użytego w celu zmiany utworu na nastepny
                TracksProperties.WaveOut.PlaybackStopped += WaveOut_PlaybackStopped;
            }
            else
            {
                TracksProperties.WaveOut = new WaveOutEvent();
                TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack?.Path);
                TracksProperties.WaveOut.PlaybackStopped += WaveOut_PlaybackStopped;
            }

            TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack?.Path);
            TracksProperties.Timer.Start();
            _isPlaying = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PlayNextTrack method exception {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Metoda odpowiadająca za obsługę odtwarzania granej muzyki przy włączonej funkcji programu Schuffle,
    /// która odpowiada za granie losowych utworów z podanej listy w aplikacji. 
    /// </summary>
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
                    var random = new Random();
                    TracksProperties.AvailableNumbers =
                        TracksProperties.AvailableNumbers.OrderBy(_ => random.Next()).ToList();
                }

                // Usunięcie numeru odtwarzanego utworu z listy, aby ten się nie powtórzył w momencie losowania
                if (TracksProperties.SelectedTrack != null)
                    TracksProperties.AvailableNumbers.Remove(TracksProperties.SelectedTrack.Id - 1);

                // Wyczyszczenie listy poprzednich utworów
                TracksProperties.PrevTrack.Clear();
                TracksProperties.FirstPlayed = TracksProperties.SelectedTrack;
            }
            else
            {
                var isSchuffleFunWithNextButtonFirst = false;
                if (TracksProperties.AvailableNumbers == null && TracksProperties.TracksList != null)
                {
                    TracksProperties.AvailableNumbers = Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                    var random = new Random();
                    TracksProperties.AvailableNumbers =
                        TracksProperties.AvailableNumbers.OrderBy(_ => random.Next()).ToList();
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
    
    /// <summary>
    /// Metoda wykonywująca się w momencie zatrzymania muzyki służąca do sprawdzania czy moment zatrzymania się utworu
    /// wystąpił w chwili, gdzie wartość bieżącego czasu jest równa jemu końcowi. Dzięki czemu wystąpi odpowiednia instrukcja
    /// pod względem sprawdzenia wszelkich dostepnych funkcji oraz odtworzy następny utwór samoczynnie.
    /// </summary>
    public void WaveOut_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        try
        {
            RefreshList?.Invoke(this, EventArgs.Empty);
            /* Czas zakończenia się utworu z reguły różnił się zaledwie o 10/15 milisekund od ogólnego
             * czasu trwania utworu, tak więc do zakończonego czasu dodawana zostaje wartość 20 milisekund, 
             * kótra przewyższa TotalTime, więc przełączony zostanie dopierdo w tym momencie na kolejny utwór.
             */
            var ts = new TimeSpan(0, 0, 0, 0, 40);
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
    
    /// <summary>
    /// Event wykonany w momencie wykonania dwuklika na dany utwór, zmieni ona tylko i wyłącznie ikone odtwarzania,
    /// a sama piosenką zostanie odtworzona poprzez wywołanie buttonPlay_click
    /// </summary>
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


    /// <summary>
    /// Metoda odnosząca sie do wykonania akcji kliknięcia przycisku następnej piosenki.
    /// </summary>
    private void NextTrackEvent(object sender, EventArgs e)
    {
        try
        {
            if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Paused &&
                TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Paused)
            {
                PlayIcon = Icons.GetPlayIcon();
                _isPlaying = false;
            }

            if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
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
    
    /// <summary>
    /// Metoda odnosząca sie do wykonania akcji kliknięcia przycisku poprzedniej piosenki.
    /// </summary>
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