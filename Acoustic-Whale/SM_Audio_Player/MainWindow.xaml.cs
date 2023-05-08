using System;
using System.Windows;
using System.Windows.Input;
using SM_Audio_Player.Music;

namespace SM_Audio_Player;

///<summary>
/// Klasa reprezentująca główne okno.
/// </summary>
public partial class MainWindow
{
    public delegate void PlayMusicBySpaceEvent(object sender, EventArgs e);
    public static event PlayMusicBySpaceEvent? PlayMusic;
    public delegate void AddTrackBySpaceEvent(object sender, EventArgs e);
    public static event AddTrackBySpaceEvent? AddTrack;
    public delegate void OnSchuffleEvent(object sender, EventArgs e);
    public static event OnSchuffleEvent? OnSchuffle;
    public delegate void OnLoopEvent(object sender, EventArgs e);
    public static event OnLoopEvent? OnLoop;
    public delegate void PrevTrackEvent(object sender, EventArgs e);
    public static event PrevTrackEvent? PrevTrack;
    public delegate void NextTrackEvent(object sender, EventArgs e);
    public static event NextTrackEvent? NextTrack;
    public delegate void ForwardSongEvent(object sender, EventArgs e);
    public static event ForwardSongEvent? ForwardSong;
    public delegate void RewindSongEvent(object sender, EventArgs e);
    public static event RewindSongEvent? RewindSong;
    public delegate void MuteSongEvent(object sender, EventArgs e);
    public static event MuteSongEvent? MuteSong;
    public delegate void VolumeChangedEventUp(object sender, EventArgs e);
    public static event VolumeChangedEventUp? VolumeChangeUp;
    
    public delegate void VolumeChangedEventDown(object sender, EventArgs e);
    public static event VolumeChangedEventDown? VolumeChangeDown;
    
    public MainWindow()
    {
        InitializeComponent();
        MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
        Focus();
    }

