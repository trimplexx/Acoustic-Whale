using System.Windows;
using System.Windows.Controls;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class buttonMaximize : UserControl
{
    public buttonMaximize()
    {
        InitializeComponent();
        
    }
    /* Maxymalizacja okno playera*/
    private void btnMaximize_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow.WindowState == WindowState.Normal)
        {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
        }
        else if(Application.Current.MainWindow.WindowState == WindowState.Maximized)
        {
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        }
    }
}