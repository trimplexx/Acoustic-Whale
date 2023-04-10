using SM_Audio_Player.Music;
using System;
using System.Linq;
using System.Windows;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class ButtonNext
{
    /*
     * Zdarzenie odnoszące się do kliknięcia w ButtonNext, dzięki któremu w innych miejscach kodu wyniknie reakcja.
     * Utworzone zostało aby aktualizować poszczególne dane innych klas. 
     */
    public delegate void NextButtonClickedEventHandler(object sender, EventArgs e);
    public static event NextButtonClickedEventHandler? NextButtonClicked;
        
    /*
     * Eventy służące odświeżaniu listy, aby wyrzucić piosenkę przed jej odtworzeniem, gdy jego ścieżka uległaby zmianie
     * w trakcie odtwarzania. 
     */
    public delegate void RefreshListEventHandler(object sender, EventArgs e);
    public static event RefreshListEventHandler? RefreshList;

    private readonly ButtonPlay _btnPlay = new();

    public ButtonNext()
    {
        InitializeComponent();
    }

    /*Włącz następny utwór*/
    private void btnNext_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            RefreshList?.Invoke(this, EventArgs.Empty);
            
            if (TracksProperties.IsSchuffleOn)
            {
                _btnPlay.SchuffleFun();
            }
            else
            {
                // Sprawdzanie czy to nie był ostatni utwór na liście
                if (TracksProperties.TracksList != null && TracksProperties.SelectedTrack != null
                                                        && TracksProperties.SelectedTrack.Id !=
                                                        TracksProperties.TracksList.Count)
                {
                    TracksProperties.SelectedTrack =
                        TracksProperties.TracksList.ElementAt(TracksProperties.SelectedTrack.Id);
                    _btnPlay.PlayNewTrack();
                }
                else
                {
                    // Przełączenie na 1 utwór z listy po zakończeniu ostatniego
                    if (TracksProperties.TracksList != null)
                        TracksProperties.SelectedTrack = TracksProperties.TracksList.ElementAt(0);
                    _btnPlay.PlayNewTrack();
                    if (TracksProperties.IsLoopOn == 0) TracksProperties.WaveOut?.Pause();
                }
            }

            NextButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"NextButton click exception! {ex.Message}");
            throw;
        }
    }
}