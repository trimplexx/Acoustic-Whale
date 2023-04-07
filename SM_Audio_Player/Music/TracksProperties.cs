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
    
    public static bool isLoopOn = false;
    public static bool isSchuffleOn = false;
    public static List<Tracks> PrevTrack = new List<Tracks>();
    public static List<int> availableNumbers { get; set; }
    public static Tracks firstPlayed { get; set; }

}