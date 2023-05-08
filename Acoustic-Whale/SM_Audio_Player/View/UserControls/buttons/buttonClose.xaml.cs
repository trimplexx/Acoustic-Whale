using System;
using System.Windows;

namespace SM_Audio_Player.View.UserControls.buttons;

///<summary>
/// Klasa implementująca funkcjonalność przycisku zamykającego aplikację.
///</summary>
public partial class ButtonClose
{
    ///<summary>
    /// Konstruktor klasy ButtonClose.
    ///</summary>
    public ButtonClose()
    {
        InitializeComponent();
    }

    ///<summary>
    /// Metoda obsługująca zdarzenie kliknięcia przycisku zamykającego aplikację.
    /// Wywołuje metodę Shutdown klasy Application, która zamyka aplikację.
    ///</summary>
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Close window exception: {ex.Message}");
            throw;
        }
    }
}