    /// <summary>
    /// Metoda Window_MouseDown obsługuje zdarzenie kliknięcia na pasek menu okna MainWindow.
    /// Metoda umożliwia poruszanie oknem za pomocą przeciągania myszą oraz przywrócenie okna z trybu
    /// pełnoekranowego do normalnego rozmiaru. Metoda pobiera pozycję kursora oraz ustawia nową pozycję okna,
    /// uwzględniając jego rozmiar i granice ekranu. Metoda obsługuje również wyjątki, wyświetlając
    /// komunikat z błędem i rzucając wyjątek dalej.
    /// </summary>
    private void Window_MouseDown(object sender, MouseEventArgs e)
    {
        try
        {
            /*Możliwość poruszania oknem*/
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();

            /*Możliwość poruszania oknem oraz przywrócenie go do normalnego rozmiaru z trybu pełnego okna*/
            if (e.LeftButton == MouseButtonState.Pressed &&
                Application.Current.MainWindow?.WindowState == WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                World.Margin = new Thickness(0);
                BtnMax.MaximizeIcon =
                    "M.3 89.5C.1 91.6 0 93.8 0 96V224 416c0 35.3 28.7 64 64 64l384 0c35.3 0 64-28.7 64-64V224 96c0-35.3-28.7-64-64-64H64c-2.2 0-4.4 .1-6.5 .3c-9.2 .9-17.8 3.8-25.5 8.2C21.8 46.5 13.4 55.1 7.7 65.5c-3.9 7.3-6.5 15.4-7.4 24zM48 224H464l0 192c0 8.8-7.2 16-16 16L64 432c-8.8 0-16-7.2-16-16l0-192z";

                /*Pobieranie pozycji kursora*/
                var mousePosition = Mouse.GetPosition(this);
                var newX = mousePosition.X - Width / 2;
                var newY = mousePosition.Y - Height / 2;

                /*Sprawdzenie czy okno wychodzi poza lewą krawędź ekranu*/
                if (newX < 0) newX = 0;

                /*Sprawdzenie czy okno wychodzi poza górną krawędź ekranu*/
                if (newY < 0) newY = 0;

                /*Sprawdzenie czy okno wychodzi poza prawą krawędź ekranu*/
                if (newX + Width > SystemParameters.PrimaryScreenWidth)
                    newX = SystemParameters.PrimaryScreenWidth - Width;

                /*Sprawdzenie czy okno wychodzi poza dolną krawędź ekranu*/
                if (newY + Height > SystemParameters.PrimaryScreenHeight)
                    newY = SystemParameters.PrimaryScreenHeight - Height;

                /*Przypisanie pozycji lewej i górnej granicy okna*/
                Left = newX;
                Top = newY;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Window_MouseDown exception: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Metoda Equalizer_Btn_Click obsługuje zdarzenie kliknięcia na przycisk "Equalizer_Btn" w oknie MainWindow.
    /// Metoda ukrywa panel biblioteki piosenek (właściwość Visibility ustawiana na Hidden) oraz wyświetla
    /// panel equalizera (właściwość Visibility ustawiana na Visible).
    /// </summary>
    private void Equalizer_Btn_Click(object sender, RoutedEventArgs e)
    {
        Lib.Visibility = Visibility.Hidden;
        Eq.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Metoda Playlist_Btn_Click obsługuje zdarzenie kliknięcia na przycisk "Playlist_Btn" w oknie MainWindow.
    /// Metoda ukrywa panel equalizera (właściwość Visibility ustawiana na Hidden) oraz wyświetla panel
    /// biblioteki piosenek (właściwość Visibility ustawiana na Visible).
    /// </summary>
    private void Playlist_Btn_Click(object sender, RoutedEventArgs e)
    {
        Eq.Visibility = Visibility.Hidden;
        Lib.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Metoda obsługuje zdarzenie naciśnięcia klawisza na klawiaturze w oknie MainWindow.
    /// Metoda wywołuje odpowiednie zdarzenia przy naciśnięciu kombinacji klawiszy Ctrl + Left/Right
    /// lub Ctrl + Shift + Left. Dla kombinacji Ctrl + Left uruchamia zdarzenie PrevTrack,
    /// dla kombinacji Ctrl + Right uruchamia zdarzenie NextTrack, a dla kombinacji Ctrl + Shift + Left
    /// uruchamia zdarzenie RewindSong i ustawia głośność na 0 dla odtwarzanych plików audio.
    /// </summary>
    private void KeyDown_event(object sender, KeyEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (e.Key == Key.Left)
                    PrevTrack?.Invoke(this, EventArgs.Empty);
            }
        }
        
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (e.Key == Key.Right)
                    NextTrack?.Invoke(this, EventArgs.Empty);
            }
        }
        
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (e.Key == Key.Left)
                {
                    RewindSong?.Invoke(this, EventArgs.Empty);
                    if (TracksProperties.AudioFileReader != null)
                        TracksProperties.AudioFileReader.Volume = 0;
                    if(TracksProperties.SecAudioFileReader != null)
                        TracksProperties.SecAudioFileReader.Volume = 0;
                }
            }
        }
        
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (e.Key == Key.Right)
                {
                    ForwardSong?.Invoke(this, EventArgs.Empty);
                    if (TracksProperties.AudioFileReader != null)
                        TracksProperties.AudioFileReader.Volume = 0;
                    if(TracksProperties.SecAudioFileReader != null)
                        TracksProperties.SecAudioFileReader.Volume = 0;
                }
                   
            }
            
            if (Keyboard.IsKeyDown(Key.F1))
            {
                MuteSong?.Invoke(this, EventArgs.Empty);
                if (TracksProperties.AudioFileReader != null)
                    TracksProperties.AudioFileReader.Volume = 0;
                if(TracksProperties.SecAudioFileReader != null)
                    TracksProperties.SecAudioFileReader.Volume = 0;
            }
            
            if (Keyboard.IsKeyDown(Key.F2))
            {
                VolumeChangeDown?.Invoke(this, EventArgs.Empty);
            }
            
            if (Keyboard.IsKeyDown(Key.F3))
            {
                VolumeChangeUp?.Invoke(this, EventArgs.Empty);
            }

            
           
        }
    }

    /// <summary>
    /// Metoda obsługująca naciśnięcie klawisza na klawiaturze w oknie MainWindow.
    /// Metoda reaguje na naciśnięcie kombinacji klawiszy Ctrl + Left/Right lub Ctrl + Shift + Left i wywołuje
    /// odpowiednie zdarzenia. Dla kombinacji Ctrl + Left uruchamia zdarzenie PrevTrack, dla kombinacji Ctrl + Right
    /// uruchamia zdarzenie NextTrack, a dla kombinacji Ctrl + Shift + Left uruchamia zdarzenie RewindSong i ustawia
    /// głośność na 0 dla odtwarzanych plików audio. Metoda korzysta z klasy Keyboard do sprawdzenia,
    /// czy klawisz Ctrl jest wciśnięty, oraz z klasy KeyEventArgs do sprawdzenia, który klawisz został naciśnięty.
    /// </summary>
    private void KeyUp_event(object sender, KeyEventArgs e)
    {
        if (Keyboard.IsKeyUp(Key.LeftCtrl))
        {
            if (Keyboard.IsKeyUp(Key.LeftShift))
            {

                if (TracksProperties.AudioFileReader != null)
                    TracksProperties.AudioFileReader.Volume = (float)TracksProperties.Volume/100;
                if(TracksProperties.SecAudioFileReader != null)
                    TracksProperties.SecAudioFileReader.Volume = (float)TracksProperties.Volume/100;
            }
        }
        
        if (e.Key == Key.Space)
        {
            TracksProperties.SpaceFlag = true;
            
            if (TracksProperties.SpaceFlag)
                PlayMusic?.Invoke(this, EventArgs.Empty);
            
            TracksProperties.SpaceFlag = true;
        }

        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            if(e.Key == Key.Insert)
                AddTrack?.Invoke(this, EventArgs.Empty);
        }

        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            if(e.Key == Key.S)
                OnSchuffle?.Invoke(this, EventArgs.Empty);
        }
        
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            if(e.Key == Key.L)
                OnLoop?.Invoke(this, EventArgs.Empty);
        }
        
    }

    /// <summary>
    /// Metoda obsługująca zdarzenie kliknięcia myszą na oknie MainWindow.
    /// Metoda ustawia flagę TracksProperties.SpaceFlag na wartość true, co oznacza, że przycisk spacji
    /// może zostać ponownie użyty do pauzowania/wznawiania odtwarzania piosenki. 
    /// </summary>
    private void FlagReset(object sender, MouseButtonEventArgs e)
    {
        TracksProperties.SpaceFlag = true;
    }

    /// <summary>
    /// Metoda Help_Btn_Click obsługuje zdarzenie kliknięcia na przycisk "Help_Btn" w oknie MainWindow.
    /// Metoda zmienia widoczność panelu pomocy Help, jeśli jest on widoczny, to ustawia jego widoczność na Hidden,
    /// a jeśli jest ukryty, to ustawia jego widoczność na Visible.
    /// </summary>
    private void Help_Btn_Click(object sender, RoutedEventArgs e)
    {
        if(Help.Visibility == Visibility.Visible)
        {
            Help.Visibility = Visibility.Hidden;
        }
        else
        {
            Help.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Metoda EscFromHelp obsługuje zdarzenie naciśnięcia klawisza na klawiaturze w oknie MainWindow,
    /// gdy panel pomocy Help jest widoczny. Metoda sprawdza, czy naciśnięty klawisz to klawisz Escape,
    /// jeśli tak, to ustawia widoczność panelu pomocy na Hidden.Metoda korzysta z klasy KeyEventArgs do sprawdzenia,
    /// który klawisz został naciśnięty, oraz z właściwości Visibility klasy FrameworkElement, aby
    /// zmienić widoczność panelu pomocy.
    /// </summary>
    private void EscFromHelp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (Help.Visibility == Visibility.Visible)
            {
                Help.Visibility = Visibility.Hidden;
            }
        }
    }
}