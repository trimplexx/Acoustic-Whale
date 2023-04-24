using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SM_Audio_Player.Music;
using SM_Audio_Player.View.UserControls.buttons;
using VarispeedDemo.SoundTouch;

namespace SM_Audio_Player.View.UserControls;

public partial class Equalizer
{
    public delegate void FadeInEventHandler(object sender, EventArgs e);

    public delegate void FadeOffOnEventHandler(object sender, EventArgs e);

    private EqualizerSampleProvider? _firstWaveEqualizer;
    private FadeInOutSampleProvider? _firstWaveFade;
    private EqualizerSampleProvider? _secWaveEqualizer;
    private FadeInOutSampleProvider? _secWaveFade;

    public Equalizer()
    {
        InitializeComponent();
        ButtonNext.NextButtonClicked += NextPrevEvent;
        ButtonPlay.TrackEnd += NextPrevEvent;
        ButtonPrevious.PreviousButtonClicked += ButtonDoubleClickEvent;
        Library.DoubleClickEvent += ButtonDoubleClickEvent;
        Player.FirstToSec += FirstToSecChange;
        Player.SecToFirst += SecToFirstChange;
        ButtonPlay.ButtonPlayEvent += ButtonDoubleClickEvent;
        ButtonNext.NextSelectedNull += ButtonDoubleClickEvent;
    }

    public static event FadeInEventHandler? FadeInEvent;
    public static event FadeOffOnEventHandler? FadeOffOn;

    private void InitNightcoreEffect()
    {
        if (Nightcore_Box.IsChecked == true)
        {
            if (_firstWaveFade != null)
            {
                var firstspeedControl = new VarispeedSampleProvider(_firstWaveFade, 100, new SoundTouchProfile(false, false));
                firstspeedControl.PlaybackRate = 1.4f;;
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut?.Init(firstspeedControl);
            }
        }
    }

    private void InitDelayEffect()
    {
        if (Delay_Box.IsChecked == true)
        {
            if (_firstWaveFade != null)
            {
                var firstDelayEffect = new DelayEffect(_firstWaveFade, 300, 0.7f);
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut?.Init(firstDelayEffect);
            }
        }
    }

    private void InitChorusEffect()
    {
        if (Chorus_Box.IsChecked == true)
        {
            if (_firstWaveFade != null)
            {
                var firstChorusEffect = new ChorusEffect(_firstWaveFade, 200, 0.4f);
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut?.Init(firstChorusEffect);
            }
        }
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
                _firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
            if (Equalizer_box.IsChecked == true)
                _firstWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value,
                    sld6.Value, sld7.Value, sld8.Value);
            else
                _firstWaveEqualizer?.UpdateEqualizer(0, 0, 0, 0, 0, 0, 0, 0);

            _firstWaveFade = new FadeInOutSampleProvider(_firstWaveEqualizer);
            TracksProperties.WaveOut?.Stop();
             TracksProperties.WaveOut.Init(_firstWaveFade);
            
