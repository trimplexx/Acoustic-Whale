using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using NAudio.Wave;
using SM_Audio_Player.Helper;
using SM_Audio_Player.Music;
using SM_Audio_Player.View.UserControls.buttons;

namespace SM_Audio_Player.View.UserControls;

/// <summary>
/// Klasa reprezentująca funkcjonalność Playera.
/// Obsługuje funkcje takie jak odtwarzanie/pauzowanie przewijanie itd.
/// </summary>
public partial class Player : INotifyPropertyChanged
{
    public delegate void FirstToSecEventHandler(object sender, EventArgs e);

    public delegate void SecToFirstEventHandler(object sender, EventArgs e);


    private string? _albumImg;
    private TimeSpan _result;
    public static event FirstToSecEventHandler? FirstToSec;
    public static event SecToFirstEventHandler? SecToFirst;

    [Obsolete("Obsolete")]
    public Player()
    {
        try
        {
            DataContext = this;
            AlbumImg = "..\\..\\assets\\default.png";
            InitializeComponent();
            /*
             * Przypisanie zdarzeń wywołanych z innych miejsc projektu
             */
            ButtonNext.NextButtonClicked += OnTrackSwitch;
            ButtonPrevious.PreviousButtonClicked += OnTrackSwitch;
            ButtonNext.ResetEverything += ResetValues;
            ButtonPrevious.ResetEverything += ResetValues;
            ButtonPlay.TrackEnd += OnTrackSwitch;
            ButtonPlay.ResetEverything += ResetValues;
            Library.DoubleClickEvent += OnTrackSwitch;
            Library.ResetEverything += ResetValues;
            Library.ResetSelected += OnTrackSwitch;

            Equalizer.FadeInEvent += OnTrackSwitch;
            Equalizer.FadeOffOn += OnTrackSwitch;
            Library.OnDeleteTrack += OnTrackSwitch;
            MainWindow.ForwardSong += TimeSlider_CtrlShiftRight;
            MainWindow.RewindSong += TimeSlider_CtrlShiftRight;

            // Przypisanie metody na tick timera
            TracksProperties.Timer.Tick += Timer_Tick;
            // Usatwienie co ile odświeża sie timer
            TracksProperties.Timer.Interval = new TimeSpan(0, 0, 0, 0, 400);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Player constructor exception: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Właściwość AlbumImg przechowuje ścieżkę do pliku z obrazem okładki albumu. Gdy wartość właściwości zostanie zmieniona,
    /// wywołuje zdarzenie PropertyChanged, aby powiadomić o zmianie wszystkich zainteresowanych obiektów.
    /// </summary>
    public string? AlbumImg
    {
        get => _albumImg;
        set
        {
            _albumImg = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlbumImg"));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Metoda ResetValues resetuje wartości niektórych kontrolek w widoku głównym programu. Zatrzymuje także timer liczący
    /// czas odtwarzania utworu. Ustawia wartości suwaka czasu odtwarzania na 0, a także ustawia wartości tekstowe kontrolek
    /// tytułu, autora, albumu i czasu odtwarzania na wartości domyślne. Dodatkowo, ustawia wartość właściwości AlbumImg
    /// na null, aby usunąć obraz okładki albumu.
    /// </summary>
    private void ResetValues(object sender, EventArgs e)
    {
        TracksProperties.Timer.Stop();
        SldTime.Value = 0;
        Title.Text = "Title";
        Author.Text = "Author";
        Cd.Text = "Album";
        TbTime.Text = "0:00";
        TbCurrTime.Text = "0:00";
        AlbumImg = null;
    }
    
    /// <summary>
    /// Event wykonywany w momencie zmiany piosenki, czy zaznaczenia opcji Fade in/out w celu zmiany poszczególnych
    /// wartości w widoku oraz zmiennej _result określającej pożądaną długość piosenki. _result jest wartością od której
    /// odejmujemy 7 sekund w momencie włączenia efektu fade in/out w celu skrócenia piosenki aby nastąpiło przełączenie
    /// na kolejną w klasie Equalizer z efektem stopniowego pogłaśniania, gdy dana piosenka będzie się wyciszać na ostatnie
    /// 7 sekund.
    /// </summary>
    [Obsolete("Obsolete")]
    private void OnTrackSwitch(object sender, EventArgs e)
    {
        try
        {
            if (TracksProperties.SelectedTrack != null)
            {
                Title.Text = TracksProperties.SelectedTrack.Title;
                TextAnimationHelper.TextAnimation(Title.Text.Length, Title, 16, CanTitle);
                Author.Text = TracksProperties.SelectedTrack.Author;
                TextAnimationHelper.TextAnimation(Author.Text.Length, Author, 21, CanAuthor);
                Cd.Text = TracksProperties.SelectedTrack.Album;
                TextAnimationHelper.TextAnimation(Cd.Text.Length, Cd, 20, CanCd);
                AlbumImg = TracksProperties.SelectedTrack.AlbumCoverPath;

                /*
                 * Sprawdzanie włączonej opcji Fade in/out
                 */
                if (TracksProperties.IsFadeOn)
                {
                    /*
                     * Jeżeli loop to odejmuje czas od AudioFileReadera.
                     */
                    if (TracksProperties.IsLoopOn == 2)
                    {
                        if (TracksProperties.AudioFileReader != null)
                        {
                            var totalFirstTime = TracksProperties.AudioFileReader.TotalTime.ToString(@"hh\:mm\:ss");
                            _result = TimeSpan.Parse(totalFirstTime) - TimeSpan.FromSeconds(7);
                        }
                    }
                    else
                    {
                        if (TracksProperties.TracksList != null)
                        {
                            /*
                             * Sprawdzanie, który z obecnych ścieżek dźwięku jest odtwarzany, a następnie od właściwej
                             * jest odejmowany czas 7 sekund.
                             */
                            if (TracksProperties.SelectedTrack.Path == TracksProperties.AudioFileReader?.FileName)
                                _result = TracksProperties.AudioFileReader.TotalTime - TimeSpan.FromSeconds(7);
                            else if (TracksProperties.SelectedTrack.Path ==
                                     TracksProperties.SecAudioFileReader?.FileName)
                                _result = TracksProperties.SecAudioFileReader.TotalTime - TimeSpan.FromSeconds(7);
                            TracksProperties.SelectedTrack.Duration = _result.TotalHours >= 1
                                ? _result.ToString(@"hh\:mm\:ss")
                                : _result.ToString(@"mm\:ss");
                        }
                    }

                    /*
                     * Przypisanie czasu do wybranego utworu, a następnie wyświetlenie go w widoku.
                     */
                    TracksProperties.SelectedTrack.Duration = _result.TotalHours >= 1
                        ? _result.ToString(@"hh\:mm\:ss")
                        : _result.ToString(@"mm\:ss");
                }
                else
                {
                    if (TracksProperties.AudioFileReader != null)
                    {
                        _result = TracksProperties.AudioFileReader.TotalTime;
                        TracksProperties.SelectedTrack.Duration = _result.TotalHours >= 1
                            ? _result.ToString(@"hh\:mm\:ss")
                            : _result.ToString(@"mm\:ss");
                    }
                }

                TbTime.Text = TracksProperties.SelectedTrack.Duration;
            }

            TracksProperties.Timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OnTrackSwtich in Player class exception: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Metoda pobierająca czas z pierwszej (bazowej) ścieżki dźwiękowej w celu aktualizowania pozycji slidera
    /// w metodzieTimer_tick
    /// </summary>
    private void TakeFirstWave()
    {
        var totalSeconds = _result.TotalSeconds;
        if (TracksProperties.AudioFileReader != null)
        {
            var currentPosition = TracksProperties.AudioFileReader.CurrentTime.TotalSeconds;
            var progress = currentPosition / totalSeconds;
            TbCurrTime.Text = currentPosition >= 3600
                ? TimeSpan.FromSeconds(currentPosition).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
                : TimeSpan.FromSeconds(currentPosition).ToString(@"mm\:ss", CultureInfo.InvariantCulture);
            SldTime.Value = progress * SldTime.Maximum;
            if (currentPosition > totalSeconds) FirstToSec?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Metoda pobierająca czas z drugiej ścieżki dźwiękowej w celu aktualizowania pozycji slidera w
    /// metodzie Timer_tick
    /// </summary>
    private void TakeSecWave()
    {
        var totalSeconds = _result.TotalSeconds;
        if (TracksProperties.SecAudioFileReader != null)
        {
            var currentPositionSec = TracksProperties.SecAudioFileReader.CurrentTime.TotalSeconds;

            var progressSec = currentPositionSec / totalSeconds;
            TbCurrTime.Text = currentPositionSec >= 3600
                ? TimeSpan.FromSeconds(currentPositionSec).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
                : TimeSpan.FromSeconds(currentPositionSec).ToString(@"mm\:ss", CultureInfo.InvariantCulture);
            SldTime.Value = progressSec * SldTime.Maximum;
            if (currentPositionSec > totalSeconds) SecToFirst?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Metoda wywoływana na tick zegara w celu odświeżenia slidera o wartości sekundowe oraz licznika pokazującego
    /// bieżący czas utworu
    /// </summary>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        try
        {
            if (TracksProperties.AudioFileReader != null)
            {
                /*
                 * Specjalne sprawdzanie, wymyślone na potrzebę włączonej funkcji loop == 2 odpowiadającej
                 * za otwarzanie po sobie tych samych piosenek w celu pobrania właściwego źródła dźwieku
                 * do wyświetlanej pozycji slidera oraz wartości.
                 */
                if (TracksProperties.AudioFileReader.FileName == TracksProperties.SecAudioFileReader?.FileName
                    && TracksProperties.SelectedTrack?.Path == TracksProperties.AudioFileReader.FileName)
                {
                    var currentPosition = TracksProperties.AudioFileReader.CurrentTime.TotalSeconds;
                    var currentPositionSec = TracksProperties.SecAudioFileReader.CurrentTime.TotalSeconds;

                    if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing
                        && TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                    {
                        if (currentPosition > currentPositionSec)
                            TakeSecWave();
                        else
                            TakeFirstWave();
                    }
                    else if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                    {
                        TakeFirstWave();
                    }
                    else if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                    {
                        TakeSecWave();
                    }
                }
                /*
                 * Pozostałe możliwe opcje pobierające wartości z odpowiednich źródeł dźwięku.
                 */
                else if (TracksProperties.SelectedTrack?.Path == TracksProperties.AudioFileReader.FileName)
                {
                    TakeFirstWave();
                }
                else
                {
                    if (TracksProperties.SecAudioFileReader != null) TakeSecWave();
                }
            }

            /*
             * Dodatkowe sprawdzanie czy podany utwór przekroczył swój czas, wykonane na tej zasadzie ze względu
             * na minimalny błąd czasowy sprawdzania wartości. Dodatkowa funkcjonalność została wykonanana ze względu
             * na brak działania Eventu zatrzymania się muzyki przy włączonej opcji Nightcore.
             */
            var ts = new TimeSpan(0, 0, 0, 0, 20);
            if (TracksProperties.AudioFileReader != null)
            {
                ts += TracksProperties.AudioFileReader.CurrentTime;
                if (ts > TracksProperties.AudioFileReader.TotalTime) TracksProperties.WaveOut?.Stop();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Timer tick error: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// W momencie kliknięcia w dół przycisku myszki timer zostaje zatrzymany w celu zmiany wartości suwaka,
    /// aby ten automatycznie nie uciekał spod myszki
    /// </summary>
    private void TimeSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            TracksProperties.Timer.Stop();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Timer stop on mouseDown exception: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Event odpowiadający za sterowanie pozycją slidera, w którym wykorzystane zostały tak samo powyższe warunki
    /// wykorzystane w evencie Timer_Tick, z tym że ciało warunków zmienione zostało odpowiednią funkcją zmieniającą
    /// położenie slidera.
    /// </summary>
    private void TimeSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            var totalSeconds = _result.TotalSeconds;
            var progress = SldTime.Value / SldTime.Maximum;
            var newPosition = totalSeconds * progress;

            if (TracksProperties.AudioFileReader != null)
            {
                if (TracksProperties.AudioFileReader.FileName == TracksProperties.SecAudioFileReader?.FileName
                    && TracksProperties.SelectedTrack?.Path == TracksProperties.AudioFileReader.FileName)
                {
                    var currentPosition = TracksProperties.AudioFileReader.CurrentTime.TotalSeconds;
                    var currentPositionSec = TracksProperties.SecAudioFileReader.CurrentTime.TotalSeconds;

                    if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing
                        && TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                    {
                        if (currentPosition > currentPositionSec)
                            TracksProperties.SecAudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                        else
                            TracksProperties.AudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                    }
                    else if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                    {
                        TracksProperties.AudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                    }
                    else if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                    {
                        TracksProperties.SecAudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                    }
                }
                else if (TracksProperties.SelectedTrack?.Path == TracksProperties.AudioFileReader.FileName)
                {
                    TracksProperties.AudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                }
                else
                {
                    if (TracksProperties.SecAudioFileReader != null)
                        TracksProperties.SecAudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                }
            }

            TracksProperties.Timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Position change error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Metoda obsługująca zdarzenie Ctrl+Shift+strzałka w prawo lub w lewo (Ctrl+Shift+Right lub Ctrl+Shift+Left)
    /// na suwaku czasu odtwarzania utworu. Zmienia pozycję odtwarzania utworu w zależności od kierunku strzałki,
    /// przesuwając ją o 0.1 sekundy. Metoda wykorzystuje informacje o długości utworu oraz aktualnej pozycji suwaka,
    /// aby obliczyć nową pozycję odtwarzania utworu. Następnie metoda sprawdza, czy aktualnie odtwarzany utwór
    /// jest taki sam jak wybrany przez użytkownika i przesuwa pozycję odtwarzania odpowiedniego pliku audio.
    /// </summary>
    private void TimeSlider_CtrlShiftRight(object sender, EventArgs e)
    {
        try
        {
            var totalSeconds = _result.TotalSeconds;
            double progress;
            if (Keyboard.IsKeyDown(Key.Right) && SldTime.Value < SldTime.Maximum - 0.1)
                progress = (SldTime.Value + 0.1) / SldTime.Maximum;
            else if (Keyboard.IsKeyDown(Key.Left) && SldTime.Value > SldTime.Minimum + 0.1)
                progress = (SldTime.Value - 0.1) / SldTime.Maximum;
            else
                progress = SldTime.Value / SldTime.Maximum;

            var newPosition = totalSeconds * progress;

            if (TracksProperties.AudioFileReader != null)
            {
                if (TracksProperties.AudioFileReader.FileName == TracksProperties.SecAudioFileReader?.FileName
                    && TracksProperties.SelectedTrack?.Path == TracksProperties.AudioFileReader.FileName)
                {
                    var currentPosition = TracksProperties.AudioFileReader.CurrentTime.TotalSeconds;
                    var currentPositionSec = TracksProperties.SecAudioFileReader.CurrentTime.TotalSeconds;

                    if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing
                        && TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                    {
                        if (currentPosition > currentPositionSec)
                            TracksProperties.SecAudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                        else
                            TracksProperties.AudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                    }
                    else if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                    {
                        TracksProperties.AudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                    }
                    else if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                    {
                        TracksProperties.SecAudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                    }
                }
                else if (TracksProperties.SelectedTrack?.Path == TracksProperties.AudioFileReader.FileName)
                {
                    TracksProperties.AudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                }
                else
                {
                    if (TracksProperties.SecAudioFileReader != null)
                        TracksProperties.SecAudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                }
            }

            TracksProperties.Timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Position change error: {ex.Message}");
            throw;
        }
    }
}