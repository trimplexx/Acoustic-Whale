using NAudio.Wave;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SM_Audio_Player.Music;

public static class TracksProperties
{

    // list of tracks
    public static List<Tracks>? tracksList = new List<Tracks>();


    // selected track
    public static Tracks? SelectedTrack { get; set; }

    //file of currently playing track
    public static AudioFileReader? audioFileReader { get; set; }

    //wave of currently playing track
    public static WaveOutEvent waveOut = new WaveOutEvent();

}