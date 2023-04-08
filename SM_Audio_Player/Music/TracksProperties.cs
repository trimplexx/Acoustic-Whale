using NAudio.Wave;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace SM_Audio_Player.Music;

public static class TracksProperties
{

    // lista z utworami
    public static List<Tracks>? tracksList = new List<Tracks>();
    
    // wybrany aktualnie utwor
    public static Tracks? SelectedTrack { get; set; }

    //trwajacy aktualnie utwór
    public static AudioFileReader? audioFileReader { get; set; }
    public static WaveOutEvent? waveOut { get; set; }
    
    // Flagi przycisków
    public static bool isLoopOn = false;
    public static bool isSchuffleOn = false;
    // Lista poprzednich utworów do przycisku schuffle
    public static List<Tracks> PrevTrack = new List<Tracks>();
    // lista dostępnych numerów do wylosowania po użyciu schuffle
    public static List<int> availableNumbers { get; set; }
    // Pierwszy zagrany utwór, po użyciu schuffle
    public static Tracks firstPlayed { get; set; }
}