using System.Collections.Generic;
using System.Windows.Threading;
using NAudio.Wave;

namespace SM_Audio_Player.Music;

/// <summary>
/// Klasa statyczna TrackProperties w celu przechowywania zmienych, do których odwołać można się w całym projekcie
/// </summary>
public static class TracksProperties
{
    /// <summary>
    /// Flaga kliknięcia spacji utworzona na potrzeby pauzowania/ odtwarzania muzyki aby metody nie wykonywały się
    /// kilka razy
    /// </summary>
    public static bool SpaceFlag = true;

    /// <summary>
    /// Zmienna przechowująca obecną wartość głośności granej muzyki.
    /// </summary>
    public static double Volume;

    /// <summary>
    /// Lista z utworami
    /// </summary>
    public static List<Tracks>? TracksList = new();

    /// <summary>
    /// Lista poprzednich utworów do przycisku schuffle
    /// </summary>
    public static List<Tracks?> PrevTrack = new();

    /// <summary>
    /// Timer utworu wykorzystywany do przypisania mu metody 'TimerTick' zmieniający pozycję utworu na sliderze.
    /// </summary>
    public static DispatcherTimer Timer = new();

    // Flagi przycisków Loop oraz Schuffle
    /// <summary>
    /// Flaga wybranej opcji loop.
    /// <code>
    /// 0 - Brak funkcji loopa 
    /// 1 - Loop całej playlisty
    /// 2 - Loop pojedyńczego utworu
    /// </code>
    /// </summary>
    public static int IsLoopOn = 0;

    /// <summary>
    /// Flaga trybu Schuffle.
    /// <code>
    /// false - tryb schuffle wyłączony 
    /// true - tryb schuffle włączony
    /// </code>
    /// </summary>
    public static bool IsSchuffleOn = false;

    /// <summary>
    /// Flaga opcji Fade in/out.
    /// <code>
    /// false - opcja Fade in/out wyłączona 
    /// true - opcja Fade in/out włączona
    /// </code>
    /// </summary>
    public static bool IsFadeOn = false;

    /// <summary>
    /// Obiekt klasy Tracks przechowujący obecnie wybrany utwór przez użytkownika.
    /// </summary>
    public static Tracks? SelectedTrack { get; set; }

    /// <summary>
    /// Główny obiekt klasy AudioFileReader, z którego źródła obecnie odtwarzana jest grana muzyka.
    /// </summary>
    /// 
    public static AudioFileReader? AudioFileReader { get; set; }

    /// <summary>
    /// Główny obiekt klasy WaveOutEvent, na którym wykonywane są wszelkie operację takie jak pauza/ start
    /// grania muzyki.
    /// </summary>
    public static WaveOutEvent? WaveOut { get; set; }

    /// <summary>
    /// Drugi obiekt klasy AudioFileReader, z którego odczytywana jest muzyka w momencie użycia opcji Fade in/out, aby
    /// uzyskać nałożenie się na siebie utworów. 
    /// </summary>
    public static AudioFileReader? SecAudioFileReader { get; set; }

    /// <summary>
    /// Drugi obiekt klasy WaveOutEvent, z którego odtwarzana jest muzyka w momencie użycia opcji Fade in/out, aby
    /// uzyskać nałożenie się na siebie utworów. 
    /// </summary>
    public static WaveOutEvent? SecWaveOut { get; set; }

    /// <summary>
    /// Lista dostępnych numerów do wylosowania po użyciu funkcji schuffle w playerze.
    /// </summary>
    public static List<int>? AvailableNumbers { get; set; }

    /// <summary>
    /// Obiekt klasy Tracks, służący do zapamiętania pierwszego zagranego utworu, w momencie włączenia
    /// funkcji schuffle przez użytkownika.
    /// </summary>
    public static Tracks? FirstPlayed { get; set; }
}