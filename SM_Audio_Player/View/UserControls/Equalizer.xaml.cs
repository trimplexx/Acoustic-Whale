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
    private EqualizerSampleProvider? firstWaveEqualizer;
    private EqualizerSampleProvider? secWaveEqualizer;
    private FadeInOutSampleProvider? firstWaveFade;
    private FadeInOutSampleProvider? secWaveFade;

    public delegate void FadeInEventHandler(object sender, EventArgs e);
    public static event FadeInEventHandler? FadeInEvent;
    public delegate void FadeOffOnEventHandler(object sender, EventArgs e);
    public static event FadeOffOnEventHandler? FadeOffOn;
    
    public Equalizer()
    {
        InitializeComponent();
        ButtonNext.NextButtonClicked += NextPrevEvent;
        ButtonPlay.TrackEnd += NextPrevEvent;
        ButtonPrevious.PreviousButtonClicked += NextPrevEvent;
        Library.DoubleClickEvent += ButtonDoubleClickEvent;
        Player.FirstToSec += FirstToSecChange;
        Player.SecToFirst += SecToFirstChange;
        ButtonPlay.ButtonPlayEvent += ButtonDoubleClickEvent;
        ButtonNext.NextSelectedNull += ButtonDoubleClickEvent;
    }

    
    /*
     * Ustawienie wartości bieżących suwaków, w momencie gdy jest zaznaczona opcja equalizera w odpowiedzi na zmiane
     * piosenki.
     */
    private void ButtonDoubleClickEvent(object sender, EventArgs e)
    {
        try
        {
            if (TracksProperties.AudioFileReader != null)
                firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
            if(Equalizer_box.IsChecked == true)
                firstWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value, sld7.Value, sld8.Value);
            else 
                firstWaveEqualizer?.UpdateEqualizer(0,0,0,0,0,0,0,0);

            firstWaveFade = new FadeInOutSampleProvider(firstWaveEqualizer);
            
            TracksProperties.WaveOut?.Stop();
            TracksProperties.WaveOut.Init(firstWaveFade);
            TracksProperties.WaveOut?.Play();
            FadeOffOn?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"CreateEqualizers: {ex.Message}");
            throw;
        }
    }

    private void NextPrevEvent(object sender, EventArgs e)
    {
        try
        {
            if (TracksProperties.AudioFileReader != null)
                firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
            if(Equalizer_box.IsChecked == true)
                firstWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value, sld7.Value, sld8.Value);
            else 
                firstWaveEqualizer?.UpdateEqualizer(0,0,0,0,0,0,0,0);

            firstWaveFade = new FadeInOutSampleProvider(firstWaveEqualizer);

            if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
            {
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut.Init(firstWaveFade);
                TracksProperties.WaveOut?.Play();
            }
            else
            {
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut.Init(firstWaveFade);
            }

            FadeOffOn?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"CreateEqualizers: {ex.Message}");
            throw;
        }
    }

    

    private void ChangeEqualizerValues()
    {
        if (Equalizer_box.IsChecked == true)
        {
            firstWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value, sld7.Value, sld8.Value);
            secWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value, sld7.Value, sld8.Value);
        }
        else
        {
            firstWaveEqualizer?.UpdateEqualizer(0,0,0,0,0,0,0,0);
            secWaveEqualizer?.UpdateEqualizer(0,0,0,0,0,0,0,0);
        }
    }
    /*
     * Zmiana wartości pasm w momencie puszczenia klawiszu myszki na sliderze.
     */
    private void OnSliderValueChange(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
        ChangeEqualizerValues();
    }

    /*
     * Włączenie bądź wyłączenie equalizera za pomocą check boxa.
     */
    private void OnOffEqualizer(object sender, RoutedEventArgs e)
    {
        ChangeEqualizerValues();
    }

    private void InitializeSecWave()
    {
        try
        {
            if (TracksProperties.SecAudioFileReader != null)
                secWaveEqualizer = new EqualizerSampleProvider(TracksProperties.SecAudioFileReader);
            ChangeEqualizerValues();
            
            secWaveFade = new FadeInOutSampleProvider(secWaveEqualizer);
                
            TracksProperties.SecWaveOut?.Stop();
                
            if (TracksProperties.SecWaveOut == null)
                TracksProperties.SecWaveOut = new WaveOutEvent();
                
            TracksProperties.SecWaveOut.Init(secWaveFade);
            
            secWaveFade?.BeginFadeIn(11000);
            TracksProperties.SecWaveOut?.Play();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"InitializeSecWave: {ex.Message}");
            throw;
        }
        
    }
    
    private void InitializeFirstWave()
    {
        try
        {
            if (TracksProperties.AudioFileReader != null)
                firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
            ChangeEqualizerValues();
                
            firstWaveFade= new FadeInOutSampleProvider(firstWaveEqualizer, true);
                
            TracksProperties.WaveOut?.Stop();

            TracksProperties.WaveOut.Init(firstWaveFade);
            
            firstWaveFade?.BeginFadeIn(11000);
            TracksProperties.WaveOut?.Play();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"InitializeFirstWave: {ex.Message}");
            throw;
        }
    }
    
    private void FirstToSecChange(object sender, EventArgs e)
    {
        try
        {
            if (Fade_box.IsChecked == true)
            {
                firstWaveFade?.BeginFadeOut(10000);

                if (TracksProperties.IsLoopOn == 2)
                {
                    TracksProperties.SecAudioFileReader = new AudioFileReader(TracksProperties.TracksList
                        .ElementAt(TracksProperties.SelectedTrack.Id - 1).Path);
                    TracksProperties.SelectedTrack =
                        TracksProperties.TracksList.ElementAt(TracksProperties.SelectedTrack.Id - 1);
                    InitializeSecWave();
                }
                // Sprawdzenie czy jest włączona opcja schuffle, ponieważ ona zmienia następny track
                else if (TracksProperties.IsSchuffleOn)
                {
                    if (TracksProperties.AvailableNumbers?.Count == 0)
                    {
                        TracksProperties.SelectedTrack =
                            TracksProperties.TracksList.ElementAt(TracksProperties.FirstPlayed.Id - 1);

                        // Reset listy dostępnych numerów utworów
                        if (TracksProperties.TracksList != null)
                        {
                            TracksProperties.AvailableNumbers =
                                Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                            Random random = new Random();
                            TracksProperties.AvailableNumbers =
                                TracksProperties.AvailableNumbers.OrderBy(x => random.Next()).ToList();
                        }

                        // Usunięcie numeru odtwarzanego utworu z listy, aby ten się nie powtórzył w momencie losowania
                        if (TracksProperties.SelectedTrack != null)
                            TracksProperties.AvailableNumbers.Remove(TracksProperties.SelectedTrack.Id - 1);

                        // Wyczyszczenie listy poprzednich utworów
                        TracksProperties.PrevTrack.Clear();

                        if (TracksProperties.IsLoopOn == 1)
                        {
                            TracksProperties.SecAudioFileReader =
                                new AudioFileReader(TracksProperties.SelectedTrack?.Path);
                            if (TracksProperties.SelectedTrack == TracksProperties.FirstPlayed)
                                if (TracksProperties.WaveOut != null)
                                    InitializeSecWave();
                        }
                        else
                        {
                            TracksProperties.SecWaveOut?.Stop();
                            TracksProperties.WaveOut?.Stop();
                            TracksProperties.SecAudioFileReader = null;
                            TracksProperties.AudioFileReader = null;
                            TracksProperties._timer.Stop();
                        }
                    }
                    else
                    {
                        TracksProperties.SecAudioFileReader =
                            new AudioFileReader(TracksProperties.TracksList
                                .ElementAt(TracksProperties.AvailableNumbers.ElementAt(0)).Path);
                        TracksProperties.PrevTrack.Add(TracksProperties.SelectedTrack);
                        TracksProperties.SelectedTrack =
                            TracksProperties.TracksList.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0));
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
                        else
                        {
                            TracksProperties.SecWaveOut?.Stop();
                            TracksProperties.WaveOut?.Stop();
                            TracksProperties.SecAudioFileReader = null;
                            TracksProperties.AudioFileReader = null;
                            TracksProperties._timer.Stop();
                        }
                    }
                    else
                    {
                        // Jeżeli track nie był ostatnim na liście odtwórz nastepny 
                        if (TracksProperties.SelectedTrack != null)
                        {
                            TracksProperties.SecAudioFileReader =
                                new AudioFileReader(TracksProperties.TracksList
                                    ?.ElementAt(TracksProperties.SelectedTrack.Id).Path);
                            TracksProperties.SelectedTrack =
                                TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id);
                            InitializeSecWave();
                        }
                    }
                }

                FadeInEvent?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"FirstToSecChange: {ex.Message}");
            throw;
        }
    }

    private void SecToFirstChange(object sender, EventArgs e)
    {
        try
        {
        if (Fade_box.IsChecked == true)
        {
            secWaveFade?.BeginFadeOut(10000);

            if (TracksProperties.IsLoopOn == 2)
            {
                TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.TracksList.ElementAt(TracksProperties.SelectedTrack.Id-1).Path);
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
                     
                     

                     if (TracksProperties.IsLoopOn == 1)
                     {
                         TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.SelectedTrack?.Path);
                         if (TracksProperties.SelectedTrack == TracksProperties.FirstPlayed)
                             if (TracksProperties.WaveOut != null)
                                 InitializeFirstWave();
                     }
                     else
                     {
                         TracksProperties.SecWaveOut?.Stop();
                         TracksProperties.WaveOut?.Stop();
                         TracksProperties.SecAudioFileReader = null;
                         TracksProperties.AudioFileReader = null;
                         TracksProperties._timer.Stop();
                     }
                 }
                 else
                 {
                     TracksProperties.AudioFileReader = 
                         new AudioFileReader(TracksProperties.TracksList.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0)).Path);
                     TracksProperties.PrevTrack.Add(TracksProperties.SelectedTrack);
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
                     else
                     {
                         TracksProperties._timer.Stop();
                         TracksProperties.SecWaveOut?.Stop();
                         TracksProperties.WaveOut?.Stop();
                         TracksProperties.SecAudioFileReader = null;
                         TracksProperties.AudioFileReader = null;
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
        catch (Exception ex)
        {
            MessageBox.Show($"SecToFirstChange: {ex.Message}");
            throw;
        }
    }

    
    private void Fade_CheckBoxClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Fade_box.IsChecked == true)
            {
                TracksProperties.IsFadeOn = true;
            }
                
            else
            {
                TracksProperties.IsFadeOn = false;
                bool isPlaying = false;

                if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                    isPlaying = true;
                else if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                    isPlaying = true;
                
                if(TracksProperties.SecAudioFileReader != null)
                    TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;
                
                TracksProperties._timer.Stop();
                TracksProperties.SecWaveOut?.Stop();

                if (TracksProperties.AudioFileReader != null)
                {
                    firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
                    if(Equalizer_box.IsChecked == true)
                        firstWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value, sld7.Value, sld8.Value);
                    else 
                        firstWaveEqualizer?.UpdateEqualizer(0,0,0,0,0,0,0,0);

                    firstWaveFade = new FadeInOutSampleProvider(firstWaveEqualizer);
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(firstWaveFade);
                    
                    if(isPlaying)
                        TracksProperties.WaveOut?.Play();
                }
            }
            FadeOffOn?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fade_CheckBoxClick: {ex.Message}");
            throw;
        }
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



