using System.Windows;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class ButtonMinimize
{
    public ButtonMinimize()
    {
        InitializeComponent();
    }

    private void btnMinimize_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow != null)
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
    }
}