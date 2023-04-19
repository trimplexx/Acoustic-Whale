using System.Collections.Generic;
using NAudio.Wave;

namespace SM_Audio_Player.Music;

/*
 * Klasa statyczna TrackProperties w celu przechowywania zmienych, do których odwołać można się w całym projekcie
 */
public static class TracksProperties
{
    // Lista z utworami
    public static List<Tracks>? TracksList = new();

    // Wybrany aktualnie utwor z listy
    public static Tracks? SelectedTrack { get; set; }

    // Trwający aktualnie utwór
    public static AudioFileReader? AudioFileReader { get; set; }
    public static WaveOutEvent? WaveOut { get; set; }

    public static string? AlbumImg { get; set; }

    // Flagi przycisków Loop oraz Schuffle
    public static int IsLoopOn = 0;
    public static bool IsSchuffleOn = false;

    // Lista poprzednich utworów do przycisku schuffle
    public static List<Tracks?> PrevTrack = new();

    // lista dostępnych numerów do wylosowania po użyciu schuffle
    public static List<int>? AvailableNumbers { get; set; }

    // Pierwszy zagrany utwór, po użyciu schuffle
    public static Tracks? FirstPlayed { get; set; }
}