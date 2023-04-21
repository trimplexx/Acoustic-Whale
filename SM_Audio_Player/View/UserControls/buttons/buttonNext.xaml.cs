using System;
using System.Linq;
using System.Windows;
using NAudio.Wave;
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class ButtonNext
{
    /*
     * Zdarzenie odnoszące się do kliknięcia w ButtonNext, dzięki któremu w innych miejscach kodu wyniknie reakcja.
     * Utworzone zostało aby aktualizować poszczególne dane innych klas. 
     */
    public delegate void NextButtonClickedEventHandler(object sender, EventArgs e);

    /*
     * Eventy służące odświeżaniu listy, aby wyrzucić piosenkę przed jej odtworzeniem, gdy jego ścieżka uległaby zmianie
     * w trakcie odtwarzania. 
     */
    public delegate void RefreshListEventHandler(object sender, EventArgs e);

    /*
     * Akcja odpowiadająca za resetowanie danych w momencie, gdy odświeżona lista będzie zawierać mniej elementów
     * niż ta wartość, która została zapisana przed jej odświeżeniem (Przykładowo, gdy ktoś zmieni ścieżkę do pliku
     * w trakcie używania aplikacji mogła by ona wyrzucić wyjątek)
     */
    public delegate void ResetEverythingEventHandler(object sender, EventArgs e);

    private readonly ButtonPlay _btnPlay = new();
    

    public ButtonNext()
    {
        InitializeComponent();
    }

    public static event NextButtonClickedEventHandler? NextButtonClicked;

    public static event RefreshListEventHandler? RefreshList;

    public static event ResetEverythingEventHandler? ResetEverything;

    /*Włącz następny utwór*/
    private void btnNext_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Sprawdź czy jest dostępny jakikolwiek numer na liście
            if (TracksProperties.TracksList != null && TracksProperties.TracksList.Count > 0)
            {
                if (TracksProperties.SecWaveOut != null && TracksProperties.SecWaveOut.PlaybackState == PlaybackState.Playing)
                {
                    TracksProperties.AudioFileReader = TracksProperties.SecAudioFileReader;
                    TracksProperties.WaveOut?.Stop();
                    TracksProperties.WaveOut?.Init(TracksProperties.AudioFileReader);
                    
                    TracksProperties.SecWaveOut.Stop();
                    TracksProperties.SecWaveOut.Dispose();
                    TracksProperties.SecAudioFileReader = null;
                }
                
                /*
                 * Walidacja odświeżania listy, zapisuje bieżącą wartość posiadanych utworów na liście, a następnie
                 * wykonane zostanie jej odświeżenie poprzez wywołanie 'RefreshList', następnie porównywana jest wartość
                 * odświeżonej listy oraz zapisanej, w celu sprawdzenia czy ścieżka, któreś z piosenek nie uległa zmianie.
                 * Jeżeli wartość piosenek uległa zmianie, następuje wyczyszczenie wszelkich danych związanych z piosenką
                 * zarówno tych w widoku poprzez wywołanie zdarzenia ResetEverything.
                 */
                var trackListBeforeRefresh = TracksProperties.TracksList.Count;
                RefreshList?.Invoke(this, EventArgs.Empty);
                if (trackListBeforeRefresh != TracksProperties.TracksList.Count)
                {
                    if (TracksProperties.WaveOut != null && TracksProperties.AudioFileReader != null)
                    {
                        TracksProperties.WaveOut.Pause();
                        TracksProperties.WaveOut.Dispose();
                        TracksProperties.AudioFileReader = null;
                        TracksProperties.SelectedTrack = null;
                    }
                    MessageBox.Show($"Ups! Któryś z odtwarzanych utworów zmienił swoją ścieżkę do pliku :(");
                    ResetEverything?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    if (TracksProperties.IsSchuffleOn)
                    {
                        _btnPlay.SchuffleFun();
                    }
                    else
                    {
                        // Sprawdzanie czy to nie był ostatni utwór na liście
                        if (TracksProperties.SelectedTrack != null && TracksProperties.SelectedTrack.Id !=
                            TracksProperties.TracksList.Count)
                        {
                            TracksProperties.SelectedTrack =
                                TracksProperties.TracksList.ElementAt(TracksProperties.SelectedTrack.Id);
                            _btnPlay.PlayNewTrack();
                        }
                        else
                        {
                            // Przełączenie na 1 utwór z listy po zakończeniu ostatniego
                            TracksProperties.SelectedTrack = TracksProperties.TracksList.ElementAt(0);
                            _btnPlay.PlayNewTrack();
                            if (TracksProperties.IsLoopOn == 0) TracksProperties.WaveOut?.Pause();
                        }
                    }

                    NextButtonClicked?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"NextButton click exception! {ex.Message}");
            throw;
        }
    }
}