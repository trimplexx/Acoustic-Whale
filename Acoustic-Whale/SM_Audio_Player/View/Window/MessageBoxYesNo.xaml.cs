using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace SM_Audio_Player.View.Window;

/// <summary>
/// Interaction logic for MessageBoxYesNo.xaml
/// </summary>
public partial class MessageBoxYesNo
{
    public MessageBoxYesNo()
    {
        InitializeComponent();
    }

    private static MessageBoxYesNo? _messageBoxYesNo;
    private static DialogResult _result;
    public const string AddTxt = "A track is already in the list. Do you want to add it again? Track name: ";
    public const string DelTxt = "Are you sure you want to delete these track(s)? number of selected tracks: ";

    private void YES_OnClick(object sender, RoutedEventArgs e)
    {
        _result = System.Windows.Forms.DialogResult.Yes;
        Close();
    }

    private void NO_OnClick(object sender, RoutedEventArgs e)
    {
        _result = System.Windows.Forms.DialogResult.No;
        Close();
    }

    public static DialogResult Show(string message)
    {
        _messageBoxYesNo = new MessageBoxYesNo();
        _messageBoxYesNo.Message.Text = message;
        _messageBoxYesNo.ShowDialog();
        return _result;
    }

    private void YESNO_OnKeys(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _result = System.Windows.Forms.DialogResult.No;
            Close();
        }

        if (e.Key == Key.Enter)
        {
            _result = System.Windows.Forms.DialogResult.Yes;
            Close();
        }
    }
}