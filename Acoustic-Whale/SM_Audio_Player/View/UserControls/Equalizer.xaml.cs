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

            if (TracksProperties.SelectedTrack == TracksProperties.TracksList?.ElementAt(0) &&
                !TracksProperties.IsSchuffleOn && TracksProperties.IsLoopOn != 1)
            {
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut.Init(_firstWaveFade);
            }
            else if (TracksProperties.SelectedTrack?.Path == TracksProperties.FirstPlayed?.Path &&
                     TracksProperties.IsSchuffleOn && TracksProperties.IsLoopOn != 1)
            {
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut.Init(_firstWaveFade);
            }
            else
            {
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut.Init(_firstWaveFade);
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

            TracksProperties.SecWaveOut?.Stop();

            if (TracksProperties.SecWaveOut == null)
                TracksProperties.SecWaveOut = new WaveOutEvent();

            TracksProperties.SecWaveOut.Init(_secWaveFade);

            _secWaveFade?.BeginFadeIn(6000);
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
                _firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
            ChangeEqualizerValues();

            _firstWaveFade = new FadeInOutSampleProvider(_firstWaveEqualizer, true);

            TracksProperties.WaveOut?.Stop();

            TracksProperties.WaveOut.Init(_firstWaveFade);

            _firstWaveFade?.BeginFadeIn(6000);
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

    private void OnOffEffects(object sender, RoutedEventArgs e)
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
        if (Nightcore_Box.IsChecked == true)
        {
            _secWaveEqualizer?.UpdateEqualizer(3.5, 2.5, 3, 3.5, 2.5, 3, 2, 1);
            _firstWaveEqualizer?.UpdateEqualizer(3.5, 2.5, 3, 3.5, 2.5, 3, 2, 1);
            if (_secWaveFade != null)
            {
                var secSpeedControl = new SpeedControlSampleProvider(_secWaveFade) { Speed = 1.5f };
                if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.SecWaveOut?.Stop();
                    TracksProperties.SecWaveOut.Init(secSpeedControl);
                    TracksProperties.SecWaveOut?.Play();
                }
            }

            if (_firstWaveFade != null)
            {
                var speedControl = new SpeedControlSampleProvider(_firstWaveFade) { Speed = 1.5f };
                if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(speedControl);
                    TracksProperties.WaveOut?.Play();
                }
                else
                {
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(speedControl);
                }
            }
        }
        else
        {
            if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
            {
                TracksProperties.SecWaveOut?.Stop();
                TracksProperties.SecWaveOut.Init(_secWaveEqualizer);
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



