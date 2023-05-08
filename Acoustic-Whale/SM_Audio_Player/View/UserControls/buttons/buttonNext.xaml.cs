using System;
using System.Linq;
using System.Windows;
using NAudio.Wave;
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons;

///<summary>
/// Klasa reprezentująca przycisk do przejścia do następnego utworu.
/// </summary>
public partial class ButtonNext
{

    /// <summary>
    /// Delegat odpowiadający za reakcję na kliknięcie w przycisk, który jest wykorzystywany do aktualizowania danych
    /// w innych klasach.
    /// </summary>
    public delegate void NextButtonClickedEventHandler(object sender, EventArgs e);
    
    /// <summary>
    /// Delegat odpowiadający za odświeżanie listy odtwarzanych piosenek, w przypadku gdy ich ścieżki ulegną zmianie.
    /// </summary>
    public delegate void RefreshListEventHandler(object sender, EventArgs e);
    
    /// <summary>
    /// Delegat odpowiadający za resetowanie danych w przypadku, gdy odświeżona lista będzie zawierać mniej elementów niż
    /// lista przed odświeżeniem (np. gdy zmieni się ścieżka do pliku).
    /// </summary>
    public delegate void ResetEverythingEventHandler(object sender, EventArgs e);
    
    /// <summary>
    /// Delegat odpowiadający za obsługę zdarzenia braku wybranego utworu.
    /// </summary>
    public delegate void SelectedTrackNullEventHandler(object sender, EventArgs e);

    private readonly ButtonPlay _btnPlay = new();


    public ButtonNext()
    {
        InitializeComponent();
        // Dodanie metody 'btnNext_Click' jako zdarzenia reakcji na 'NextTrack' klasy 'MainWindow'.
        MainWindow.NextTrack += btnNext_Click;
    }

    // Zdarzenie odpowiadające za reakcję na kliknięcie w przycisk ButtonNext.
    public static event NextButtonClickedEventHandler? NextButtonClicked;

    // Zdarzenie odpowiadające za odświeżanie listy odtwarzanych piosenek.
    public static event RefreshListEventHandler? RefreshList;

    // Zdarzenie odpowiadające za resetowanie danych w przypadku, gdy odświeżona lista będzie zawierać mniej elementów niż lista przed odświeżeniem.
    public static event ResetEverythingEventHandler? ResetEverything;

    // Zdarzenie odpowiadające za obsługę zdarzenia braku wybranego utworu.
    public static event SelectedTrackNullEventHandler? NextSelectedNull;

    ///<summary>
    /// Metoda wywoływana po kliknięciu w przycisk ButtonNext, odpowiada za przejście do kolejnego utworu na liście.
    ///</summary>
    private void btnNext_Click(object sender, EventArgs e)
    {
        try
        {
            // Sprawdź czy jest dostępny jakikolwiek numer na liście
            if (TracksProperties.TracksList != null && TracksProperties.TracksList.Count > 0)
            {
                if (TracksProperties.SelectedTrack == null)
                {
                    _btnPlay.btnPlay_Click(sender, e);
                    NextSelectedNull?.Invoke(this, EventArgs.Empty);
                }

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