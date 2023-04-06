using NAudio.Wave;
using System.Collections;
using System.Collections.Generic;
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

}