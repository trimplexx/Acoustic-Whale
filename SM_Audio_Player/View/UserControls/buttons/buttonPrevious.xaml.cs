using SM_Audio_Player.Music;
using System;
using System.Linq;
using System.Windows;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class ButtonPrevious
{
    private readonly ButtonPlay _btnPlay = new();

    /*
    * Zdarzenie odnoszące się do kliknięcia w ButtonPrevious, dzięki któremu w innych miejscach kodu wyniknie reakcja.
    * Utworzone zostało aby aktualizować poszczególne dane innych klas. 
    */
    public delegate void PreviousButtonClickedEventHandler(object sender, EventArgs e);
    public static event PreviousButtonClickedEventHandler? PreviousButtonClicked;
        
    /*
     * Eventy służące odświeżaniu listy, aby wyrzucić piosenkę przed jej odtworzeniem, gdy jego ścieżka uległaby zmianie
     * w trakcie odtwarzania. 
     */
    public delegate void RefreshListEventHandler(object sender, EventArgs e);
    public static event RefreshListEventHandler? RefreshList;

    public ButtonPrevious()
    {
        InitializeComponent();
    }

    /*Włącz pooprzedni utwór*/
    private void btnPrevious_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            RefreshList?.Invoke(this, EventArgs.Empty);
            /*
             * Sprawdzenie, czy została użyta funkcja schuffle, w tym wypadku, jako poprzedni utwór nie zostanie
             * wzięty utwór z mniejszym id, tylko ten zapamiętany na liśćie PrevTrack, aby móc powrócić do
             * wylosowanego wcześniej kawałka.
             */
            if (TracksProperties.IsSchuffleOn && TracksProperties.TracksList != null)
            {
                // Sprawdzanie, czy lista nie jest pusta, jeżeli tak to odtworzy obecny utwór.
                if (TracksProperties.PrevTrack.Count == 0)
                {
                    TracksProperties.AvailableNumbers = Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                    _btnPlay.PlayNewTrack();
                }
                // Jeżeli lista posiada poprzedni utwór, zostanie on wpisany jako obecny oraz zostanie odtworzony
                else
                {
                    /*
                     * Jezeli track, nie jest pierwszym, także jego numer zostanie zwrócony do listy dostępnych
                     * numerów, aby mógł być ponownie wybrany do odsłuchu
                     */
                    if (TracksProperties.SelectedTrack != TracksProperties.FirstPlayed)
                        if (TracksProperties.SelectedTrack != null)
                            TracksProperties.AvailableNumbers?.Add(TracksProperties.SelectedTrack.Id - 1);
                    TracksProperties.SelectedTrack =
                        TracksProperties.PrevTrack.ElementAt(TracksProperties.PrevTrack.Count - 1);
                    TracksProperties.PrevTrack.RemoveAt(TracksProperties.PrevTrack.Count - 1);
                    _btnPlay.PlayNewTrack();
                }
            }
            else
            {
                // Sprawdzanie czy to pierwszy utwór na liście, jeżeli tak odtworzony zostanie od nowa.
                if (TracksProperties.SelectedTrack != null && TracksProperties.SelectedTrack.Id == 1)
                {
                    _btnPlay.PlayNewTrack();
                }
                else
                {
                    // W innym wypadku zostanie odtworzony poprzedni utwór.
                    if (TracksProperties.SelectedTrack != null)
                        TracksProperties.SelectedTrack =
                            TracksProperties.TracksList?.ElementAt(TracksProperties.SelectedTrack.Id - 2);
                    _btnPlay.PlayNewTrack();
                }
            }

            PreviousButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Previous button exception: {ex.Message}");
            throw;
        }
    }
}