using Microsoft.Win32;
using SM_Audio_Player.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json;
using SM_Audio_Player.View.UserControls.buttons;
using File = System.IO.File;
using System.IO;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.CodeDom;

namespace SM_Audio_Player.View.UserControls;

public partial class Library : INotifyPropertyChanged
{
    public const string JsonPath = @"MusicTrackList.json";

    /*
    * Zdarzenie odnoszące się do double clicka w wybrany utwór, dzięki któremu w innych miejscach kodu wyniknie reakcja.
    * Utworzone zostało aby aktualizować poszczególne dane innych klas. 
    */
    public delegate void LibraryEventHandler(object sender, EventArgs e);

    public static event LibraryEventHandler? DoubleClickEvent;

    private bool _sortingtype = true;
    private string? _prevColumnSorted;
    public event PropertyChangedEventHandler? PropertyChanged;
    private string? _albumImg;
    

    public Library()
    {
        try
        {
            DataContext = this;
            AlbumImg = "..\\..\\assets\\default.png";
            InitializeComponent();
            ButtonPlay.TrackEnd += OnTrackSwitch;
            ButtonNext.NextButtonClicked += OnTrackSwitch;
            ButtonPrevious.PreviousButtonClicked += OnTrackSwitch;

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

    public string? AlbumImg
    {
        get => _albumImg;
        set
        {
            _albumImg = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlbumImg"));
        }
    }

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
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {
                var binding = headerClicked.Column.DisplayMemberBinding as Binding;
                if (binding != null)
                {
                    var bindingPath = binding.Path.Path;

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
                    else if (_prevColumnSorted != bindingPath && bindingPath != "Id")
                    {
                        SortTracksList(true, bindingPath);
                        _sortingtype = false;
                        _prevColumnSorted = bindingPath;
                    }
                }

                RefreshTrackListViewAndId();
                if (TracksProperties.TracksList != null && TracksProperties.SelectedTrack != null)
                    foreach (var track in TracksProperties.TracksList)
                        if (TracksProperties.SelectedTrack.Title == track.Title)
                            lv.SelectedIndex = track.Id - 1;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ListView header click exception: {ex.Message}");
            throw;
        }
    }

    private void Add_Btn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter =
                "Music files (*.mp3)|*.mp3|Waveform Audio File Format (.wav)|.wav|Windows Media Audio Professional (.wma)|.wma|MPEG-4 Audio (.mp4)|.mp4|" +
                "Free Lossless Audio Codec (.flac)|.flac|All files (*.*)|*.*";

            var addedToTheList = false;

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    var title = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    var newPath = filePath;

                    if (TracksProperties.TracksList != null &&
                        TracksProperties.TracksList.Any(track => track.Path == newPath))
                    {
                        var duplicateTrack = System.IO.Path.GetFileNameWithoutExtension(newPath);
                        var result = MessageBox.Show(
                            $"The track '{duplicateTrack}' is already in the list. Do you want to add it again?",
                            "Duplicate Music",
                            MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.No) continue;
                    }

                    var file = TagLib.File.Create(newPath);

                    var newTitle = file.Tag.Title ?? title;
                    var newAuthor = file.Tag.FirstPerformer ?? "Unknown";
                    var newAlbum = file.Tag.Album ?? "Unknown";
                    var duration = (int)file.Properties.Duration.TotalSeconds;
                    var albumCover = file.Tag.Pictures.FirstOrDefault();

                    var albumCoverPath = "";

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

                    var hours = duration / 3600;
                    var minutes = duration % 3600 / 60;
                    var seconds = duration % 60;

                    var formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);

                    if (TracksProperties.TracksList != null)
                    {
                        var newId = TracksProperties.TracksList.Count + 1;

                        var newTrack = new Tracks(newId, newTitle, newAuthor, newAlbum, newPath, formattedTime, albumCoverPath);

                        TracksProperties.TracksList.Add(newTrack);
                        addedToTheList = true;
                    }

                    var newJsonData = JsonConvert.SerializeObject(TracksProperties.TracksList);
                    File.WriteAllText(JsonPath, newJsonData);
                    RefreshTrackListViewAndId();
                }

                if (addedToTheList) MessageBox.Show($"Successfully added to the list.", "Add Music");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Add track exception {ex.Message}");
            throw;
        }
    }

    private void Delete_Btn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (lv.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete {lv.SelectedItems.Count} track(s)?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var selectedIndices = new List<int>();
                    foreach (var item in lv.SelectedItems) selectedIndices.Add(lv.Items.IndexOf(item));

                    // Posortuj indeksy w porządku malejącym, aby uniknąć problemów z usuwaniem wielu elementów
                    selectedIndices.Sort((a, b) => b.CompareTo(a));

                    // Sortuj indeksy w kolejności malejącej, aby uniknąć problemów z usuwaniem wielu elementów
                    foreach (var index in selectedIndices) TracksProperties.TracksList.RemoveAt(index);

                    var newJsonData = JsonConvert.SerializeObject(TracksProperties.TracksList);
                    File.WriteAllText(JsonPath, newJsonData);
                    TracksProperties.SelectedTrack = null;
                    RefreshTrackListViewAndId();

                    if (lv.Items.Count > 0)
                    {
                        var selectedIndex = Math.Max(selectedIndices.Min() - 1, 0);
                        lv.SelectedIndex = selectedIndex;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Delete track exception {ex.Message}");
            throw;
        }
    }


    private void Lv_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (lv.SelectedIndex != -1)
            {
                var elementId = lv.SelectedIndex;
                TracksProperties.SelectedTrack = TracksProperties.TracksList?.ElementAt(elementId);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Track selectedIndex changing exception {ex.Message}");
            throw;
        }
    }

    private void OnTrackSwitch(object sender, EventArgs e)
    {
        try
        {
            if (TracksProperties.SelectedTrack != null)
                lv.SelectedIndex = TracksProperties.SelectedTrack.Id - 1;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Track switch Library class exception {ex.Message}");
            throw;
        }
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            var selectedIndex = lv.SelectedIndex;
            var track = lv.SelectedItem as Tracks;
            RefreshTrackListViewAndId();
            lv.SelectedIndex = selectedIndex;

            var btnPlay = new ButtonPlay();
            if (TracksProperties.IsSchuffleOn && TracksProperties.WaveOut != null &&
                TracksProperties.AudioFileReader != null)
            {
                TracksProperties.WaveOut.Stop();
                TracksProperties.WaveOut.Dispose();
                TracksProperties.AudioFileReader = null;
            }
            btnPlay.btnPlay_Click(sender, e);
            AlbumImg = track.AlbumCoverPath;

            DoubleClickEvent?.Invoke(this, EventArgs.Empty);
            
        }
        catch (Exception ex)
        {
            MessageBox.Show($"On double click track {ex.Message}");
            throw;
        }
    }
}