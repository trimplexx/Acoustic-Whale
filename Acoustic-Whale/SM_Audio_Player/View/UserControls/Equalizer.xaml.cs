using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SM_Audio_Player.Music;
using SM_Audio_Player.SoundTouch;
using SM_Audio_Player.View.UserControls.buttons;

namespace SM_Audio_Player.View.UserControls;

public partial class Equalizer
{
    public delegate void FadeInEventHandler(object sender, EventArgs e);

    public delegate void FadeOffOnEventHandler(object sender, EventArgs e);

    /*
     * Podstawowe efekty ISampleProvider, które kolejno będą się na siebie nakładać, aby uzyskać możliwy efekt equalizera
     * oraz Fade in/out w tym samym czasie. Dodatkowo aplikacja zakłada rozbudowanie o kolejny możliwy efekt dźwiękowy
     * dostępny w tym samym czasie na utworze, dlatego dwa bazowe efekty będą dodatkowo rozbudowane o kolejny z nich
     * w dalszej części kodu.
     */
    private EqualizerSampleProvider? _firstWaveEqualizer;
    private FadeInOutSampleProvider? _firstWaveFade;
    private EqualizerSampleProvider? _secWaveEqualizer;
    private FadeInOutSampleProvider? _secWaveFade;
    private StereoToMonoSampleProvider? _firstStereoToMono;
    private StereoToMonoSampleProvider? _secStereoToMono;
    private bool _equalizerOn = true;
    
    // Effects
    private DelayEffect? _firstDelayEffect;
    private DelayEffect? _secDelayEffect;
    private ChorusEffect? _firstChorusEffect;
    private ChorusEffect? _secChorusEffect;
    private DistortionSampleProvider? _firstDistortionEffect;
    private DistortionSampleProvider? _secDistortionEffect;
    private VarispeedSampleProvider? _firstNightcoreEffect;
    private VarispeedSampleProvider? _secNightcoreEffect;


    public Equalizer()
    {
        InitializeComponent();
        /*
         * Przypisanie odpowiednich eventów występujących w innych miejscach w kodzie, aby aplikacja działała
         * płynnie oraz aktualizowała swoje wartości.
         */
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

    #region ChangingTracksEvents
    
    /*
     * Event odpowiadający za wybranie danej piosenki, która wiemy że będzie aktualie wymagana do odtworzenia, a nie będzie
     * możliwe, że użytkownik będzie chciał żeby była zatrzymana.
     */
    private void ButtonDoubleClickEvent(object sender, EventArgs e)
    {
        try
        {
            /*
             * Stworzenie kolejno nowych obiektów Equalizera oraz Fade, aby następnie sprawdzić, czy dodatkowo efektu
             * Fade nie należy umieścić w innym z dostępnych efektów. Jeżeli nie to bazowym efektem zostaje Equalizer
             * nałożony na efekt Fade in/out.
             */
            InitStereoToMonoEffect();
            if (TracksProperties.AudioFileReader != null)
                _firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
            
            if (StereoToMono_Box.IsChecked == true && _firstStereoToMono != null)
                _firstWaveEqualizer = new EqualizerSampleProvider(_firstStereoToMono);
            
            if (_equalizerOn)
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
            InitDistortionEffect();
            TracksProperties.WaveOut?.Play();

            /*
             * Event aktualizujący dane w innych miejscach kodu.
             */
            FadeOffOn?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"CreateEqualizers: {ex.Message}");
            throw;
        }
    }

