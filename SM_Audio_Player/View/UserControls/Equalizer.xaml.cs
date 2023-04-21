using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SM_Audio_Player.Music;
using SM_Audio_Player.View.UserControls.buttons;

namespace SM_Audio_Player.View.UserControls;

public partial class Equalizer
{
    private EqualizerSampleProvider? _equalizer;
    private FadeInOutSampleProvider? _fadeOut;
    private FadeInOutSampleProvider? _fadeIn;
    private bool DoesTrackHaveFadeInOut;
    
    public delegate void FadeInEventHandler(object sender, EventArgs e);
    public static event FadeInEventHandler? FadeInEvent;
    
    public delegate void FadeOffOnEventHandler(object sender, EventArgs e);
    public static event FadeOffOnEventHandler? FadeOffOn;
    
    public Equalizer()
    {
        InitializeComponent();
        ButtonNext.NextButtonClicked += SetEqualizerValueToTrack;
        ButtonPlay.TrackEnd += SetEqualizerValueToTrack;
        ButtonPrevious.PreviousButtonClicked += SetEqualizerValueToTrack;
        Library.DoubleClickEvent += SetEqualizerValueToTrack;
        ButtonPlay.ButtonPlayEvent += SetEqualizerValueToTrack;
        Player.FirstToSec += FirstToSecChange;
        Player.SecToFirst += SecToFirstChange;
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
            else
            {
                TracksProperties.WaveOut?.Stop();
                _equalizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
                _equalizer?.UpdateEqualizer(0, 0, 0,0,0,0);
                TracksProperties.WaveOut.Init(_equalizer);
                TracksProperties.WaveOut?.Play();
            }
            /*
             * Inicjalizacja opcji FadeIn/Out na każdy odtworzony utwór
             */
            DoesTrackHaveFadeInOut = true;
            _fadeOut = new FadeInOutSampleProvider(TracksProperties.AudioFileReader);
            TracksProperties.WaveOut?.Stop();
            TracksProperties.WaveOut.Init(_fadeOut);
            TracksProperties.WaveOut?.Play();
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
            if (_equalizer == null)
            {
                TracksProperties.WaveOut?.Stop();
                _equalizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
                _equalizer.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value);
                TracksProperties.WaveOut.Init(_equalizer);
                TracksProperties.WaveOut?.Play();
            }
            
