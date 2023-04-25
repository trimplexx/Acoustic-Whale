using System;
using System.Linq;
using System.Windows;
using NAudio.Wave;
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class ButtonPrevious
{
    /*
    * Zdarzenie odnoszące się do kliknięcia w ButtonPrevious, dzięki któremu w innych miejscach kodu wyniknie reakcja.
    * Utworzone zostało aby aktualizować poszczególne dane innych klas. 
    */
    public delegate void PreviousButtonClickedEventHandler(object sender, EventArgs e);

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

    public ButtonPrevious()
    {
        InitializeComponent();
    }

    public static event PreviousButtonClickedEventHandler? PreviousButtonClicked;

    public static event RefreshListEventHandler? RefreshList;

    public static event ResetEverythingEventHandler? ResetEverything;

    /*Włącz pooprzedni utwór*/
    private void btnPrevious_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (TracksProperties.SelectedTrack != null)
                // Sprawdź czy jest dostępny jakikolwiek numer na liście
                if (TracksProperties.TracksList != null && TracksProperties.TracksList.Count > 0)
                {
                    /*
                     * Przepisanie do bazowej ścieżki AudioFileReader utworu odtwarzanego z SecAudioFileReader.
                     */
                    if (TracksProperties.SecWaveOut != null &&
                        TracksProperties.SecWaveOut.PlaybackState == PlaybackState.Playing)
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

                        MessageBox.Show("Ups! Któryś z odtwarzanych utworów zmienił swoją ścieżkę do pliku :(");
                        ResetEverything?.Invoke(this, EventArgs.Empty);
                    }
                    /*
                    * Sprawdzenie, czy została użyta funkcja schuffle, w tym wypadku, jako poprzedni utwór nie zostanie
                    * wzięty utwór z mniejszym id, tylko ten zapamiętany na liśćie PrevTrack, aby móc powrócić do
                    * wylosowanego wcześniej kawałka.
                    */
                    else
                    {
                        if (TracksProperties.IsSchuffleOn)
                        {
                            // Sprawdzanie, czy lista nie jest pusta, jeżeli tak to odtworzy obecny utwór.
                            if (TracksProperties.PrevTrack.Count == 0)
                            {
                                TracksProperties.AvailableNumbers =
                                    Enumerable.Range(0, TracksProperties.TracksList.Count).ToList();
                                var random = new Random();
                                TracksProperties.AvailableNumbers =
                                    TracksProperties.AvailableNumbers.OrderBy(_ => random.Next()).ToList();
                                _btnPlay.PlayNewTrack();
                            }
                            // Jeżeli lista posiada poprzedni utwór, zostanie on wpisany jako obecny oraz zostanie odtworzony
                            else
                            {
                                /*
                             * Jezeli track, nie jest pierwszym, także jego numer zostanie zwrócony do listy dostępnych
                             * numerów, aby mógł być ponownie wybrany do odsłuchu
                             */
                                if (TracksProperties.SelectedTrack != TracksProperties.FirstPlayed) TracksProperties.AvailableNumbers?.Add(TracksProperties.SelectedTrack.Id - 1);

                                TracksProperties.SelectedTrack =
                                    TracksProperties.PrevTrack.ElementAt(TracksProperties.PrevTrack.Count - 1);
                                TracksProperties.PrevTrack.RemoveAt(TracksProperties.PrevTrack.Count - 1);
                                _btnPlay.PlayNewTrack();
                            }
                        }
                        else
                        {
                            // Sprawdzanie czy to pierwszy utwór na liście, jeżeli tak odtworzony zostanie od nowa.
                            if (TracksProperties.SelectedTrack.Id == 1)
                            {
                                _btnPlay.PlayNewTrack();
                            }
                            else
                            {
                                // W innym wypadku zostanie odtworzony poprzedni utwór.
                                TracksProperties.SelectedTrack =
                                    TracksProperties.TracksList.ElementAt(TracksProperties.SelectedTrack.Id - 2);
                                _btnPlay.PlayNewTrack();
                            }
                        }

                        PreviousButtonClicked?.Invoke(this, EventArgs.Empty);
                    }
                }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Previous button exception: {ex.Message}");
            throw;
        }
    }
}