using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SM_Audio_Player.Music;
using SM_Audio_Player.View.UserControls.buttons;

namespace SM_Audio_Player.View.UserControls;

public partial class Player : INotifyPropertyChanged
{
    private const bool IsDraggingSlider = false;
    private readonly DispatcherTimer _timer = new();
    private string? _albumImg;

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
            // Przypisanie metody na tick timera
            _timer.Tick += Timer_Tick;
            // Usatwienie co ile odświeża sie timer
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 600);
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
        _timer.Stop();
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
    private void OnTrackSwitch(object sender, EventArgs e)
    {
        try
        {
            if (TracksProperties.SelectedTrack != null)
            {
                title.Text = TracksProperties.SelectedTrack.Title;
                author.Text = TracksProperties.SelectedTrack.Author;
                CD.Text = TracksProperties.SelectedTrack.Album;
                tbTime.Text = TracksProperties.SelectedTrack.Time;
                AlbumImg = TracksProperties.SelectedTrack.AlbumCoverPath;
            }

            sldTime.Value = 0;
            _timer.Start();
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
            if (TracksProperties.AudioFileReader != null)
            {
                var totalSeconds = TracksProperties.AudioFileReader.TotalTime.TotalSeconds;
                var currentPosition = TracksProperties.AudioFileReader.CurrentTime.TotalSeconds;
                var progress = currentPosition / totalSeconds;
                tbCurrTime.Text = TimeSpan.FromSeconds(currentPosition).ToString(@"hh\:mm\:ss");
                sldTime.Value = progress * sldTime.Maximum;
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
            _timer.Stop();
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
            if (TracksProperties.AudioFileReader != null && !IsDraggingSlider)
            {
                var totalSeconds = TracksProperties.AudioFileReader.TotalTime.TotalSeconds;
                var progress = sldTime.Value / sldTime.Maximum;
                var newPosition = totalSeconds * progress;
                TracksProperties.AudioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                _timer.Start();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Position change error: {ex.Message}");
            throw;
        }
    }
}