            InitNightcoreEffect();
            InitDelayEffect();
            InitChorusEffect();
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
                _firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
            if (Equalizer_box.IsChecked == true)
                _firstWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value,
                    sld6.Value, sld7.Value, sld8.Value);
            else
                _firstWaveEqualizer?.UpdateEqualizer(0, 0, 0, 0, 0, 0, 0, 0);

            _firstWaveFade = new FadeInOutSampleProvider(_firstWaveEqualizer);
            TracksProperties.WaveOut?.Stop();
            TracksProperties.WaveOut.Init(_firstWaveFade);
            
            if (TracksProperties.SelectedTrack == TracksProperties.TracksList?.ElementAt(0) &&
                !TracksProperties.IsSchuffleOn && TracksProperties.IsLoopOn != 1)
            {
                InitNightcoreEffect();
                InitDelayEffect();
                InitChorusEffect();
            }
            else if (TracksProperties.SelectedTrack?.Path == TracksProperties.FirstPlayed?.Path &&
                     TracksProperties.IsSchuffleOn && TracksProperties.IsLoopOn != 1)
            {
                InitNightcoreEffect();
                InitDelayEffect();
                InitChorusEffect();
            }
            else
            {
                InitNightcoreEffect();
                InitDelayEffect();
                InitChorusEffect();
                TracksProperties.WaveOut?.Play();
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
            _firstWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value,
                sld7.Value, sld8.Value);
            _secWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value, sld6.Value,
                sld7.Value, sld8.Value);
        }
        else
        {
            _firstWaveEqualizer?.UpdateEqualizer(0, 0, 0, 0, 0, 0, 0, 0);
            _secWaveEqualizer?.UpdateEqualizer(0, 0, 0, 0, 0, 0, 0, 0);
        }

        Slider_Value0.Text = ((int)sld1.Value).ToString();
        Slider_Value1.Text = ((int)sld2.Value).ToString();
        Slider_Value2.Text = ((int)sld3.Value).ToString();
        Slider_Value3.Text = ((int)sld4.Value).ToString();
        Slider_Value4.Text = ((int)sld5.Value).ToString();
        Slider_Value5.Text = ((int)sld6.Value).ToString();
        Slider_Value6.Text = ((int)sld7.Value).ToString();
        Slider_Value7.Text = ((int)sld8.Value).ToString();
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
                _secWaveEqualizer = new EqualizerSampleProvider(TracksProperties.SecAudioFileReader);
            ChangeEqualizerValues();

            _secWaveFade = new FadeInOutSampleProvider(_secWaveEqualizer);
            _secWaveFade?.BeginFadeIn(6000);
            
            TracksProperties.SecWaveOut?.Stop();

            if (TracksProperties.SecWaveOut == null)
                TracksProperties.SecWaveOut = new WaveOutEvent();

            if (Nightcore_Box.IsChecked == true)
            {
                if (_secWaveFade != null)
                {
                    var secSpeedControl = new VarispeedSampleProvider(_secWaveFade, 100, new SoundTouchProfile(false, false));
                    secSpeedControl.PlaybackRate = 1.4f;;
                    TracksProperties.SecWaveOut.Init(secSpeedControl);
                }
            }
            else if (Delay_Box.IsChecked == true)
            {
                if (_secWaveFade != null)
                {
                    var secDelayEffect = new DelayEffect(_secWaveFade, 300, 0.5f);
                    TracksProperties.SecWaveOut.Init(secDelayEffect);
                }
            }
            else if (Chorus_Box.IsChecked == true)
            {
                if (_secWaveFade != null)
                {
                    var secChorusEffect = new ChorusEffect(_secWaveFade, 160, 0.4f);
                    TracksProperties.SecWaveOut.Init(secChorusEffect);
                }
            }
            else
            {
                TracksProperties.SecWaveOut.Init(_secWaveFade);
            }
            
            TracksProperties.SecWaveOut.Play();
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
                _firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
            ChangeEqualizerValues();

            _firstWaveFade = new FadeInOutSampleProvider(_firstWaveEqualizer, true);
            _firstWaveFade?.BeginFadeIn(6000);
            
            TracksProperties.WaveOut?.Stop();
            
            if (Nightcore_Box.IsChecked == true)
            {
                if (_firstWaveFade != null)
                {
                    var firstSpeedControl = new VarispeedSampleProvider(_firstWaveFade, 100, new SoundTouchProfile(false, false));
                    firstSpeedControl.PlaybackRate = 1.4f;;
                    TracksProperties.WaveOut.Init(firstSpeedControl);
                }
            }
            else if (Delay_Box.IsChecked == true)
            {
                if (_firstWaveFade != null)
                {
                    var firstDelayEffect = new DelayEffect(_firstWaveFade, 300, 0.5f);
                    TracksProperties.WaveOut.Init(firstDelayEffect);
                }
            }
            else if (Chorus_Box.IsChecked == true)
            {
                if (_firstWaveFade != null)
                {
                    var firstChorusEffect = new ChorusEffect(_firstWaveFade, 160, 0.4f);
                    TracksProperties.WaveOut.Init(firstChorusEffect);
                }
            }
            else
            {
                TracksProperties.WaveOut.Init(_secWaveFade);
            }
            
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
                _firstWaveFade?.BeginFadeOut(7000);

                if (TracksProperties.IsLoopOn == 2)
                {
                    TracksProperties.SecAudioFileReader = new AudioFileReader(TracksProperties.TracksList?
                        .ElementAt(TracksProperties.SelectedTrack.Id - 1).Path);
                    TracksProperties.SelectedTrack =
                        TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id - 1);
                    InitializeSecWave();
                }
                // Sprawdzenie czy jest włączona opcja schuffle, ponieważ ona zmienia następny track
                else if (TracksProperties.IsSchuffleOn)
                {
                    if (TracksProperties.AvailableNumbers?.Count == 0)
                    {
                        TracksProperties.SelectedTrack =
                            TracksProperties.TracksList?.ElementAt(TracksProperties.FirstPlayed.Id - 1);

                        // Reset listy dostępnych numerów utworów
                        if (TracksProperties.TracksList != null)
                        {
                            TracksProperties.AvailableNumbers =
                                Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                            var random = new Random();
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
                            new AudioFileReader(TracksProperties.TracksList?
                                .ElementAt(TracksProperties.AvailableNumbers.ElementAt(0)).Path);
                        TracksProperties.PrevTrack.Add(TracksProperties.SelectedTrack);
                        TracksProperties.SelectedTrack =
                            TracksProperties.TracksList?.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0));
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
                                new AudioFileReader(TracksProperties.TracksList?.ElementAt(0).Path);
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
                _secWaveFade?.BeginFadeOut(7000);

                if (TracksProperties.IsLoopOn == 2)
                {
                    TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.TracksList
                        ?.ElementAt(TracksProperties.SelectedTrack.Id - 1).Path);
                    TracksProperties.SelectedTrack =
                        TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id - 1);
                    InitializeFirstWave();
                }
                // Sprawdzenie czy jest włączona opcja schuffle, ponieważ ona zmienia następny track
                else if (TracksProperties.IsSchuffleOn)
                {
                    if (TracksProperties.AvailableNumbers?.Count == 0)
                    {
                        TracksProperties.SelectedTrack =
                            TracksProperties.TracksList?.ElementAt(TracksProperties.FirstPlayed.Id - 1);

                        // Reset listy dostępnych numerów utworów
                        if (TracksProperties.TracksList != null)
                        {
                            TracksProperties.AvailableNumbers =
                                Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                            var random = new Random();
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
                            TracksProperties.AudioFileReader =
                                new AudioFileReader(TracksProperties.SelectedTrack?.Path);
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
                            new AudioFileReader(TracksProperties.TracksList
                                ?.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0)).Path);
                        TracksProperties.PrevTrack.Add(TracksProperties.SelectedTrack);
                        TracksProperties.SelectedTrack =
                            TracksProperties.TracksList?.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0));
                        InitializeFirstWave();
                        TracksProperties.AvailableNumbers?.RemoveAt(0);
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
                                new AudioFileReader(TracksProperties.TracksList?.ElementAt(0).Path);
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
                                new AudioFileReader(TracksProperties.TracksList
                                    ?.ElementAt(TracksProperties.SelectedTrack.Id).Path);
                            TracksProperties.SelectedTrack =
                                TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id);
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
                var isPlaying = false;

                if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                    isPlaying = true;
                else if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                    isPlaying = true;

                if (TracksProperties.SecAudioFileReader != null && TracksProperties.SelectedTrack?.Path == TracksProperties.SecAudioFileReader.FileName)
                    TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;

                TracksProperties.SecAudioFileReader = null;

                TracksProperties._timer.Stop();
                TracksProperties.SecWaveOut?.Stop();

                if (TracksProperties.AudioFileReader != null)
                {
                    _firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
                    if (Equalizer_box.IsChecked == true)
                        _firstWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value,
                            sld6.Value, sld7.Value, sld8.Value);
                    else
                        _firstWaveEqualizer?.UpdateEqualizer(0, 0, 0, 0, 0, 0, 0, 0);

                    _firstWaveFade = new FadeInOutSampleProvider(_firstWaveEqualizer);
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(_firstWaveFade);
                    
                    InitNightcoreEffect();
                    InitDelayEffect();
                    InitChorusEffect();
                    
                    if (isPlaying)
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
    private void OnOffDelay(object sender, RoutedEventArgs e)
    {
        Nightcore_Box.IsChecked = false;
        Chorus_Box.IsChecked = false;
        Distortion_Box.IsChecked = false;
        
        if (Delay_Box.IsChecked == true)
        {
            if (_firstWaveFade != null)
            {
                var firstDelayEffect = new DelayEffect(_firstWaveFade, 300, 0.5f);

                if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.WaveOut.Stop();
                    TracksProperties.WaveOut.Init(firstDelayEffect);
                    TracksProperties.WaveOut.Play();
                }
                else
                {
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(firstDelayEffect);
                }
            }
            if (_secWaveFade != null)
            {
                var secDelayEffect = new DelayEffect(_secWaveFade, 300, 0.5f);

                if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.SecWaveOut.Stop();
                    TracksProperties.SecWaveOut.Init(secDelayEffect);
                    TracksProperties.SecWaveOut.Play();
                }
            }
        }
        else
            ImplementBaseWave();
        
    }

    private void OnOffChorus(object sender, RoutedEventArgs e)
    {
        Nightcore_Box.IsChecked = false;
        Delay_Box.IsChecked = false;
        Distortion_Box.IsChecked = false;
        
        if (Chorus_Box.IsChecked == true)
        {
            if (_firstWaveFade != null)
            {
                var firstChorusEffect = new ChorusEffect(_firstWaveFade, 160, 0.4f);

                if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.WaveOut.Stop();
                    TracksProperties.WaveOut.Init(firstChorusEffect);
                    TracksProperties.WaveOut.Play();
                }
                else
                {
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(firstChorusEffect);
                }
            }
            if (_secWaveFade != null)
            {
                var secChorusEffect = new ChorusEffect(_secWaveFade, 160, 0.4f);

                if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.SecWaveOut.Stop();
                    TracksProperties.SecWaveOut.Init(secChorusEffect);
                    TracksProperties.SecWaveOut.Play();
                }
            }
        }
        else
            ImplementBaseWave();
    }

    private void OnOffDistortion(object sender, RoutedEventArgs e)
    {
    }

    private void OnOffNightcore(object sender, RoutedEventArgs e)
    {
        Delay_Box.IsChecked = false;
        Chorus_Box.IsChecked = false;
        Distortion_Box.IsChecked = false;
        
        if (Nightcore_Box.IsChecked == true)
        {
            if (_firstWaveFade != null)
            {
                var speedControl = new VarispeedSampleProvider(_firstWaveFade, 50, new SoundTouchProfile(false, true));
                speedControl.PlaybackRate = 1.4f;
                
                if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.WaveOut.Stop();
                    TracksProperties.WaveOut.Init(speedControl);
                    TracksProperties.WaveOut.Play();
                }
                else
                {
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(speedControl);
                }
            }
            if (_secWaveFade != null)
            {
                var secSpeedControl = new VarispeedSampleProvider(_secWaveFade, 50, new SoundTouchProfile(false, true));
                secSpeedControl.PlaybackRate = 1.4f;
                
                if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.SecWaveOut.Stop();
                    TracksProperties.SecWaveOut.Init(secSpeedControl);
                    TracksProperties.SecWaveOut.Play();
                }
            }
        }
        else
            ImplementBaseWave();
        
    }

    private void ImplementBaseWave()
    {
        if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
        {
            TracksProperties.SecWaveOut?.Stop();
            TracksProperties.SecWaveOut.Init(_secWaveFade);
            TracksProperties.SecWaveOut?.Play();
        }
        else if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
        {
            TracksProperties.WaveOut?.Stop();
            TracksProperties.WaveOut.Init(_firstWaveFade);
            TracksProperties.WaveOut?.Play();
        }
        else
        {
            TracksProperties.WaveOut?.Stop();
            TracksProperties.WaveOut.Init(_firstWaveFade);
        }
    }

    private void Reset_Btn_Click(object sender, RoutedEventArgs e)
    {
        sld1.Value = 0;
        sld2.Value = 0;
        sld3.Value = 0;
        sld4.Value = 0;
        sld5.Value = 0;
        sld6.Value = 0;
        sld7.Value = 0;
        sld8.Value = 0;
        ChangeEqualizerValues();
    }
}