            if (Equalizer_box.IsChecked == true)
            {
                _equalizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value);
            }
            else
            {
                _equalizer?.UpdateEqualizer(0, 0, 0,0,0,0);
            }
        }
    }

    private void InitializeSecWave()
    {
        _fadeIn = new FadeInOutSampleProvider(TracksProperties.SecAudioFileReader);
        _fadeIn?.BeginFadeIn(11000);
        TracksProperties.SecWaveOut = new WaveOutEvent();
        TracksProperties.SecWaveOut.Init(TracksProperties.SecAudioFileReader);
        TracksProperties.SecWaveOut.Init(_fadeIn);
        TracksProperties.SecWaveOut.Play();
    }
    
    private void InitializeFirstWave()
    {
        _fadeOut = new FadeInOutSampleProvider(TracksProperties.AudioFileReader);
        _fadeOut?.BeginFadeIn(11000);
        TracksProperties.WaveOut = new WaveOutEvent();
        TracksProperties.WaveOut.Init(TracksProperties.AudioFileReader);
        TracksProperties.WaveOut.Init(_fadeOut);
        TracksProperties.WaveOut.Play();
    }
    
    private void FirstToSecChange(object sender, EventArgs e)
    {
        if (Fade_box.IsChecked == true)
        {
            _fadeOut?.BeginFadeOut(10000);

            if (TracksProperties.IsLoopOn == 2)
            {
                    TracksProperties.SecAudioFileReader = 
                        new AudioFileReader(TracksProperties.TracksList.ElementAt(TracksProperties.SelectedTrack.Id-1).Path);
                    TracksProperties.SelectedTrack = TracksProperties.TracksList.ElementAt(TracksProperties.SelectedTrack.Id-1);
                    InitializeSecWave();
            }
                // Sprawdzenie czy jest włączona opcja schuffle, ponieważ ona zmienia następny track
             else if (TracksProperties.IsSchuffleOn)
             {
                 if (TracksProperties.AvailableNumbers?.Count == 0)
                 {
                     TracksProperties.SelectedTrack = TracksProperties.TracksList.ElementAt(TracksProperties.FirstPlayed.Id-1);
                     
                     // Reset listy dostępnych numerów utworów
                     if (TracksProperties.TracksList != null)
                     {
                         TracksProperties.AvailableNumbers = Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                         Random random = new Random();
                         TracksProperties.AvailableNumbers =
                             TracksProperties.AvailableNumbers.OrderBy(x => random.Next()).ToList();
                     }
                     // Usunięcie numeru odtwarzanego utworu z listy, aby ten się nie powtórzył w momencie losowania
                     if (TracksProperties.SelectedTrack != null)
                         TracksProperties.AvailableNumbers.Remove(TracksProperties.SelectedTrack.Id - 1);
                     
                     // Wyczyszczenie listy poprzednich utworów
                     TracksProperties.PrevTrack.Clear();
                     
                     TracksProperties.SecAudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack?.Path);

                     if (TracksProperties.IsLoopOn == 1)
                     {
                         if (TracksProperties.SelectedTrack == TracksProperties.FirstPlayed)
                             if (TracksProperties.WaveOut != null)
                                 InitializeSecWave();
                     }
                     else
                     {
                         TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;
                         TracksProperties.WaveOut?.Stop();
                         TracksProperties.WaveOut?.Init(TracksProperties.AudioFileReader);
                         TracksProperties.SecWaveOut?.Stop();
                         TracksProperties.SecWaveOut?.Dispose();
                         TracksProperties.SecAudioFileReader = null;
                     }
                 }
                 else
                 {
                     TracksProperties.SecAudioFileReader = 
                         new AudioFileReader(TracksProperties.TracksList.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0)).Path);
                     TracksProperties.SelectedTrack = TracksProperties.TracksList.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0));
                     TracksProperties.AvailableNumbers.RemoveAt(0);
                     InitializeSecWave();
                 }
             }
             else
             {
                 if (TracksProperties.SelectedTrack?.Id == TracksProperties.TracksList?.Count)
                 {
                     // Przełączenie na 1 utwór z listy po zakończeniu ostatniego
                     TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(0);
                   
                     // Sprawdzenie czy jest loop, jeżeli tak to zatrzymaj muzykę i zmień ikone
                     if (TracksProperties.IsLoopOn == 1)
                     {
                         TracksProperties.SecAudioFileReader = 
                             new AudioFileReader(TracksProperties.TracksList.ElementAt(0).Path);
                         InitializeSecWave();
                     }
                 }
                 else
                 {
                     // Jeżeli track nie był ostatnim na liście odtwórz nastepny 
                     if (TracksProperties.SelectedTrack != null)
                     {
                         TracksProperties.SecAudioFileReader = 
                             new AudioFileReader(TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id).Path);
                         TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id);
                         InitializeSecWave();
                     }
                 }
             }
            FadeInEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SecToFirstChange(object sender, EventArgs e)
    {
        if (Fade_box.IsChecked == true)
        {
            _fadeIn?.BeginFadeOut(10000);

            if (TracksProperties.IsLoopOn == 2)
            {
                    TracksProperties.SecAudioFileReader = 
                        new AudioFileReader(TracksProperties.TracksList.ElementAt(TracksProperties.SelectedTrack.Id-1).Path);
                    TracksProperties.SelectedTrack = TracksProperties.TracksList.ElementAt(TracksProperties.SelectedTrack.Id-1);
                    InitializeFirstWave();
            }
                // Sprawdzenie czy jest włączona opcja schuffle, ponieważ ona zmienia następny track
             else if (TracksProperties.IsSchuffleOn)
             {
                 if (TracksProperties.AvailableNumbers?.Count == 0)
                 {
                     TracksProperties.SelectedTrack = TracksProperties.TracksList.ElementAt(TracksProperties.FirstPlayed.Id-1);
                     
                     // Reset listy dostępnych numerów utworów
                     if (TracksProperties.TracksList != null)
                     {
                         TracksProperties.AvailableNumbers = Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                         Random random = new Random();
                         TracksProperties.AvailableNumbers =
                             TracksProperties.AvailableNumbers.OrderBy(x => random.Next()).ToList();
                     }
                     // Usunięcie numeru odtwarzanego utworu z listy, aby ten się nie powtórzył w momencie losowania
                     if (TracksProperties.SelectedTrack != null)
                         TracksProperties.AvailableNumbers.Remove(TracksProperties.SelectedTrack.Id - 1);
                     
                     // Wyczyszczenie listy poprzednich utworów
                     TracksProperties.PrevTrack.Clear();
                     
                     TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack?.Path);

                     if (TracksProperties.IsLoopOn == 1)
                     {
                         if (TracksProperties.SelectedTrack == TracksProperties.FirstPlayed)
                             if (TracksProperties.WaveOut != null)
                                 InitializeFirstWave();
                     }
                     else
                     {
                         TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;
                         TracksProperties.WaveOut?.Stop();
                         TracksProperties.WaveOut?.Init(TracksProperties.AudioFileReader);
                         TracksProperties.SecWaveOut?.Stop();
                         TracksProperties.SecWaveOut?.Dispose();
                         TracksProperties.SecAudioFileReader = null;
                     }
                 }
                 else
                 {
                     TracksProperties.AudioFileReader = 
                         new AudioFileReader(TracksProperties.TracksList.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0)).Path);
                     TracksProperties.SelectedTrack = TracksProperties.TracksList.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0));
                     InitializeFirstWave();
                     TracksProperties.AvailableNumbers.RemoveAt(0);
                     
                 }
             }
             else
             {
                 if (TracksProperties.SelectedTrack?.Id == TracksProperties.TracksList?.Count)
                 {
                     // Przełączenie na 1 utwór z listy po zakończeniu ostatniego
                     TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(0);
                   
                     // Sprawdzenie czy jest loop, jeżeli tak to zatrzymaj muzykę i zmień ikone
                     if (TracksProperties.IsLoopOn == 1)
                     {
                         TracksProperties.AudioFileReader = 
                             new AudioFileReader(TracksProperties.TracksList.ElementAt(0).Path);
                         InitializeFirstWave();
                     }
                 }
                 else
                 {
                     // Jeżeli track nie był ostatnim na liście odtwórz nastepny 
                     if (TracksProperties.SelectedTrack != null)
                     {
                         TracksProperties.AudioFileReader = 
                             new AudioFileReader(TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id).Path);
                         TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id);
                         InitializeFirstWave();
                     }
                 }
             }
            FadeInEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    
    private void Fade_CheckBoxClick(object sender, RoutedEventArgs e)
    {
        if (Fade_box.IsChecked == true)
            TracksProperties.IsFadeOn = true;
        else
        {
            TracksProperties.IsFadeOn = false;
            if (TracksProperties.SecAudioFileReader != null)
            {
                TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut?.Init(TracksProperties.AudioFileReader);
                TracksProperties.SecWaveOut?.Stop();
                TracksProperties.SecWaveOut?.Dispose();
                TracksProperties.SecAudioFileReader = null;
                TracksProperties.WaveOut?.Play();
            }
            
        }
        FadeOffOn?.Invoke(this, EventArgs.Empty);
    }
    private void OnOffEffects(object sender, RoutedEventArgs e)
    {

    }
    private void OnOffFade(object sender, RoutedEventArgs e)
    {

    }
    private void OnOffDelay(object sender, RoutedEventArgs e)
    {

    }
    private void OnOffChoir(object sender, RoutedEventArgs e)
    {

    }
    private void OnOffDistortion(object sender, RoutedEventArgs e)
    {

    }
    private void OnOffNightcore(object sender, RoutedEventArgs e)
    {

    }
}



