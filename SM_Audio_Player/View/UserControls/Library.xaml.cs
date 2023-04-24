using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NAudio.Wave;
using Newtonsoft.Json;
using SM_Audio_Player.Music;
using SM_Audio_Player.View.UserControls.buttons;

namespace SM_Audio_Player.View.UserControls;

public partial class Library
{
    /*
    * Zdarzenie odnoszące się do double clicka w wybrany utwór, dzięki któremu w innych miejscach kodu wyniknie reakcja.
    * Utworzone zostało aby aktualizować poszczególne dane innych klas. 
    */
    public delegate void LibraryEventHandler(object sender, EventArgs e);
    public delegate void OnDeleteTrackEventHandler(object sender, EventArgs e);
    

    /*
    * Akcja odpowiadająca za resetowanie danych w momencie, gdy odświeżona lista będzie zawierać mniej elementów
    * niż ta wartość, która została zapisana przed jej odświeżeniem (Przykładowo, gdy ktoś zmieni ścieżkę do pliku
    * w trakcie używania aplikacji mogła by ona wyrzucić wyjątek)
    */
    public delegate void ResetEverythingEventHandler(object sender, EventArgs e);

    public const string JsonPath = @"MusicTrackList.json";
    private string? _prevColumnSorted;

    private bool _sortingtype = true;

    public Library()
    {
        try
        {
            InitializeComponent();
            ButtonPlay.TrackEnd += OnTrackSwitch;
            ButtonNext.NextButtonClicked += OnTrackSwitch;
            ButtonPrevious.PreviousButtonClicked += OnTrackSwitch;
            ButtonPlay.ButtonPlayEvent += OnTrackSwitch;
            Equalizer.FadeInEvent += OnTrackSwitch;

            ButtonPlay.RefreshList += RefreshTrackList;
            ButtonNext.RefreshList += RefreshTrackList;
            ButtonPrevious.RefreshList += RefreshTrackList;
            RefreshTrackListViewAndId();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Library constructor exception: {ex.Message}");
            throw;
        }
    }

    public static event LibraryEventHandler? DoubleClickEvent;

    public static event ResetEverythingEventHandler? ResetEverything;
    
    public static event OnDeleteTrackEventHandler? OnDeleteTrack;

    /*
     * Istotne odświeżanie listy gdyby scieżka do pliku się zmieniła w trakcie odtwarzania, track z złą ściażka z pliku
     * JSON jest wyrzucany przed otworzeniem.
     */
    private void RefreshTrackList(object sender, EventArgs e)
    {
        try
        {
            var selectedIndex = lv.SelectedIndex;
            RefreshTrackListViewAndId();
            lv.SelectedIndex = selectedIndex;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Library RefreshTrackList exception: {ex.Message}");
            throw;
        }
    }

