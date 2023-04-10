using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class ButtonShuffle : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private string? _shuffleColor;
    private string? _shuffleIcon;
    private string? _shuffleMouseColor;

    public ButtonShuffle()
    {
        try
        {
            DataContext = this;
            ShuffleColor = "#037994";
            ShuffleMouseColor = "#2FC7E9";
            ShuffleIcon = Icons.GetShuffleIconOff();
            InitializeComponent();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonSchuffle contructor exception: {ex.Message}");
            throw;
        }
    }

    public string? ShuffleIcon
    {
        get => _shuffleIcon;
        set
        {
            _shuffleIcon = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShuffleIcon"));
        }
    }

    public string? ShuffleColor
    {
        get => _shuffleColor;
        set
        {
            _shuffleColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShuffleColor"));
        }
    }

    public string? ShuffleMouseColor
    {
        get => _shuffleMouseColor;
        set
        {
            _shuffleMouseColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShuffleMouseColor"));
        }
    }

    /*Włącz losowe odtwarzanie utworów*/
    private void btnShuffle_Click(object sender, RoutedEventArgs e)
    {
        try
        {
// Jeżeli przycisk schuffle jest włączony 
            if (TracksProperties.IsSchuffleOn)
            {
                // Zresetuj wartości i wyczyść zapamiętane poprzednie utwory.
                if (TracksProperties.TracksList != null)
                    TracksProperties.AvailableNumbers = Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
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
                        TracksProperties.AvailableNumbers =
                            Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                    TracksProperties.AvailableNumbers?.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
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