    /*
     * Event odpowiadający za zagranie następnego tracka, który został odtworzony samoczynnie, bądź przez przycisk
     * 'Next', aby sprawdzać czy dana piosenka nie powinna sie przypadkiem zatrzymać. 
     */
    private void NextPrevEvent(object sender, EventArgs e)
    {
        try
        {
            /*
             * Inicjalizacja bazowych efektów podobnie jak w przypadku powyżej. 
             */
            InitStereoToMonoEffect();
            if (TracksProperties.AudioFileReader != null)
                _firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
            
            if (StereoToMono_Box.IsChecked == true && _firstStereoToMono != null)
                _firstWaveEqualizer = new EqualizerSampleProvider(_firstStereoToMono);
            
            if (_equalizerOn)
                _firstWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value,
                    sld6.Value, sld7.Value, sld8.Value);
            else
                _firstWaveEqualizer?.UpdateEqualizer(0, 0, 0, 0, 0, 0, 0, 0);

            _firstWaveFade = new FadeInOutSampleProvider(_firstWaveEqualizer);
            TracksProperties.WaveOut?.Stop();
            TracksProperties.WaveOut.Init(_firstWaveFade);
            
            /*
             * Sprawdzanie czy zainicjalizowana piosenka nie powinna zostać zatrzymana, w momencie, gdy przełączy się na 1
             * element z listy. Bądź gdy przełączy się na pierwszy utwór z opcją Schuffle. W przeciwnym wypadku piosenka
             * zostanie odtworzona. 
             */
            if (TracksProperties.SelectedTrack == TracksProperties.TracksList?.ElementAt(0) &&
                !TracksProperties.IsSchuffleOn && TracksProperties.IsLoopOn == 0)
            {
                InitNightcoreEffect();
                InitDelayEffect();
                InitChorusEffect();
                InitDistortionEffect();
            }
            else if (TracksProperties.SelectedTrack?.Path == TracksProperties.FirstPlayed?.Path &&
                     TracksProperties.IsSchuffleOn && TracksProperties.IsLoopOn == 0)
            {
                InitNightcoreEffect();
                InitDelayEffect();
                InitChorusEffect();
                InitDistortionEffect();
            }
            else
            {
                InitNightcoreEffect();
                InitDelayEffect();
                InitChorusEffect();
                InitDistortionEffect();
                TracksProperties.WaveOut?.Play();
            }

            /*
            * Event aktualizujący dane w innych miejscach kodu.
            */
            FadeOffOn?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"CreateEqualizers: {ex.Message}");
            throw;
        }
    }
    #endregion
    
    #region EqualizerLogic
    
    /*
     * Metoda odpowiada za aktualizacje wartości equalizerów służąca do wywołania w innych miejscach kodu.
     */
    private void ChangeEqualizerValues()
    {
        try
        {
            if (_equalizerOn)
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
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ChangeEqualizerValues on mouse click: {ex.Message}");
            throw;
        }
    }
    
    /*
     * Przycisk resetujący wartości sliderów oraz wartości equalizera.
     */
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
    
    /*
     * Event odpowiadający za dynamiczne przypisywanie wartości do pól nad sliderami.
     */
    private void DynamicEqualizerValueChaning(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        try
        {
            Slider_Value1.Text = ((int)sld1.Value).ToString();
            Slider_Value2.Text = ((int)sld2.Value).ToString();
            Slider_Value3.Text = ((int)sld3.Value).ToString();
            Slider_Value4.Text = ((int)sld4.Value).ToString();
            Slider_Value5.Text = ((int)sld5.Value).ToString();
            Slider_Value6.Text = ((int)sld6.Value).ToString();
            Slider_Value7.Text = ((int)sld7.Value).ToString();
            Slider_Value8.Text = ((int)sld8.Value).ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"DynamicEqualizerValueChaning: {ex.Message}");
            throw;
        }
    }
    
    /*
 * Zmiana wartości equalizera w momencie puszczenia klawiszu myszki na sliderze.
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
        if (_equalizerOn)
        {
            _equalizerOn = false;
            Equalizer_Btn.Content = "Equalizer Off";
        }
        else
        {
            _equalizerOn = true;
            Equalizer_Btn.Content = "Equalizer On";
        }
        ChangeEqualizerValues();
    }
    
    #endregion

    #region FadeInOutLogic
    
    /*
     * Uniwersalna metoda użyta następnie w evencie zmiany muzyki pozwalająca na inicjalizacje drugiej ścieżki dźwiękowej
     * do równoczesnego odtworzenia aby uzyskać efekt Fade in/out
     */
    private void InitializeSecWave()
    {
        try
        {
            if (StereoToMono_Box.IsChecked == true)
            {
                if (TracksProperties.SecAudioFileReader != null)
                {
                    if (TracksProperties.SecAudioFileReader.WaveFormat.Channels == 2)
                    {
                        _secStereoToMono = new StereoToMonoSampleProvider(TracksProperties.AudioFileReader);
                    }
                    
                }
            }
            
            if (TracksProperties.AudioFileReader != null)
                if (TracksProperties.SecAudioFileReader != null)
                    _secWaveEqualizer = new EqualizerSampleProvider(TracksProperties.SecAudioFileReader);

            if (StereoToMono_Box.IsChecked == true && _secStereoToMono != null)
                _secWaveEqualizer = new EqualizerSampleProvider(_secStereoToMono);
            
            ChangeEqualizerValues();

            /*
             * Nałożenie efektu FadeIn pogłaśniającego stopniowo piosenkę.
             */
            if (_secWaveEqualizer != null)
            {
                _secWaveFade = new FadeInOutSampleProvider(_secWaveEqualizer);
                _secWaveFade?.BeginFadeIn(6000);
            }

            TracksProperties.SecWaveOut?.Stop();

            if (TracksProperties.SecWaveOut == null)
                TracksProperties.SecWaveOut = new WaveOutEvent();

            /*
             * Sprawdzanie czy zostały nałożone jakiekolwiek inne efekty.
             */
            if (Nightcore_Box.IsChecked == true)
            {
                if (_secWaveFade != null)
                {
                    _secNightcoreEffect = new VarispeedSampleProvider(_secWaveFade, 100, new SoundTouchProfile(false, false));
                    _secNightcoreEffect.PlaybackRate = (float)SliderFirst.Value;
                    TracksProperties.SecWaveOut.Init(_secNightcoreEffect);
                }
            }
            else if (Delay_Box.IsChecked == true)
            {
                if (_secWaveFade != null)
                {
                    _secDelayEffect = new DelayEffect(_secWaveFade, (int)SliderFirst.Value, (float)SliderSec.Value);
                    TracksProperties.SecWaveOut.Init(_secDelayEffect);
                }
            }
            else if (Chorus_Box.IsChecked == true)
            {
                if (_secWaveFade != null)
                {
                    _secChorusEffect = new ChorusEffect(_secWaveFade, 160, 0.4f);
                    TracksProperties.SecWaveOut.Init(_secChorusEffect);
                }
            }
            else if (Distortion_Box.IsChecked == true)
            {
                if (_secWaveFade != null)
                {
                    _secDistortionEffect = new DistortionSampleProvider(_secWaveFade) { Gain = (float)SliderFirst.Value, Mix = (float)SliderSec.Value };
                    TracksProperties.SecWaveOut.Init(_secDistortionEffect);
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

    /*
    * Uniwersalna metoda użyta następnie w evencie zmiany muzyki pozwalająca na inicjalizacje następnej ścieżki dźwiękowej
    * do równoczesnego odtworzenia aby uzyskać efekt Fade in/out
    */
    private void InitializeFirstWave()
    {
        try
        {
            if (StereoToMono_Box.IsChecked == true)
            {
                if (TracksProperties.AudioFileReader != null)
                {
                    if (TracksProperties.AudioFileReader.WaveFormat.Channels == 2)
                    {
                        _firstStereoToMono = new StereoToMonoSampleProvider(TracksProperties.AudioFileReader);
                    }
                }
            }
            
            if (TracksProperties.AudioFileReader != null)
                _firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
            
            if (StereoToMono_Box.IsChecked == true && _firstStereoToMono != null)
                _firstWaveEqualizer = new EqualizerSampleProvider(_firstStereoToMono);
            
            ChangeEqualizerValues();

            /*
             * Nałożenie efektu fade in pogłaśniającego stopniowo piosenkę.
             */
            if (_firstWaveEqualizer != null)
            {
                _firstWaveFade = new FadeInOutSampleProvider(_firstWaveEqualizer, true);
                _firstWaveFade?.BeginFadeIn(6000);
            }
                
            
            TracksProperties.WaveOut?.Stop();
            
            /*
             * Sprawdzenie czy zostały nałożone jakieś inne efekty dźwiękowe.
             */
            if (Nightcore_Box.IsChecked == true)
            {
                if (_firstWaveFade != null)
                {
                    _firstNightcoreEffect = new VarispeedSampleProvider(_firstWaveFade, 100, new SoundTouchProfile(false, false));
                    _firstNightcoreEffect.PlaybackRate = (float)SliderFirst.Value;
                    TracksProperties.WaveOut.Init(_firstNightcoreEffect);
                }
            }
            else if (Delay_Box.IsChecked == true)
            {
                if (_firstWaveFade != null)
                {
                    _firstDelayEffect = new DelayEffect(_firstWaveFade, (int)SliderFirst.Value, (float)SliderSec.Value);
                    TracksProperties.WaveOut.Init(_firstDelayEffect);
                }
            }
            else if (Chorus_Box.IsChecked == true)
            {
                if (_firstWaveFade != null)
                {
                    _firstChorusEffect = new ChorusEffect(_firstWaveFade, (int)SliderFirst.Value, (float)SliderSec.Value);
                    TracksProperties.WaveOut.Init(_firstChorusEffect);
                }
            }
            else if (Distortion_Box.IsChecked == true)
            {
                if (_firstWaveFade != null)
                {
                    _firstDistortionEffect = new DistortionSampleProvider(_firstWaveFade) { Gain = (float)SliderFirst.Value, Mix = (float)SliderSec.Value };
                    TracksProperties.WaveOut.Init(_firstDistortionEffect);
                }
            }
            else
            {
                TracksProperties.WaveOut.Init(_firstWaveFade);
            }
            
            TracksProperties.WaveOut?.Play();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"InitializeFirstWave: {ex.Message}");
            throw;
        }
    }

    /*
     * Metoda wywoływana z klasy Player, która odpowiada za płynną zamiane pierwszej ścieżki dźwiękowej na drugą, uzyskując
     * w ten sposób efekt Fade in/out przy pomocy użycia FadeInOutSampleProvider. Event obejmuje wszelkie możliwe opcje
     * zmiany piosenki na następną włącznie z nałożonymi funkcjonalnościami schuffle bądź loop.
     */
    private void FirstToSecChange(object sender, EventArgs e)
    {
        try
        {
            if (Fade_box.IsChecked == true)
            {
                _firstWaveFade?.BeginFadeOut(7000);

                // Sprawdzenie funkcji loop tego samego utworu.
                if (TracksProperties.IsLoopOn == 2)
                {
                    if (TracksProperties.SelectedTrack != null)
                    {
                        TracksProperties.SecAudioFileReader = new AudioFileReader(TracksProperties.TracksList?
                            .ElementAt(TracksProperties.SelectedTrack.Id - 1).Path);
                        TracksProperties.SelectedTrack =
                            TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id - 1);
                    }

                    InitializeSecWave();
                }
                // Sprawdzenie czy jest włączona opcja schuffle, ponieważ ona zmienia następny track
                else if (TracksProperties.IsSchuffleOn)
                {
                    if (TracksProperties.AvailableNumbers?.Count == 0)
                    {
                        if (TracksProperties.FirstPlayed != null)
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
                                    TracksProperties.AvailableNumbers.OrderBy(_ => random.Next()).ToList();
                            }

                            // Usunięcie numeru odtwarzanego utworu z listy, aby ten się nie powtórzył w momencie losowania
                            if (TracksProperties.SelectedTrack != null)
                                TracksProperties.AvailableNumbers.Remove(TracksProperties.SelectedTrack.Id - 1);

                            // Wyczyszczenie listy poprzednich utworów
                            TracksProperties.PrevTrack.Clear();

                            // Sprawdzenie użycia funkcji loop, jeżeli jest włączona następny utwór zostanie włączony
                            if (TracksProperties.IsLoopOn == 1)
                            {
                                TracksProperties.SecAudioFileReader =
                                    new AudioFileReader(TracksProperties.SelectedTrack?.Path);
                                if (TracksProperties.SelectedTrack == TracksProperties.FirstPlayed)
                                    if (TracksProperties.WaveOut != null)
                                        InitializeSecWave();
                            }
                            // W przeciwnym razie wyczyścimy zasoby
                            else
                            {
                                TracksProperties.SecWaveOut?.Stop();
                                TracksProperties.WaveOut?.Stop();
                                TracksProperties.SecAudioFileReader = null;
                                TracksProperties.AudioFileReader = null;
                                TracksProperties.Timer.Stop();
                            }
                        }
                    }
                    else
                    {
                        // Jeżeli są dostępne następne utwory, odtwórz go korzystając z listy AvailableNumbers.
                        if (TracksProperties.AvailableNumbers != null)
                        {
                            TracksProperties.SecAudioFileReader =
                                new AudioFileReader(TracksProperties.TracksList?
                                    .ElementAt(TracksProperties.AvailableNumbers.ElementAt(0)).Path);
                            TracksProperties.PrevTrack.Add(TracksProperties.SelectedTrack);
                            TracksProperties.SelectedTrack =
                                TracksProperties.TracksList?.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0));
                            TracksProperties.AvailableNumbers.RemoveAt(0);
                        }

                        InitializeSecWave();
                    }
                }
                else
                {
                    if (TracksProperties.SelectedTrack?.Id == TracksProperties.TracksList?.Count)
                    {
                        // Przełączenie na 1 utwór z listy po zakończeniu ostatniego
                        TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(0);

                        // Sprawdzenie czy jest loop
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
                            TracksProperties.Timer.Stop();
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

    
    /*
     * Podobnie jak w powyższym przypadku metoda wywoływana z klasy Player, odpowiada za płynną zamiane drugiej ścieżki
     * dźwiękowej na pierwszą, uzyskując w ten sposób efekt Fade in/out przy pomocy użycia FadeInOutSampleProvider.
     * Event obejmuje wszelkie możliwe opcje zmiany piosenki na następną włącznie z nałożonymi funkcjonalnościami schuffle bądź loop.
     */
    private void SecToFirstChange(object sender, EventArgs e)
    {
        try
        {
            if (Fade_box.IsChecked == true)
            {
                _secWaveFade?.BeginFadeOut(7000);

                // Sprawdzenie funkcji loop tego samego utworu.
                if (TracksProperties.IsLoopOn == 2)
                {
                    if (TracksProperties.SelectedTrack != null)
                    {
                        TracksProperties.AudioFileReader = new AudioFileReader(TracksProperties.TracksList
                            ?.ElementAt(TracksProperties.SelectedTrack.Id - 1).Path);
                        TracksProperties.SelectedTrack =
                            TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id - 1);
                    }

                    InitializeFirstWave();
                }
                // Sprawdzenie czy jest włączona opcja schuffle, ponieważ ona zmienia następny track
                else if (TracksProperties.IsSchuffleOn)
                {
                    if (TracksProperties.AvailableNumbers?.Count == 0)
                    {
                        if (TracksProperties.FirstPlayed != null)
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
                                    TracksProperties.AvailableNumbers.OrderBy(_ => random.Next()).ToList();
                            }

                            // Usunięcie numeru odtwarzanego utworu z listy, aby ten się nie powtórzył w momencie losowania
                            if (TracksProperties.SelectedTrack != null)
                                TracksProperties.AvailableNumbers.Remove(TracksProperties.SelectedTrack.Id - 1);

                            // Wyczyszczenie listy poprzednich utworów
                            TracksProperties.PrevTrack.Clear();


                            // Sprawdzenie użycia funkcji loop, jeżeli jest włączona następny utwór zostanie włączony
                            if (TracksProperties.IsLoopOn == 1)
                            {
                                TracksProperties.AudioFileReader =
                                    new AudioFileReader(TracksProperties.SelectedTrack?.Path);
                                if (TracksProperties.SelectedTrack == TracksProperties.FirstPlayed)
                                    if (TracksProperties.WaveOut != null)
                                        InitializeFirstWave();
                            }
                            // W przeciwnym razie wyczyścimy zasoby
                            else
                            {
                                TracksProperties.SecWaveOut?.Stop();
                                TracksProperties.WaveOut?.Stop();
                                TracksProperties.SecAudioFileReader = null;
                                TracksProperties.AudioFileReader = null;
                                TracksProperties.Timer.Stop();
                            }
                        }
                    }
                    else
                    {
                        // Jeżeli są dostępne następne utwory, odtwórz go korzystając z listy AvailableNumbers.
                        if (TracksProperties.AvailableNumbers != null)
                        {
                            TracksProperties.AudioFileReader =
                                new AudioFileReader(TracksProperties.TracksList
                                    ?.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0)).Path);
                            TracksProperties.PrevTrack.Add(TracksProperties.SelectedTrack);
                            TracksProperties.SelectedTrack =
                                TracksProperties.TracksList?.ElementAt(TracksProperties.AvailableNumbers.ElementAt(0));
                            InitializeFirstWave();
                            TracksProperties.AvailableNumbers.RemoveAt(0);
                        }
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
                            TracksProperties.Timer.Stop();
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

    /*
     * Event odpowiadający za włączenie i wyłączanie funkcji fade in/out.
     */
    private void Fade_CheckBoxClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Fade_box.IsChecked == false)
            {
                /* Przypisanie wartości flagi przechowywanej w statycznej klasie w celu oznajmienia w innych
                 * miejscach kodu, że funckja została włączona.
                 */
                TracksProperties.IsFadeOn = true;
            }
            else
            {

                // Informacja o wyłączeniu funkcji Fade in/out
                TracksProperties.IsFadeOn = false;
                var isPlaying = false;

                /*
                 * Sprawdzanie czy jakikolwiek track w chwili wyłączenia był odtwarzany. Jeżeli tak to zmieni się wartość
                 * flagi isPlaying.
                 */
                if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                    isPlaying = true;
                else if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                    isPlaying = true;

                /*
                 * Przepisanie wartości SecAudioFileReadera do bazowego obiektu będącego źródłem odtwarzanej muzyki w programie,
                 * jeżeli ten był w danym momencie głównym źródłem.
                 */
                if (TracksProperties.SecAudioFileReader != null && TracksProperties.SelectedTrack?.Path == TracksProperties.SecAudioFileReader.FileName)
                    TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;

                TracksProperties.SecAudioFileReader = null;

                /*
                 * Zatrzymanie muzyki w celu poniższego zainicjalizowania nowej ścieżki dźwięku.
                 */
                TracksProperties.Timer.Stop();
                TracksProperties.SecWaveOut?.Stop();

                if (TracksProperties.AudioFileReader != null)
                {
                    InitStereoToMonoEffect();
                    
                    _firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
                    
                    if (StereoToMono_Box.IsChecked == true && _firstStereoToMono != null)
                        _firstWaveEqualizer = new EqualizerSampleProvider(_firstStereoToMono);
                    
                    if (_equalizerOn)
                        _firstWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value,
                            sld6.Value, sld7.Value, sld8.Value);
                    else
                        _firstWaveEqualizer?.UpdateEqualizer(0, 0, 0, 0, 0, 0, 0, 0);

                    _firstWaveFade = new FadeInOutSampleProvider(_firstWaveEqualizer);
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(_firstWaveFade);
                    
                    /*
                     * Sprawdzenie innych nałożonych efektów dźwiękowych.
                     */
                    InitNightcoreEffect();
                    InitDelayEffect();
                    InitChorusEffect();
                    InitDistortionEffect();

                    /*
                     * Flaga isPlaying wykorzystana wcześniej do sprawdzania czy muzyka w momencie wyłączenia opcji była odtwarzana.
                     * Jeżeli tak to chcemy odtworzyć z powrotem graną muzykę.
                     */
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
    #endregion

    #region OtherEffects
    /*
    * Sprawdzanie czy efekt nightcore nie został nałożony, aby aktualizować następną piosenkę o dodatkowy efekt.
    */
    private void InitNightcoreEffect()
    {
        if (Nightcore_Box.IsChecked == true)
        {
            if (_firstWaveFade != null)
            {
                _firstNightcoreEffect = new VarispeedSampleProvider(_firstWaveFade, 100, new SoundTouchProfile(false, false));
                _firstNightcoreEffect.PlaybackRate = (float)SliderFirst.Value;
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut?.Init(_firstNightcoreEffect);
            }
        }
    }

    /*
    * Sprawdzanie czy efekt Delay nie został nałożony, aby aktualizować następną piosenkę o dodatkowy efekt.
    */
    private void InitDelayEffect()
    {
        if (Delay_Box.IsChecked == true)
        {
            if (_firstWaveFade != null)
            {
                _firstDelayEffect = new DelayEffect(_firstWaveFade, (int)SliderFirst.Value, (float)SliderSec.Value);
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut?.Init(_firstDelayEffect);
            }
        }
    }
    
    /*
    * Sprawdzanie czy efekt Chorus nie został nałożony, aby aktualizować następną piosenkę o dodatkowy efekt.
    */
    private void InitChorusEffect()
    {
        if (Chorus_Box.IsChecked == true)
        {
            if (_firstWaveFade != null)
            {
                _firstChorusEffect = new ChorusEffect(_firstWaveFade, (int)SliderFirst.Value, (float)SliderSec.Value);
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut?.Init(_firstChorusEffect);
            }
        }
    }
    
    /*
    * Sprawdzanie czy efekt Chorus nie został nałożony, aby aktualizować następną piosenkę o dodatkowy efekt.
    */
    private void InitDistortionEffect()
    {
        if (Distortion_Box.IsChecked == true)
        {
            if (_firstWaveFade != null)
            {
                _firstDistortionEffect = new DistortionSampleProvider(_firstWaveFade) { Gain = (float)SliderFirst.Value, Mix = (float)SliderSec.Value };
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut?.Init(_firstDistortionEffect);
            }
        }
    }
    
    /*
    * Sprawdzanie czy efekt Chorus nie został nałożony, aby aktualizować następną piosenkę o dodatkowy efekt.
    */
    private void InitStereoToMonoEffect()
    {
        if (StereoToMono_Box.IsChecked == true)
        {
            if (TracksProperties.AudioFileReader != null)
            {
                if (TracksProperties.AudioFileReader.WaveFormat.Channels == 2)
                {
                    _firstStereoToMono = new StereoToMonoSampleProvider(TracksProperties.AudioFileReader);
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut?.Init(_firstStereoToMono);
                }
            }
        }
    }
    
    /*
     * Metoda odpowiadająca za event załączenia efektu Delay, poprzez naciśnięcie checkBoxa.
     */
    private void OnOffDelay(object sender, RoutedEventArgs e)
    {
        /*
        * Wyłączenie innych check boxów, ponieważ z założenia możemy nałożyć dodatkowo do opcji equalizera oraz
        * Fade in/out jeden z efektów na siebie.
        */
        Nightcore_Box.IsChecked = false;
        Chorus_Box.IsChecked = false;
        Distortion_Box.IsChecked = false;

        if (Delay_Box.IsChecked == false)
        {
            SliderSecPanel.Visibility = Visibility.Visible;
            SliderFirstPanel.Visibility = Visibility.Visible;
                
            SliderFirst.Value = 300;
            SliderFirst.Maximum = 1400;
            SliderFirst.Minimum = 100;
            SliderFirstText.Text = "Delay [ms]";
                
            SliderSec.Value = 0.5f;
            SliderSec.Maximum = 0.8f;
            SliderSec.Minimum = 0f;
            SliderSecText.Text = "Decay";
            
            /*
            * Sprawdzanie czy, któryś z zadeklarowanych obiektów nie jest równy null, oraz przypisanie do obu
            * nowego ISampleProvidera z efektem delay.
            */
            if (_firstWaveFade != null)
            {
                _firstDelayEffect = new DelayEffect(_firstWaveFade, 300, 0.5f);
                
                if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.WaveOut.Stop();
                    TracksProperties.WaveOut.Init(_firstDelayEffect);
                    TracksProperties.WaveOut.Play();
                }
                else
                {
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(_firstDelayEffect);
                }
            }
            if (_secWaveFade != null)
            {
                _secDelayEffect = new DelayEffect(_secWaveFade, (int)SliderFirst.Value, (float)SliderSec.Value);

                
                /*
                 * Sprawdzenie tylko i wyłączenie czy druga śceiżka dźwiękowa w danym momencie była grana, poniważ
                 * z założenia, gdy utwór zostaje zatrzymany automatycznie zmieniana jest ścieżka na bazowe
                 * źródło dźwięku oraz odtwarzacz.
                 */
                if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.SecWaveOut.Stop();
                    TracksProperties.SecWaveOut.Init(_secDelayEffect);
                    TracksProperties.SecWaveOut.Play();
                }
            }
        }
        else
        {
            ImplementBaseWave();
            SliderSecPanel.Visibility = Visibility.Hidden;
            SliderFirstPanel.Visibility = Visibility.Hidden;
        }
    }
    
    /*
    * Metoda odpowiadająca za event załączenia efektu Chorus, poprzez naciśnięcie checkBoxa.
    */
    private void OnOffChorus(object sender, RoutedEventArgs e)
    {
        /*
        * Wyłączenie innych check boxów, ponieważ z założenia możemy nałożyć dodatkowo do opcji equalizera oraz
        * Fade in/out jeden z efektów na siebie.
        */
        Nightcore_Box.IsChecked = false;
        Delay_Box.IsChecked = false;
        Distortion_Box.IsChecked = false;

        if (Chorus_Box.IsChecked == false)
        {
            SliderSecPanel.Visibility = Visibility.Visible;
            SliderFirstPanel.Visibility = Visibility.Visible;
                
            SliderFirst.Value = 160;
            SliderFirst.Maximum = 1200;
            SliderFirst.Minimum = 10;
            SliderFirstText.Text = "Delay [ms]";
                
            SliderSec.Value = 0.5f;
            SliderSec.Maximum = 0.8f;
            SliderSec.Minimum = 0f;
            SliderSecText.Text = "Depth";
            
            /*
            * Sprawdzanie czy, któryś z zadeklarowanych obiektów nie jest równy null, oraz przypisanie do obu
            * nowego ISampleProvidera z efektem chorus.
            */
            if (_firstWaveFade != null)
            {
                _firstChorusEffect = new ChorusEffect(_firstWaveFade, 160, 0.5f);
                
                if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.WaveOut.Stop();
                    TracksProperties.WaveOut.Init(_firstChorusEffect);
                    TracksProperties.WaveOut.Play();
                }
                else
                {
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(_firstChorusEffect);
                }
            }
            if (_secWaveFade != null)
            {
                _secChorusEffect = new ChorusEffect(_secWaveFade, (int)SliderFirst.Value, (float)SliderSec.Value);

                /*
                * Sprawdzenie tylko i wyłączenie czy druga śceiżka dźwiękowa w danym momencie była grana, poniważ
                * z założenia, gdy utwór zostaje zatrzymany automatycznie zmieniana jest ścieżka na bazowe
                * źródło dźwięku oraz odtwarzacz.
                */
                if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.SecWaveOut.Stop();
                    TracksProperties.SecWaveOut.Init(_secChorusEffect);
                    TracksProperties.SecWaveOut.Play();
                }
            }
        }
        else
        {
            SliderSecPanel.Visibility = Visibility.Hidden;
            SliderFirstPanel.Visibility = Visibility.Hidden;
            ImplementBaseWave();
        }
    }

    // Włączenie bądź wyłączenie zniekształcenia za pomocą check boxa.
    private void OnOffDistortion(object sender, RoutedEventArgs e)
    {
/*
         * Wyłączenie innych check boxów, ponieważ z założenia możemy nałożyć dodatkowo do opcji equalizera oraz
         * Fade in/out jeden z efektów na siebie.
         */
        Delay_Box.IsChecked = false;
        Chorus_Box.IsChecked = false;
        Nightcore_Box.IsChecked = false;

        if (Distortion_Box.IsChecked == false)
        {
            SliderSecPanel.Visibility = Visibility.Visible;
            SliderFirstPanel.Visibility = Visibility.Visible;
                
            SliderFirst.Value = 1.5f;
            SliderFirst.Maximum = 8f;
            SliderFirst.Minimum = 1f;
            SliderFirstText.Text = "Gain";
                
            SliderSec.Value = 7f;
            SliderSec.Maximum = 35f;
            SliderSec.Minimum = 1f;
            SliderSecText.Text = "Mix";
            
            /*
             * Sprawdzanie czy, któryś z zadeklarowanych obiektów nie jest równy null, oraz przypisanie do obu
             * nowego ISampleProvidera z efektem nightcore.
             */
            if (_firstWaveFade != null)
            {
                /*
                 * Utworzenie obiektu klasy Distortion effect
                 */
                _firstDistortionEffect = new DistortionSampleProvider(_firstWaveFade) { Gain = 1.5f, Mix = 7f };
                
                if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.WaveOut.Stop();
                    TracksProperties.WaveOut.Init(_firstDistortionEffect);
                    TracksProperties.WaveOut.Play();
                }
                else
                {
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(_firstDistortionEffect);
                }
            }
            if (_secWaveFade != null)
            {
                _secDistortionEffect = new DistortionSampleProvider(_secWaveFade) { Gain = (float)SliderFirst.Value, Mix = (float)SliderSec.Value };

                /*
                * Sprawdzenie tylko i wyłączenie czy druga śceiżka dźwiękowa w danym momencie była grana, poniważ
                * z założenia, gdy utwór zostaje zatrzymany automatycznie zmieniana jest ścieżka na bazowe
                * źródło dźwięku oraz odtwarzacz.
                */
                if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.SecWaveOut.Stop();
                    TracksProperties.SecWaveOut.Init(_secDistortionEffect);
                    TracksProperties.SecWaveOut.Play();
                }
            }
        }
        else
        {
            SliderSecPanel.Visibility = Visibility.Hidden;
            SliderFirstPanel.Visibility = Visibility.Hidden;
            ImplementBaseWave();
        }
    }

    /*
    * Metoda odpowiadająca za event załączenia efektu Nightcore, poprzez naciśnięcie checkBoxa.
    */
    private void OnOffNightcore(object sender, RoutedEventArgs e)
    {
        /*
         * Wyłączenie innych check boxów, ponieważ z założenia możemy nałożyć dodatkowo do opcji equalizera oraz
         * Fade in/out jeden z efektów na siebie.
         */
        Delay_Box.IsChecked = false;
        Chorus_Box.IsChecked = false;
        Distortion_Box.IsChecked = false;

        if (Nightcore_Box.IsChecked == false)
        {
            SliderSecPanel.Visibility = Visibility.Hidden;
            SliderFirstPanel.Visibility = Visibility.Visible;

            SliderFirst.Value = 1.4f;
            SliderFirst.Maximum = 2.2f;
            SliderFirst.Minimum = 1.0f;
            SliderFirstText.Text = "Speed";
            
            /*
             * Sprawdzanie czy, któryś z zadeklarowanych obiektów nie jest równy null, oraz przypisanie do obu
             * nowego ISampleProvidera z efektem nightcore.
             */
            if (_firstWaveFade != null)
            {
                /*
                 * Utworzenie obiektu klasy VarispeedSampleProvider w celu uzyskania efektu Nightcore poprzez
                 * przyśpieszenie granego utworu.
                 */
                
                // Wartość przyśpieszenia utworu.
                _firstNightcoreEffect = new VarispeedSampleProvider(_firstWaveFade, 50, new SoundTouchProfile(false, true));
                _firstNightcoreEffect.PlaybackRate = 1.4f;
                
                if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.WaveOut.Stop();
                    TracksProperties.WaveOut.Init(_firstNightcoreEffect);
                    TracksProperties.WaveOut.Play();
                }
                else
                {
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut.Init(_firstNightcoreEffect);
                }
            }
            if (_secWaveFade != null)
            {
                _secNightcoreEffect = new VarispeedSampleProvider(_secWaveFade, 50, new SoundTouchProfile(false, true));
                _secNightcoreEffect.PlaybackRate = (float)SliderFirst.Value;
                
                /*
                * Sprawdzenie tylko i wyłączenie czy druga śceiżka dźwiękowa w danym momencie była grana, poniważ
                * z założenia, gdy utwór zostaje zatrzymany automatycznie zmieniana jest ścieżka na bazowe
                * źródło dźwięku oraz odtwarzacz.
                */
                if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.SecWaveOut.Stop();
                    TracksProperties.SecWaveOut.Init(_secNightcoreEffect);
                    TracksProperties.SecWaveOut.Play();
                }
            }
        }
        else
        {
            ImplementBaseWave();
            SliderFirstPanel.Visibility = Visibility.Hidden;
        }
    }
    
    private void OnOffStereoToMono(object sender, RoutedEventArgs e)
    {
        var isPlaying = false;
        /*
         * Sprawdzanie czy jakikolwiek track w chwili wyłączenia był odtwarzany. Jeżeli tak to zmieni się wartość
         * flagi isPlaying.
         */
        if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
            isPlaying = true;
        else if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
            isPlaying = true;
        
        /*
        * Przepisanie wartości SecAudioFileReadera do bazowego obiektu będącego źródłem odtwarzanej muzyki w programie,
        * jeżeli ten był w danym momencie głównym źródłem.
        */
        if (TracksProperties.SecAudioFileReader != null && TracksProperties.SelectedTrack?.Path == TracksProperties.SecAudioFileReader.FileName)
            TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;

        TracksProperties.SecAudioFileReader = null;

        if (TracksProperties.AudioFileReader != null)
            _firstWaveEqualizer = new EqualizerSampleProvider(TracksProperties.AudioFileReader);
        
        /*
         * Sprawdzenie zaznaczenia opcji Stereo to mono
         */
        if (StereoToMono_Box.IsChecked == false && _firstStereoToMono != null)
        {
            if (TracksProperties.AudioFileReader != null)
            {
                if (TracksProperties.AudioFileReader.WaveFormat.Channels == 2)
                {
                    _firstStereoToMono = new StereoToMonoSampleProvider(TracksProperties.AudioFileReader);
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut?.Init(_firstStereoToMono);
                }
            }
            
            _firstWaveEqualizer = new EqualizerSampleProvider(_firstStereoToMono);
        }
            

        if (_equalizerOn)
            _firstWaveEqualizer?.UpdateEqualizer(sld1.Value, sld2.Value, sld3.Value, sld4.Value, sld5.Value,
                sld6.Value, sld7.Value, sld8.Value);
        else
            _firstWaveEqualizer?.UpdateEqualizer(0, 0, 0, 0, 0, 0, 0,0 );
        
        if(_firstWaveEqualizer != null)
            _firstWaveFade = new FadeInOutSampleProvider(_firstWaveEqualizer);
        TracksProperties.SecWaveOut?.Stop();
        TracksProperties.WaveOut?.Stop();
        
        if(_firstWaveFade != null)
            TracksProperties.WaveOut.Init(_firstWaveFade);
        
        InitNightcoreEffect();
        InitDelayEffect();
        InitChorusEffect();
        InitDistortionEffect();

        if (isPlaying)
        {
            TracksProperties.WaveOut?.Play();
        }
    }

    /*
     * Metoda odpowiadająca za powrócenie bazowej ścieżki odtwarzania się dźwięku, w momencie wyłączenia, któregość
     * z checkBoxów. Metoda dodatkowo sprawdza czy muzyka była grana oraz jeżeli tak, to wznawia jej odtwarzanie.
     */
    private void ImplementBaseWave()
    {
        if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing)
        {
            if (_secWaveFade != null)
            {
                TracksProperties.SecWaveOut.Stop();
                TracksProperties.SecWaveOut.Init(_secWaveFade);
                TracksProperties.SecWaveOut.Play();      
            }
        }
        else if (TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
        {
            if (_firstWaveFade != null)
            {
                TracksProperties.WaveOut.Stop();
                TracksProperties.WaveOut.Init(_firstWaveFade);
                TracksProperties.WaveOut.Play();
            }
        }
        else
        {
            if (_firstWaveFade != null)
            {
                TracksProperties.WaveOut?.Stop();
                TracksProperties.WaveOut.Init(_firstWaveFade);
            }
        }
    }
    
    #endregion

    private void ValueChangeSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (Delay_Box.IsChecked == true)
        {
            _firstDelayEffect?.SetDelay((int)SliderFirst.Value);
            _firstDelayEffect?.SetDecay((float)SliderSec.Value);
            
            if (_secDelayEffect != null)
            {
                _secDelayEffect.SetDelay((int)SliderFirst.Value);
                _secDelayEffect.SetDecay((float)SliderSec.Value);
            }
        }
        else if (Chorus_Box.IsChecked == true)
        {
            _firstChorusEffect?.SetDelay((int)SliderFirst.Value);
            _firstChorusEffect?.SetDepth((float)SliderSec.Value);
            
            if (_secChorusEffect != null)
            {
                _secChorusEffect.SetDelay((int)SliderFirst.Value);
                _secChorusEffect.SetDepth((float)SliderSec.Value);
            }
        }
        else if (Distortion_Box.IsChecked == true)
        {
            if (_firstDistortionEffect != null)
            {
                _firstDistortionEffect.Gain = (float)SliderFirst.Value;
                _firstDistortionEffect.Mix = (float)SliderSec.Value;
            }

            if (_secDistortionEffect != null)
            {
                _secDistortionEffect.Gain = (float)SliderFirst.Value;
                _secDistortionEffect.Mix = (float)SliderSec.Value;
            }
        }
        else if (Nightcore_Box.IsChecked == true)
        {
            if (_firstNightcoreEffect != null) _firstNightcoreEffect.PlaybackRate = (float)SliderFirst.Value;

            if(_secNightcoreEffect != null)
                _secNightcoreEffect.PlaybackRate = (float)SliderFirst.Value;
        }
    }

    private void DynamicSliderEffectValueChaning(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        SliderFirstValueText.Text = Math.Round(SliderFirst.Value, 1).ToString();
        SliderSecValueText.Text = Math.Round(SliderSec.Value, 1).ToString();
    }
}
