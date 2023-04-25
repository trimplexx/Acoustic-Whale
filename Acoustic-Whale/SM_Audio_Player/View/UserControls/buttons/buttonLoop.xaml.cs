using System;
using System.ComponentModel;
using System.Windows;
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class ButtonLoop : INotifyPropertyChanged
{
    private string? _loopColor;
    private string? _loopIcon;
    private string? _loopMouseColor;
    public event PropertyChangedEventHandler? PropertyChanged;

    public ButtonLoop()
    {
        try
        {
            DataContext = this;
            LoopColor = "#037994";
            LoopMouseColor = "#2FC7E9";
            LoopIcon = Icons.GetLoopOff();
            InitializeComponent();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonLoop Constructor exception: {ex.Message}");
            throw;
        }
    }

    public string? LoopIcon
    {
        get => _loopIcon;
        set
        {
            _loopIcon = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoopIcon"));
        }
    }

    public string? LoopColor
    {
        get => _loopColor;
        set
        {
            _loopColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoopColor"));
        }
    }

    public string? LoopMouseColor
    {
        get => _loopMouseColor;
        set
        {
            _loopMouseColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoopMouseColor"));
        }
    }

    /*Powtarzaj aktualnie włączony utwór*/
    private void btnLoop_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Wyłączony
            if (TracksProperties.IsLoopOn == 0)
            {
                LoopIcon = Icons.GetLoopOn();
                LoopColor = "#2FC7E9";
                LoopMouseColor = "#45a7bc";
                TracksProperties.IsLoopOn = 1;
            }
            // Cała playlista
            else if (TracksProperties.IsLoopOn == 1)
            {
                LoopIcon = Icons.GetLoopOnce();
                LoopColor = "#2FC7E9";
                LoopMouseColor = "#45a7bc";
                TracksProperties.IsLoopOn = 2;
            }
            // Pojedynczy track
            else if (TracksProperties.IsLoopOn == 2)
            {
                LoopIcon = Icons.GetLoopOff();
                LoopColor = "#037994";
                LoopMouseColor = "#2FC7E9";
                TracksProperties.IsLoopOn = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonLoop click exception: {ex.Message}");
            throw;
        }
    }
}