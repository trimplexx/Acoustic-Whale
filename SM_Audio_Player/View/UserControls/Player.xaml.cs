using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using NAudio.Wave;
using SM_Audio_Player.Music;
using SM_Audio_Player.View.UserControls.buttons;

namespace SM_Audio_Player.View.UserControls;

public partial class Player : INotifyPropertyChanged
{
    public delegate void FirstToSecEventHandler(object sender, EventArgs e);

    public delegate void SecToFirstEventHandler(object sender, EventArgs e);

    private const bool IsDraggingSlider = false;

    private string? _albumImg;
    private TimeSpan result;
    public static event FirstToSecEventHandler? FirstToSec;
    public static event SecToFirstEventHandler? SecToFirst;

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

            Equalizer.FadeInEvent += OnTrackSwitch;
            Equalizer.FadeOffOn += OnTrackSwitch;
            Library.OnDeleteTrack += OnTrackSwitch;

            // Przypisanie metody na tick timera
            TracksProperties._timer.Tick += Timer_Tick;
            // Usatwienie co ile odświeża sie timer
            TracksProperties._timer.Interval = new TimeSpan(0, 0, 0, 0, 600);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Player constructor exception: {ex.Message}");
            throw;
        }
    }

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

    private void ResetValues(object sender, EventArgs e)
    {
        TracksProperties._timer.Stop();
        sldTime.Value = 0;
        title.Text = "Title";
        author.Text = "Author";
        CD.Text = "Album";
        tbTime.Text = "0:00";
        tbCurrTime.Text = "0:00";
        AlbumImg = null;
    }

    /*
     * Ustawienie wszelkich wartości na temat piosenki przy jej zmianie oraz startowej wartości slidera
     */

    private void OnTrackSwitch(object sender, EventArgs e)
    {
        try
        {
            if (TracksProperties.SelectedTrack != null)
            {
                title.Text = TracksProperties.SelectedTrack.Title;
                author.Text = TracksProperties.SelectedTrack.Author;
                CD.Text = TracksProperties.SelectedTrack.Album;
                AlbumImg = TracksProperties.SelectedTrack.AlbumCoverPath;

                if (TracksProperties.IsFadeOn)
                {
                    if (TracksProperties.IsLoopOn == 2)
                    {
                        if (TracksProperties.AudioFileReader != null)
                        {
                            var totalFirstTime = TracksProperties.AudioFileReader.TotalTime.ToString(@"hh\:mm\:ss");
                            result = TimeSpan.Parse(totalFirstTime) - TimeSpan.FromSeconds(7);
                        }
                    }
                    else
                    {
                        if (TracksProperties.TracksList != null)
                        {
                            if (TracksProperties.SelectedTrack.Path == TracksProperties.AudioFileReader?.FileName)
                            {
                                result = TracksProperties.AudioFileReader.TotalTime - TimeSpan.FromSeconds(7);
                            }
                            else if(TracksProperties.SelectedTrack.Path == TracksProperties.SecAudioFileReader?.FileName)
                            {
                                result = TracksProperties.SecAudioFileReader.TotalTime - TimeSpan.FromSeconds(7);
                            }
                        }
                    }
                }
                else
                {
                    if (TracksProperties.AudioFileReader != null)
                    {
                        result = TracksProperties.AudioFileReader.TotalTime;
                    }
                }
                tbTime.Text = TracksProperties.SelectedTrack.Time;
                TracksProperties.SelectedTrack.Time = result.TotalHours >= 1 ? result.ToString(@"hh\:mm\:ss") : result.ToString(@"mm\:ss"); 
            }
            
            TracksProperties._timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OnTrackSwtich in Player class exception: {ex.Message}");
            throw;
        }
    }

    private void TakeFirstWave()
    {
        var totalSeconds = result.TotalSeconds;
        if (TracksProperties.AudioFileReader != null)
        {
            var currentPosition = TracksProperties.AudioFileReader.CurrentTime.TotalSeconds;
            var progress = currentPosition / totalSeconds;
            tbCurrTime.Text = currentPosition >= 3600 ? TimeSpan.FromSeconds(currentPosition).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture) : TimeSpan.FromSeconds(currentPosition).ToString(@"mm\:ss", CultureInfo.InvariantCulture);
            sldTime.Value = progress * sldTime.Maximum;
            if (currentPosition > totalSeconds) FirstToSec?.Invoke(this, EventArgs.Empty);
        }
    }

    private void TakeSecWave()
    {
        var totalSeconds = result.TotalSeconds;
        if (TracksProperties.SecAudioFileReader != null)
        {
            var currentPositionSec = TracksProperties.SecAudioFileReader.CurrentTime.TotalSeconds;

            var progressSec = currentPositionSec / totalSeconds;
            tbCurrTime.Text = currentPositionSec >= 3600 ? TimeSpan.FromSeconds(currentPositionSec).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture) : TimeSpan.FromSeconds(currentPositionSec).ToString(@"mm\:ss", CultureInfo.InvariantCulture);
            sldTime.Value = progressSec * sldTime.Maximum;
            if (currentPositionSec > totalSeconds) SecToFirst?.Invoke(this, EventArgs.Empty);
        }
    }

    /*
     * Metoda wywoływana na tick zegara w celu odświeżenia slidera o wartości sekundowe oraz licznika pokazującego
     * bieżący czas utworu
     */
    private void Timer_Tick(object? sender, EventArgs e)
    {
        try
        {
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
                else if (TracksProperties.SelectedTrack?.Path == TracksProperties.AudioFileReader?.FileName)
                {
                    if (TracksProperties.AudioFileReader != null) TakeFirstWave();
                }
                else
                {
                    if (TracksProperties.SecAudioFileReader != null) TakeSecWave();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Timer tick error: {ex.Message}");
            throw;
        }
    }

    /*
     * W momencie kliknięcia w dół przycisku myszki timer zostaje zatrzymany w celu zmiany wartości suwaka,
     * aby ten automatycznie nie uciekał spod myszki
     */
    private void TimeSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            TracksProperties._timer.Stop();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Timer stop on mouseDown exception: {ex.Message}");
            throw;
        }
    }

    /*
     * Gdy klawisz zostanie podniesiony ustanowiona zostanie nowa pozycja piosenki oraz wznowione odliczanie timera
    */
    private void TimeSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            var totalSeconds = result.TotalSeconds;
            var progress = sldTime.Value / sldTime.Maximum;
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

            TracksProperties._timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Position change error: {ex.Message}");
            throw;
        }
    }
}