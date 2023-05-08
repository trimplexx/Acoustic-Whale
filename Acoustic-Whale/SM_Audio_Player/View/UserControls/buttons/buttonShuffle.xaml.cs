using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons;

///<summary>
/// Klasa reprezentująca przycisk do włączenia losowego odtwarzania utworów etc.
/// </summary>
public partial class ButtonShuffle : INotifyPropertyChanged
{
    private string? _shuffleColor;
    private string? _shuffleIcon;
    private string? _shuffleMouseColor;
 
    public ButtonShuffle()
    {
        try
        {
            DataContext = this; // Ustawienie DataContext na tę klasę
            ShuffleColor = "#037994"; // Ustawienie koloru przycisku Shuffle
            ShuffleMouseColor = "#2FC7E9"; // Ustawienie koloru przycisku Shuffle po najechaniu myszką
            ShuffleIcon = Icons.GetShuffleIconOff(); // Ustawienie ikony przycisku Shuffle
            InitializeComponent(); // Inicjalizacja komponentów
            MainWindow.OnSchuffle += btnShuffle_Click; // Dodanie obsługi zdarzenia OnSchuffle do metody btnShuffle_Click
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonSchuffle contructor exception: {ex.Message}"); // Wyświetlenie komunikatu o wyjątku w konstruktorze ButtonSchuffle
            throw; // Rzucenie wyjątku dalej
        }
    }

    ///<summary>
    /// Metoda reprezentująca przycisk do włączenia losowych utworów.
    /// </summary>
    public string? ShuffleIcon
    {
        get => _shuffleIcon;
        set
        {
            _shuffleIcon = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShuffleIcon"));
        }
    }

    ///<summary>
    /// Metoda reprezentuje kolor przycisku, który jest wyświetlany, gdy przycisk jest w stanie normalnym.
    /// </summary>
    public string? ShuffleColor
    {
        get => _shuffleColor;
        set
        {
            _shuffleColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShuffleColor"));
        }
    }
    
    ///<summary>
    /// Reprezentuje kolor przycisku, który jest wyświetlany, gdy kursor myszy znajduje się
    /// na przycisku (stan najechania).
    /// </summary>
    public string? ShuffleMouseColor
    {
        get => _shuffleMouseColor;
        set
        {
            _shuffleMouseColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShuffleMouseColor"));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    ///<summary>
    /// Metoda reprezentująca przycisk do włączenia losowego odtwarzania utworów.
    /// </summary>
    private void btnShuffle_Click(object sender, EventArgs e)
    {
        try
        {
            // Jeżeli przycisk schuffle jest włączony 
            if (TracksProperties.IsSchuffleOn)
            {
                // Zresetuj wartości i wyczyść zapamiętane poprzednie utwory.
                if (TracksProperties.TracksList != null)
                {
                    TracksProperties.AvailableNumbers = Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                    var random = new Random();
                    TracksProperties.AvailableNumbers =
                        TracksProperties.AvailableNumbers.OrderBy(_ => random.Next()).ToList();
                }

                TracksProperties.PrevTrack.Clear();
                ShuffleIcon = Icons.GetShuffleIconOff();
                ShuffleColor = "#037994";
                ShuffleMouseColor = "#2FC7E9";
                TracksProperties.IsSchuffleOn = false;
            }
            // Jeżeli był wyłączony.
            else
            {
                if (TracksProperties.SelectedTrack != null)
                {
                    // Zapamiętaj pierwszy utwór, jako obecnie wybrany oraz zresetuj dostępne opcje.
                    TracksProperties.FirstPlayed = TracksProperties.SelectedTrack;
                    if (TracksProperties.TracksList != null)
                    {
                        TracksProperties.AvailableNumbers =
                            Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                        var random = new Random();
                        TracksProperties.AvailableNumbers =
                            TracksProperties.AvailableNumbers.OrderBy(_ => random.Next()).ToList();
                    }

                    TracksProperties.AvailableNumbers?.Remove(TracksProperties.SelectedTrack.Id - 1);
                }

                ShuffleIcon = Icons.GetShuffleIconOn();
                ShuffleColor = "#2FC7E9";
                ShuffleMouseColor = "#45a7bc";
                TracksProperties.IsSchuffleOn = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"SchuffleButton exception: {ex.Message}");
            throw;
        }
    }
}