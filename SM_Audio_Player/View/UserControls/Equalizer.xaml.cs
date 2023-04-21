using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NAudio.Dsp;
using NAudio.Wave;
using SM_Audio_Player.Music;
using SM_Audio_Player.View.UserControls.buttons;

namespace SM_Audio_Player.View.UserControls;

public partial class Equalizer
{
    private EqualizerSampleProvider? _equalizer;
    public Equalizer()
    {
        InitializeComponent();
        ButtonNext.NextButtonClicked += SetEqualizerValueToTrack;
        ButtonPlay.TrackEnd += SetEqualizerValueToTrack;
        ButtonPrevious.PreviousButtonClicked += SetEqualizerValueToTrack;
        Library.DoubleClickEvent += SetEqualizerValueToTrack;
        ButtonPlay.ButtonPlayEvent += SetEqualizerValueToTrack;
    }

    /*
     * Ustawienie wartości bieżących suwaków, w momencie gdy jest zaznaczona opcja equalizera w odpowiedzi na zmiane
     * piosenki.
     */
    private void SetEqualizerValueToTrack(object sender, EventArgs e)
    {
        if (TracksProperties.WaveOut != null && TracksProperties.AudioFileReader != null && TracksProperties.WaveOut.PlaybackState == PlaybackState.Playing)
        {
            if (Equalizer_box.IsChecked == true)
            {
                TracksProperties.WaveOut?.Stop();
                _equalizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
                _equalizer.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value);
                TracksProperties.WaveOut.Init(_equalizer);
                TracksProperties.WaveOut?.Play();
            }
        }
    }

    /*
     * Zmiana wartości pasm w momencie puszczenia klawiszu myszki na sliderze.
     */
    private void OnSliderValueChange(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
        if (Equalizer_box.IsChecked == true)
        {
            _equalizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value);
        }
    }

    /*
     * Włączenie bądź wyłączenie equalizera za pomocą check boxa.
     */
    private void OnOffEqualizer(object sender, RoutedEventArgs e)
    {
        if (TracksProperties.WaveOut != null && TracksProperties.AudioFileReader != null && TracksProperties.WaveOut.PlaybackState == PlaybackState.Playing)
        {
            if (Equalizer_box.IsChecked == true)
            {
                TracksProperties.WaveOut?.Stop();
                _equalizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
                _equalizer.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value);
                TracksProperties.WaveOut.Init(_equalizer);
            }
            else
            {
                TracksProperties.WaveOut?.Stop();
                _equalizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
                TracksProperties.WaveOut.Init(_equalizer);
            }

            TracksProperties.WaveOut?.Play();
        }
    }
}


