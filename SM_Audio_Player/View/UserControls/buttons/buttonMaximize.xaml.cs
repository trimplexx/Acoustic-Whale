using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class buttonMaximize : UserControl, INotifyPropertyChanged
{
    public buttonMaximize()
    {
        DataContext = this;
        MaximizeIcon = Icons.GetMaxIcon();
        InitializeComponent();
    }

    private string maximizeIcon;

    public event PropertyChangedEventHandler? PropertyChanged;

    /*PropertyChanged - pozwala na przekazywanie wartości do widoku*/
    public string MaximizeIcon
    {
        get { return maximizeIcon; }
        set 
        { 
            maximizeIcon = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaximizeIcon"));
        }
    }

    /*Maxymalizacja okno playera*/
    private void btnMaximize_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow.WindowState == WindowState.Normal)
        {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            MaximizeIcon = Icons.GetMinIcon();
        }
        else if(Application.Current.MainWindow.WindowState == WindowState.Maximized)
        {
            Application.Current.MainWindow.WindowState = WindowState.Normal;
            MaximizeIcon = Icons.GetMaxIcon();
        }
    }
}