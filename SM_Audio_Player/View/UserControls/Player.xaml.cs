using System;
using System.ComponentModel;
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
    private const bool IsDraggingSlider = false;
    
    private string? _albumImg;
    public delegate void FirstToSecEventHandler(object sender, EventArgs e);
    public static event FirstToSecEventHandler? FirstToSec;
    
    public delegate void SecToFirstEventHandler(object sender, EventArgs e);
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
            
            Equalizer.FadeInEvent +=OnTrackSwitch;
            Equalizer.FadeOffOn += OnTrackSwitch;

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
        title.Text = "Tytuł";
        author.Text = "Autor";
        CD.Text = "Płyta";
        tbTime.Text = "hh:mm:ss";
        tbCurrTime.Text = "hh:mm:ss";
        AlbumImg = null;
    }

    /*
     * Ustawienie wszelkich wartości na temat piosenki przy jej zmianie oraz startowej wartości slidera
     */
    private TimeSpan result;
    private int id;
    private void OnTrackSwitch(object sender, EventArgs e)
    {
        try
        {
            if (TracksProperties.SelectedTrack != null)
            {
                title.Text = TracksProperties.SelectedTrack.Title;
                author.Text = TracksProperties.SelectedTrack.Author;
                CD.Text = TracksProperties.SelectedTrack.Album;
                if (TracksProperties.IsFadeOn)
                {
                    TimeSpan totalTime = TimeSpan.Parse(TracksProperties.SelectedTrack.Time);
                    result = totalTime - TimeSpan.FromSeconds(10);
                    TracksProperties.SelectedTrack.Time = result.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    if (TracksProperties.AudioFileReader != null)
                    {
                        result = TracksProperties.AudioFileReader.TotalTime;
                        TracksProperties.SelectedTrack.Time =
                            TracksProperties.AudioFileReader.TotalTime.ToString(@"hh\:mm\:ss");
                    }
                }
                tbTime.Text = TracksProperties.SelectedTrack.Time;
                //sldTime.Maximum = Double.Parse(TracksProperties.SelectedTrack.Time);
                AlbumImg = TracksProperties.SelectedTrack.AlbumCoverPath;
            }

            sldTime.Value = 0;
            TracksProperties._timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OnTrackSwtich in Player class exception: {ex.Message}");
            throw;
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
            var totalSeconds = result.TotalSeconds;

            if (TracksProperties.AudioFileReader != null)
            {
                var totalSecondsFirst = TracksProperties.AudioFileReader.TotalTime - TimeSpan.FromSeconds(10);
            
                if (TracksProperties.SelectedTrack?.Time == TracksProperties.AudioFileReader.TotalTime.ToString(@"hh\:mm\:ss") || result.ToString() == totalSecondsFirst.ToString(@"hh\:mm\:ss"))
                {
                    var currentPosition = TracksProperties.AudioFileReader.CurrentTime.TotalSeconds;
                    var progress = currentPosition / totalSeconds;
                    tbCurrTime.Text = TimeSpan.FromSeconds(currentPosition).ToString(@"hh\:mm\:ss");
                    sldTime.Value = progress * sldTime.Maximum;
                    if (currentPosition > totalSeconds)
                    {
                        FirstToSec?.Invoke(this, EventArgs.Empty);
                    }
                
                }
                else
                {
                    if (TracksProperties.SecAudioFileReader != null)
                    {
                        var currentPositionSec = TracksProperties.SecAudioFileReader.CurrentTime.TotalSeconds;
                        var progressSec = currentPositionSec / totalSeconds;
                        tbCurrTime.Text = TimeSpan.FromSeconds(currentPositionSec).ToString(@"hh\:mm\:ss");
                        sldTime.Value = progressSec * sldTime.Maximum;
                        if (currentPositionSec > totalSeconds)
                        {
                            SecToFirst?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Timer error: {ex.Message}");
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
            if (TracksProperties.AudioFileReader != null)
            {
                var progress = sldTime.Value / sldTime.Maximum;
                var newPosition = totalSeconds * progress;
                
                var totalSecondsFirst = TracksProperties.AudioFileReader.TotalTime - TimeSpan.FromSeconds(10);

                if (TracksProperties.SelectedTrack?.Time ==
                    TracksProperties.AudioFileReader.TotalTime.ToString(@"hh\:mm\:ss") ||
                    result.ToString() == totalSecondsFirst.ToString(@"hh\:mm\:ss"))
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