    // Metoda służąca odświeżaniu listView z pliku JSON, do ktorego zapisana zostaje lista piosenek
    private void RefreshTrackListViewAndId()
    {
        try
        {
            lv.Items.Clear();
            TracksProperties.TracksList?.Clear();
            if (File.Exists(JsonPath))
            {
                var json = File.ReadAllText(JsonPath);
                TracksProperties.TracksList = JsonConvert.DeserializeObject<List<Tracks>>(json);
                var coutTracksOnJson = TracksProperties.TracksList?.Count;
                for (var i = 0; i < coutTracksOnJson; i++)
                    if (!File.Exists(TracksProperties.TracksList?.ElementAt(i).Path))
                    {
                        TracksProperties.TracksList?.Remove(TracksProperties.TracksList.ElementAt(i));
                        coutTracksOnJson--;
                        i--;
                    }
                    else
                    {
                        TracksProperties.TracksList.ElementAt(i).Id = i + 1;
                        lv.Items.Add(TracksProperties.TracksList.ElementAt(i));
                    }

                var newJsonData = JsonConvert.SerializeObject(TracksProperties.TracksList);
                File.WriteAllText(JsonPath, newJsonData);
                lv.SelectedIndex = -1;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Refresh Listview error: {ex.Message}");
            throw;
        }
    }

    // Metoda sortująca trackList.
    public void SortTracksList(bool ascending, string property)
    {
        try
        {
            if (ascending)
                TracksProperties.TracksList = TracksProperties.TracksList?
                    .OrderBy(track => track.GetType().GetProperty(property)?.GetValue(track)).ToList();
            else
                TracksProperties.TracksList = TracksProperties.TracksList?
                    .OrderByDescending(track => track.GetType().GetProperty(property)?.GetValue(track)).ToList();

            // Zapisanie posortowanej listy do JSON
            var newJsonData = JsonConvert.SerializeObject(TracksProperties.TracksList);
            File.WriteAllText(JsonPath, newJsonData);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Sort TrackList error: {ex.Message}");
            throw;
        }
    }

    // Metoda odpowiadająca za kliknięcie nagłówka, po którym następuje sortowanie elementów w liście oraz na listview
    private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
    {
        try
        {
            // Pobierz kliknięty nagłówek kolumny
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {
                // Pobierz powiązanie danych z kolumny
                var binding = headerClicked.Column.DisplayMemberBinding as Binding;
                if (binding != null)
                {
                    var bindingPath = binding.Path.Path;

                    // Jeśli ta sama kolumna została kliknięta ponownie, zmień kierunek sortowania
                    if (_prevColumnSorted == bindingPath && bindingPath != "Id")
                    {
                        if (!_sortingtype)
                        {
                            SortTracksList(false, bindingPath);
                            _sortingtype = true;
                        }
                        else
                        {
                            SortTracksList(true, bindingPath);
                            _sortingtype = false;
                        }

                        _prevColumnSorted = bindingPath;
                    }
                    // Jeśli inna kolumna została kliknięta, posortuj listę w kolejności rosnącej i ustaw kierunek sortowania na malejący
                    else if (_prevColumnSorted != bindingPath && bindingPath != "Id")
                    {
                        SortTracksList(true, bindingPath);
                        _sortingtype = false;
                        _prevColumnSorted = bindingPath;
                    }
                }

                // Odśwież ListView i ustaw zaznaczenie na aktualnie odtwarzanym utworze
                RefreshTrackListViewAndId();
                if (TracksProperties.TracksList != null && TracksProperties.SelectedTrack != null)
                    foreach (var track in TracksProperties.TracksList)
                        if (TracksProperties.SelectedTrack.Title == track.Title)
                            lv.SelectedIndex = track.Id - 1;
            }
        }
        catch (Exception ex)
        {
            // Obsłuż i wyświetl wszystkie wyjątki, które mogą wystąpić podczas kliknięcia nagłówka kolumny w ListView
            MessageBox.Show($"ListView header click exception: {ex.Message}");
            throw;
        }
    }

    // Metoda odpowiadająca za dodawanie utworów do biblioteki utworów w programie wraz ze wszystkimi metadanymi
    private void Add_Btn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Utworzenie okna dialogowego umożliwiającego wybór plików muzycznych
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter =
                "Music files (*.mp3)|*.mp3|Waveform Audio File Format (.wav)|.wav|Windows Media Audio Professional (.wma)|.wma|MPEG-4 Audio (.mp4)|.mp4|" +
                "Free Lossless Audio Codec (.flac)|.flac|All files (*.*)|*.*";
            var addedToTheList = false;

            // Wyświetlenie okna dialogowego i dodanie wybranych plików do biblioteki
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    var title = Path.GetFileNameWithoutExtension(filePath);
                    var newPath = filePath;

                    // Sprawdzenie, czy plik został już dodany do biblioteki
                    if (TracksProperties.TracksList != null &&
                        TracksProperties.TracksList.Any(track => track.Path == newPath))
                    {
                        var duplicateTrack = Path.GetFileNameWithoutExtension(newPath);
                        var result = MessageBox.Show(
                            $"The track '{duplicateTrack}' is already in the list. Do you want to add it again?",
                            "Duplicate Music",
                            MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.No) continue;
                    }

                    // Pobranie metadanych z pliku muzycznego
                    var file = TagLib.File.Create(newPath);
                    var newTitle = file.Tag.Title ?? title;
                    var newAuthor = file.Tag.FirstPerformer ?? "Unknown";
                    var newAlbum = file.Tag.Album ?? "Unknown";
                    var duration = (int)file.Properties.Duration.TotalSeconds;
                    var albumCover = file.Tag.Pictures.FirstOrDefault();
                    var albumCoverPath = "";

                    // Jeśli plik posiada okładkę, to zostaje ona zapisana w folderze aplikacji
                    if (albumCover != null)
                    {
                        var albumCoverImage = new BitmapImage();
                        albumCoverImage.BeginInit();
                        albumCoverImage.StreamSource = new MemoryStream(albumCover.Data.Data);
                        albumCoverImage.EndInit();

                        var albumCoverName = $"{Guid.NewGuid()}.jpg";
                        albumCoverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, albumCoverName);
                        using var fileStream = new FileStream(albumCoverPath, FileMode.Create);
                        var encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(albumCoverImage));
                        encoder.Save(fileStream);
                    }

                    // Konwersja czasu trwania utworu na format hh:mm:ss lub mm:ss, w zależności od długości utworu
                    var formattedTime = "";
                    if (duration >= 3600)
                    {
                        var hours = duration / 3600;
                        var minutes = duration % 3600 / 60;
                        var seconds = duration % 60;
                        formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
                    }
                    else
                    {
                        var minutes = duration / 60;
                        var seconds = duration % 60;
                        formattedTime = string.Format("{0:D2}:{1:D2}", minutes, seconds);
                    }

                    // Dodanie utworu do listy utworów
                    if (TracksProperties.TracksList != null)
                    {
                        var newId = TracksProperties.TracksList.Count + 1;
                        var newTrack = new Tracks(newId, newTitle, newAuthor, newAlbum, newPath, formattedTime,
                            albumCoverPath);
                        TracksProperties.TracksList.Add(newTrack);
                        addedToTheList = true;
                    }

                    // Aktualizacja pliku JSON zawierającego dane o utworach
                    var newJsonData = JsonConvert.SerializeObject(TracksProperties.TracksList);
                    File.WriteAllText(JsonPath, newJsonData);
                    RefreshTrackListViewAndId();
                }
                // Wyświetlenie komunikatu o pomyślnym dodaniu utworów do biblioteki
                if (addedToTheList) MessageBox.Show("Successfully added to the list.", "Add Music");
            }

            if (lv.SelectedIndex != -1)
            {
                var elementId = lv.SelectedIndex;
                TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(elementId);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Add track exception {ex.Message}");
            throw;
        }
    }

    // Metoda odpowiadająca za usuwanie piosenek z biblioteki utworów
    private void Delete_Btn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Sprawdzenie, czy coś jest zaznaczone na liście utworów
            if (lv.SelectedItems.Count > 0)
            {
                // Wyświetlenie okna dialogowe z potwierdzeniem usunięcia wybranych utworów
                var result = MessageBox.Show(
                    $"Are you sure you want to delete {lv.SelectedItems.Count} track(s)?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                // Jeśli użytkownik potwierdzi, usuń wybrane utwory
                if (result == MessageBoxResult.Yes)
                {
                    var selectedIndices = new List<int>();
                    foreach (var item in lv.SelectedItems) selectedIndices.Add(lv.Items.IndexOf(item));

                    // Posortowanie indeksów w porządku malejącym, aby uniknąć problemów z usuwaniem wielu elementów
                    selectedIndices.Sort((a, b) => b.CompareTo(a));

                    // Sortowanie indeksów w kolejności malejącej, aby uniknąć problemów z usuwaniem wielu elementów
                    foreach (var index in selectedIndices) TracksProperties.TracksList?.RemoveAt(index);

                    // Zapisanie zaktualizowanej listy utworów do pliku JSON
                    var newJsonData = JsonConvert.SerializeObject(TracksProperties.TracksList);
                    File.WriteAllText(JsonPath, newJsonData);

                    if (TracksProperties.AudioFileReader?.FileName == TracksProperties.SelectedTrack?.Path && 
                        TracksProperties.WaveOut?.PlaybackState == PlaybackState.Playing)
                    {
                        TracksProperties.WaveOut.Stop();
                        TracksProperties.WaveOut = null;
                        TracksProperties.AudioFileReader = null;
                    }
                    else if (TracksProperties.SecWaveOut?.PlaybackState == PlaybackState.Playing
                             && TracksProperties.SecAudioFileReader?.FileName == TracksProperties.SelectedTrack?.Path)
                    {
                        TracksProperties.SecWaveOut.Stop();
                        TracksProperties.SecWaveOut = null;
                        TracksProperties.AudioFileReader = null;
                    }
                    
                    // Resetowanie wybranego utwór i odświeżanie ListView oraz ID utworów
                    RefreshTrackListViewAndId();

                    // Zaznaczenie utworu na indeksie kolejnego elementu poniżej ostatnio zaznaczonego elementu
                    if (lv.Items.Count > 0)
                    {
                        var selectedIndex = Math.Max(selectedIndices.Min() - 1, 0);
                        lv.SelectedIndex = selectedIndex;
                    }
                    OnDeleteTrack?.Invoke(this, EventArgs.Empty);
                }
            }
            // Zaktualizowanie wybranego utworu, jeśli jakiś utwór jest wciąż zaznaczony po operacji usuwania
            if (lv.SelectedIndex != -1)
            {
                var elementId = lv.SelectedIndex;
                TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(elementId);
            }
        }
        catch (Exception ex)
        {
            // Obsłużenie i wyświetlenie wszystkich wyjątków, które mogą wystąpić podczas operacji usuwania
            MessageBox.Show($"Delete track exception {ex.Message}");
            throw;
        }
    }

    // Metoda wywoływana po zmianie zaznaczenia na liście utworów
    private void Lv_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Jeśli jakiś utwór jest zaznaczony, zaktualizuj wybrany utwór w TracksProperties
            if (lv.SelectedIndex != -1)
            {
                var elementId = lv.SelectedIndex;
                TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(elementId);
            }
        }
        catch (Exception ex)
        {
            // Obsłuż i wyświetl wszystkie wyjątki, które mogą wystąpić podczas zmiany zaznaczenia utworu
            MessageBox.Show($"Track selectedIndex changing exception {ex.Message}");
            throw;
        }
    }

    // Metoda wywoływana po zmianie aktualnie odtwarzanego utworu
    private void OnTrackSwitch(object sender, EventArgs e)
    {
        try
        {
            // Jeśli wybrany utwór nie jest pusty, ustaw zaznaczenie ListView na indeks odpowiadający ID wybranego utworu
            if (TracksProperties.SelectedTrack != null)
                lv.SelectedIndex = TracksProperties.SelectedTrack.Id - 1;
        }
        catch (Exception ex)
        {
            // Obsłuż i wyświetl wszystkie wyjątki, które mogą wystąpić podczas zmiany aktualnie odtwarzanego utworu
            MessageBox.Show($"Track switch Library class exception {ex.Message}");
            throw;
        }
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            /*
            * Walidacja odświeżania listy, zapisuje bieżącą wartość posiadanych utworów na liście, a następnie
            * wykonane zostanie jej odświeżenie poprzez wywołanie 'RefreshList', następnie porównywana jest wartość
            * odświeżonej listy oraz zapisanej, w celu sprawdzenia czy ścieżka, któreś z piosenek nie uległa zmianie.
            * Jeżeli wartość piosenek uległa zmianie, następuje wyczyszczenie wszelkich danych związanych z piosenką
            * zarówno tych w widoku poprzez wywołanie zdarzenia ResetEverything.
            */
            if (TracksProperties.TracksList != null)
            {
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

                var trackListBeforeRefresh = TracksProperties.TracksList.Count;
                RefreshTrackList(sender, e);
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
                    var selectedIndex = lv.SelectedIndex;
                    RefreshTrackListViewAndId();
                    lv.SelectedIndex = selectedIndex;
                    TracksProperties.SelectedTrack = TracksProperties.TracksList.ElementAt(selectedIndex);
                    var btnPlay = new ButtonPlay();
                    btnPlay.PlayNewTrack();
                    DoubleClickEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"On double click track {ex.Message}");
            throw;
        }
    }
}