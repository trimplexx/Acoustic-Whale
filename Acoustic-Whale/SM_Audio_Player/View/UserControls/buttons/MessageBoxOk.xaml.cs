using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class MessageBoxOk : INotifyPropertyChanged
{
    private string? _message;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public MessageBoxOk()
    {
        DataContext = this;
        //WarningOK.Text = "Tutej domyślny tekst konstruktora";
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

    private void OK_OnClick(object sender, RoutedEventArgs e)
    {

    }
}