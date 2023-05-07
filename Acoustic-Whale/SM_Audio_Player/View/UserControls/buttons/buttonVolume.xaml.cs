using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons;

/// <summary>
/// Klasa reprezentująca przycisk służący do modulacji głośnocią.
/// </summary>
public partial class ButtonVolume : INotifyPropertyChanged
{
    public const string JsonPath = @"MusicVolumeJSON.json";
    private bool _isMuted;
    private double _savedVolumeValue;
    private string? _volumeIcon;

    /// <summary>
    /// Konstruktor klasy ButtonVolume, inicjalizujący elementy interfejsu i przypisujący metody obsługi zdarzeń.
    /// </summary>
    public ButtonVolume()
    {
        try
        {
            DataContext = this;
            InitializeComponent();
            // Odczyt wartości głośności z pliku JSON, lub ustawienie domyślnej wartości 50, jeśli plik nie istnieje.
            if (File.Exists(JsonPath))
                TracksProperties.Volume = ReadVolumeFromJsonFile();
            else
                TracksProperties.Volume = 50;

            SldVolume.Value = TracksProperties.Volume;  // Ustawienie wartości slidera na wczytaną wartość głośności
            VolumeIcon = ValueIconChange(SldVolume.Value); // Ustawienie ikony głośnika na odpowiednią
            // Przypisanie metod obsługi zdarzeń 
            ButtonNext.NextButtonClicked += OnTrackSwitch;
            ButtonPrevious.PreviousButtonClicked += OnTrackSwitch;
            ButtonPlay.TrackEnd += OnTrackSwitch;
            Library.DoubleClickEvent += OnTrackSwitch;
            Equalizer.FadeInEvent += OnTrackSwitch;
            ButtonPlay.ButtonPlayEvent += OnTrackSwitch;
            MainWindow.MuteSong += btnVolume_Click;
            MainWindow.VolumeChangeUp += VolumeUp;
            MainWindow.VolumeChangeDown += VolumeDown;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonVolume constructor exception: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Właściwość reprezentująca ikonę głośnika.
    /// </summary>
    public string? VolumeIcon
    {
        get => _volumeIcon;
        set
        {
            _volumeIcon = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VolumeIcon"));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    /// <summary>
    /// Metoda sprawdzająca aktualną wartość slidera i na jej podstawie ustawiająca ikonkę.
    /// </summary>
    private string? ValueIconChange(double volumeValue)
    {
        try
        {
            if (volumeValue == 0)
            {
                VolumeIcon = Icons.GetVolumeIconZero();
                return Icons.GetVolumeIconZero();
            }

            if (volumeValue > 0 && volumeValue <= 40)
            {
                VolumeIcon = Icons.GetVolumeIconLow();
                return Icons.GetVolumeIconLow();
            }

            if (volumeValue > 40 && volumeValue <= 75)
            {
                VolumeIcon = Icons.GetVolumeIconHalf();
                return Icons.GetVolumeIconHalf();
            }

            if (volumeValue > 75)
            {
                VolumeIcon = Icons.GetVolumeIconMax();
                return Icons.GetVolumeIconMax();
            }

            return Icons.GetVolumeIconHalf();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ValueIconChange exception: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Obsługa zdarzenia kliknięcia na przycisk głośności.
    /// Wycisza oraz odcisza utwór do poprzedniej głośności.
    /// </summary>
    private void btnVolume_Click(object sender, EventArgs e)
    {
        try
        {
            // Jeśli dźwięk jest wyciszony i wartość głośności została zapisana wcześniej,
            // to przywróć poprzednią wartość głośności, zmień ikonę głośnika i ustaw zmienną _isMuted na false.
            if (_isMuted && _savedVolumeValue != 0)
            {
                SldVolume.Value = _savedVolumeValue;
                ValueIconChange(TracksProperties.Volume);
                _isMuted = false;
            }
            // W przeciwnym przypadku zapisz obecną wartość głośności, ustaw wartość głośności na zero,
            // zmień ikonę głośnika na wyciszone i ustaw zmienną _isMuted na true.
            else
            {
                _savedVolumeValue = SldVolume.Value;
                SldVolume.Value = 0;
                VolumeIcon = Icons.GetVolumeIconZero();
                _isMuted = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"btnVolume_Click exception: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Metoda obsługująca zmianę wartości slidera głośności.
    /// </summary>  
    private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        try
        {
            /*Pobieranie aktualnej wartości slidera*/
            TracksProperties.Volume = e.NewValue;
            ValueIconChange(TracksProperties.Volume);

            var sliderValue = TracksProperties.Volume / 100.0; // Skalowanie wartości na zakres od 0 do 1
            var newVolume = sliderValue; // Obliczamy nową wartość głośności

            if (TracksProperties.AudioFileReader != null)
                TracksProperties.AudioFileReader.Volume = (float)newVolume; // Aktualizujemy głośność pliku audio

            if (TracksProperties.SecAudioFileReader != null)
                TracksProperties.SecAudioFileReader.Volume = (float)newVolume;

            // Zapis do pliku JSON w celu ponownego odpalenia aplikacji z zapisaną wartością głośności
            var output = JsonConvert.SerializeObject(TracksProperties.Volume, Formatting.Indented);
            File.WriteAllText(JsonPath, output);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Volume change error: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Metoda obsługująca zwiększenie głośności o 5% po wciśnięciu klawisza F3.
    /// </summary>
    private void VolumeUp(object sender, EventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.F3))
        {
            if (TracksProperties.AudioFileReader != null)
            {
                var currentVolume = TracksProperties.Volume;
                var newVolume = Math.Max(0, currentVolume + 5); // Zmniejsz głośność o 5%, ale nie mniej niż 0
                TracksProperties.Volume = newVolume;
                SldVolume.Value = newVolume; // Ustaw wartość slidera na nową wartość głośności

                var sliderValue = newVolume / 100.0; // Skalowanie wartości na zakres od 0 do 1

                TracksProperties.AudioFileReader.Volume = (float)sliderValue; // Aktualizujemy głośność pliku audio

                if (TracksProperties.SecAudioFileReader != null)
                    TracksProperties.SecAudioFileReader.Volume = (float)sliderValue;

                // Zapis do pliku JSON w celu ponownego odpalenia aplikacji z zapisaną wartością głośności
                var output = JsonConvert.SerializeObject(TracksProperties.Volume, Formatting.Indented);
                File.WriteAllText(JsonPath, output);
            }
        }
    }
    
    /// <summary>
    /// Metoda obsługująca zmniejszanie głośności o 5% po wciśnięciu klawisza F2.
    /// </summary>
    private void VolumeDown(object sender, EventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.F2))
        {
            if (TracksProperties.AudioFileReader != null)
            {
                var currentVolume = TracksProperties.Volume;
                var newVolume = Math.Max(0, currentVolume - 5); // Zmniejsz głośność o 5%, ale nie mniej niż 0
                TracksProperties.Volume = newVolume;
                SldVolume.Value = newVolume; // Ustaw wartość slidera na nową wartość głośności

                var sliderValue = newVolume / 100.0; // Skalowanie wartości na zakres od 0 do 1

                TracksProperties.AudioFileReader.Volume = (float)sliderValue; // Aktualizujemy głośność pliku audio

                if (TracksProperties.SecAudioFileReader != null)
                    TracksProperties.SecAudioFileReader.Volume = (float)sliderValue;

                // Zapis do pliku JSON w celu ponownego odpalenia aplikacji z zapisaną wartością głośności
                var output = JsonConvert.SerializeObject(TracksProperties.Volume, Formatting.Indented);
                File.WriteAllText(JsonPath, output);
            }
        }
    }

    /// <summary>
    /// Metoda służąca do odczytywania wartości głośności z pliku JSON.
    /// </summary>
    public double ReadVolumeFromJsonFile()
    {
        try
        {
            double volume = 50;
            var json = File.ReadAllText(JsonPath);
            dynamic jsonObj = JsonConvert.DeserializeObject(json) ?? 50;
            if (jsonObj != null)
                volume = jsonObj;
            return volume;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ReadValueJson error: {ex.Message}");
            return 0;
        }
    }
    
    /// <summary>
    /// Metoda obsługująca zmianę utworu na liście odtwarzania, aktualizująca wartość głośności
    /// i ustawiająca odpowiednią ikonę.
    /// </summary>
    private void OnTrackSwitch(object sender, EventArgs e)
    {
        try
        {
            ValueIconChange(TracksProperties.Volume);
            var sliderValue = TracksProperties.Volume / 100.0; // Skalowanie wartości na zakres od 0 do 1
            var newVolume = sliderValue; // Obliczamy nową wartość głośności

            if (TracksProperties.AudioFileReader != null)
                TracksProperties.AudioFileReader.Volume = (float)newVolume; // Aktualizujemy głośność pliku audio

            if (TracksProperties.SecAudioFileReader != null)
                TracksProperties.SecAudioFileReader.Volume = (float)newVolume;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Volume change error: {ex.Message}");
        }
    }
}