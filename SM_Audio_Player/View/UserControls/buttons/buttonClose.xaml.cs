using System.Windows;
using System.Windows.Controls;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class buttonClose : UserControl
{
    public buttonClose()
    {
        InitializeComponent();
    }
    /* Zamknięcie aplikacji */
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}