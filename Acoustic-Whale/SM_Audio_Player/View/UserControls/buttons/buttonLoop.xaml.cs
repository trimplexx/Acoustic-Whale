using System;
using System.ComponentModel;
using System.Windows;
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons;

///<summary>
/// Klasa reprezentująca przycisk do obsługi funkcji loop odtwarzacza muzyki
/// Implementuje interfejs INotifyPropertyChanged w celu informowania interfejsu użytkownika o zmianach w wartościach pól klasy
///</summary>
public partial class ButtonLoop : INotifyPropertyChanged
{
    private string? _loopColor;
    private string? _loopIcon;
    private string? _loopMouseColor;
    public event PropertyChangedEventHandler? PropertyChanged;

    ///<summary>
    /// Konstruktor inicjujący obiekt ButtonLoop
    /// Ustawia domyślne wartości kolorów i ikony dla przycisku, oraz subskrybuje zdarzenie OnLoop klasy MainWindow, aby wywołać metodę btnLoop_Click po włączeniu opcji loop w odtwarzaczu muzyki
    ///</summary>
    public ButtonLoop()
    {
        try
        {
            DataContext = this;
            LoopColor = "#037994";
            LoopMouseColor = "#2FC7E9";
            LoopIcon = Icons.GetLoopOff();
            InitializeComponent();
            MainWindow.OnLoop += btnLoop_Click;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonLoop Constructor exception: {ex.Message}");
            throw;
        }
    }

    ///<summary>
    /// Właściwość określająca aktualną ikonę dla przycisku loop
    ///</summary>
    ///<value>Aktualna ikona przycisku loop</value>
    public string? LoopIcon
    {
        get => _loopIcon;
        set
        {
            _loopIcon = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoopIcon"));
        }
    }

    ///<summary>
    /// Właściwość określająca aktualny kolor przycisku loop
    ///</summary>
    ///<value>Aktualny kolor przycisku loop</value>
    public string? LoopColor
    {
        get => _loopColor;
        set
        {
            _loopColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoopColor"));
        }
    }

    ///<summary>
    /// Właściwość określająca aktualny kolor przycisku loop po najechaniu na niego kursorem
    ///</summary>
    ///<value>Aktualny kolor przycisku loop po najechaniu na niego kursorem</value>
    public string? LoopMouseColor
    {
        get => _loopMouseColor;
        set
        {
            _loopMouseColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoopMouseColor"));
        }
    }

    ///<summary>
    /// Metoda obsługująca kliknięcie przycisku loop
    /// Ustawia właściwości LoopIcon, LoopColor i LoopMouseColor w zależności od wartości zmiennej IsLoopOn klasy TracksProperties
    ///</summary>
    private void btnLoop_Click(object sender, EventArgs e)
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