using System;
using System.Windows;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class ButtonClose
{
    public ButtonClose()
    {
        InitializeComponent();
    }

    /* Zamknięcie aplikacji */
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