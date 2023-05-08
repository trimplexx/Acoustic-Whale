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

    /// <summary>
    /// Metoda obsługuje zdarzenie kliknięcia na przycisk "Tak" w oknie MessageBoxYesNo.
    /// Po kliknięciu przycisku, metoda ustawia wartość pola _result na DialogResult.Yes i zamyka okno.
    /// </summary>
    private void YES_OnClick(object sender, RoutedEventArgs e)
    {
        _result = System.Windows.Forms.DialogResult.Yes;
        Close();
    }

    /// <summary>
    /// Metoda obsługuje zdarzenie kliknięcia na przycisk "Nie" w oknie MessageBoxYesNo.
    /// Po kliknięciu przycisku, metoda ustawia wartość pola _result na DialogResult.No i zamyka okno.
    /// </summary>
    private void NO_OnClick(object sender, RoutedEventArgs e)
    {
        _result = System.Windows.Forms.DialogResult.No;
        Close();
    }

    /// <summary>
    /// Metoda ta jest metodą statyczną klasy MessageBoxYesNo, która przyjmuje jako argument
    /// wiadomość do wyświetlenia w oknie MessageBoxYesNo.
    /// Metoda tworzy nowy obiekt MessageBoxYesNo, ustawia jego właściwość Message na podaną wiadomość,
    /// wyświetla okno MessageBoxYesNo metodą ShowDialog i zwraca wartość pola _result.
    /// </summary>
    public static DialogResult Show(string message)
    {
        _messageBoxYesNo = new MessageBoxYesNo();
        _messageBoxYesNo.Message.Text = message;
        _messageBoxYesNo.ShowDialog();
        return _result;
    }
 
    /// <summary>
    /// Metoda obsługuje zdarzenia naciśnięcia klawiszy Enter i Escape w oknie MessageBoxYesNo.
    /// Po naciśnięciu klawisza Escape, metoda ustawia wartość pola _result na DialogResult.No i zamyka okno.
    /// Po naciśnięciu klawisza Enter, metoda ustawia wartość pola _result na DialogResult.Yes i zamyka okno.
    /// </summary>
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