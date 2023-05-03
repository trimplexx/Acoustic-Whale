using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class MessageBoxYesNo : INotifyPropertyChanged
{
    private string? _message;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    public MessageBoxYesNo()
    {
        DataContext = this;
        //WarningYesNo.Text = "Tutej domyślny tekst konstruktora";
        InitializeComponent();
    }
    
    public string? Message
    {
        get => _message;
        set
        {
            _message = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
        }
    }

    private void YES_OnClick(object sender, RoutedEventArgs e)
    {

    }

    private void NO_OnClick(object sender, RoutedEventArgs e)
    {

    }
}