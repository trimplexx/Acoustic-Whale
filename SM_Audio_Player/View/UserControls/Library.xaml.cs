using Microsoft.Win32;
using SM_Audio_Player.Music;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using NAudio.Wave;
using Newtonsoft.Json;
using SM_Audio_Player.View.UserControls.buttons;
using JsonSerializer = System.Text.Json.JsonSerializer;
using TagLib;
using File = System.IO.File;

namespace SM_Audio_Player.View.UserControls
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    /// 

    public partial class Library : UserControl
    {
        public String jsonPath = @"MusicTrackList.json";
        public delegate void NextButtonClickedEventHandler(object sender, EventArgs e);
        public static event NextButtonClickedEventHandler DoubleClickEvent;
        private bool sortingtype = true;
        private string prevColumnSorted;

        public Library()
        {
            InitializeComponent();
            buttonNext.NextButtonClicked += OnTrackSwitch;
            buttonPrevious.PreviousButtonClicked += OnTrackSwitch;
            buttonPlay.TrackEnd += OnTrackSwitch;
            RefreshTrackListViewAndID();
        }

        // Metoda służąca odświeżaniu listView z pliku JSON, do ktorego zapisana zostaje lista piosenek
        public void RefreshTrackListViewAndID()
        {
            try
            {
                lv.Items.Clear();
                TracksProperties.tracksList.Clear();
                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    TracksProperties.tracksList = JsonConvert.DeserializeObject<List<Tracks>>(json);
                    int coutTracksOnJson = TracksProperties.tracksList.Count;
                    for (int i = 0; i < coutTracksOnJson; i++)
                    {
                        if (!File.Exists(TracksProperties.tracksList.ElementAt(i).Path))
                        {
                            TracksProperties.tracksList.Remove(TracksProperties.tracksList.ElementAt(i));
                            coutTracksOnJson--;
                            i--;
                        }
                        else
                        {
                            TracksProperties.tracksList.ElementAt(i).Id = i + 1;
                            lv.Items.Add(TracksProperties.tracksList.ElementAt(i));
                        }
                    }
                    var NewJsonData = JsonConvert.SerializeObject(TracksProperties.tracksList);
                    File.WriteAllText(jsonPath, NewJsonData);
                    lv.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Refresh Listview error: {ex.Message}");
            }
        }

        // Metoda sortująca trackList.
        public void SortTracksList(bool ascending, string property)
        {
            if (ascending)
                TracksProperties.tracksList = TracksProperties.tracksList.
                    OrderBy(track => track.GetType().GetProperty(property).GetValue(track)).ToList();
            else
                TracksProperties.tracksList = TracksProperties.tracksList.
                    OrderByDescending(track => track.GetType().GetProperty(property).GetValue(track)).ToList();
            
            // Zapisanie posortowanej listy do JSON
            var NewJsonData = JsonConvert.SerializeObject(TracksProperties.tracksList);
            File.WriteAllText(jsonPath, NewJsonData);
        }
        
        // Metoda odpowiadająca za kliknięcie nagłówka, po którym następuje sortowanie elementów w liście oraz na listview
        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {
                var binding = headerClicked.Column.DisplayMemberBinding as Binding;
                if (binding != null)
                {
                    string bindingPath = binding.Path.Path;
                    
                    if (prevColumnSorted == bindingPath && bindingPath != "Id")
                    {
                        if (!sortingtype)
                        {
                            SortTracksList(false, bindingPath);
                            sortingtype = true;
                        }
                        else
                        {
                            SortTracksList(true, bindingPath);
                            sortingtype = false;
                        }
                        prevColumnSorted = bindingPath;
                    }
                    else if (prevColumnSorted != bindingPath&& bindingPath != "Id")
                    {
                        SortTracksList(true, bindingPath);
                        sortingtype = false;
                        prevColumnSorted = bindingPath;
                    }
                }
                RefreshTrackListViewAndID();
            }
        }


        private void Add_Btn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Music files (*.mp3)|*.mp3|Waveform Audio File Format (.wav)|.wav|Windows Media Audio Professional (.wma)|.wma|MPEG-4 Audio (.mp4)|.mp4|" +
                "Free Lossless Audio Codec (.flac)|.flac|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filePath in openFileDialog.FileNames)
                {
                    string title = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    string newPath = filePath;

                    if (TracksProperties.tracksList.Any(track => track.Path == newPath))
                    {
                        MessageBoxResult result = MessageBox.Show("This music is already in the list. Do you want to add it again?", "Duplicate Music",
                            MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.No)
                        {
                            continue;
                        }
                    }

                    try
                    {
                        TagLib.File file = TagLib.File.Create(newPath);

                        string newTitle = file.Tag.Title ?? title;
                        string newAuthor = file.Tag.FirstPerformer ?? "Unknown";
                        string newAlbum = file.Tag.Album ?? "Unknown";
                        int duration = (int)file.Properties.Duration.TotalSeconds;

                        int hours = duration / 3600;
                        int minutes = (duration % 3600) / 60;
                        int seconds = duration % 60;

                        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);

                        int newId = TracksProperties.tracksList.Count + 1;

                        Tracks newTrack = new Tracks(newId, newTitle, newAuthor, newAlbum, newPath, formattedTime);

                        TracksProperties.tracksList.Add(newTrack);
                        var NewJsonData = JsonConvert.SerializeObject(TracksProperties.tracksList);
                        File.WriteAllText(jsonPath, NewJsonData);
                        RefreshTrackListViewAndID();

                        MessageBox.Show($"Successfully added {newTitle} to the list.", "Add Music");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading metadata from file: {ex.Message}", "Error");
                    }
                }
            }
        }

        private void Delete_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lv.SelectedItem != null)
                {
                    // Ask the user for confirmation
                    MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {TracksProperties.SelectedTrack.Title}?",
                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Remove the selected track from the tracksList
                        TracksProperties.tracksList.RemoveAt(lv.SelectedIndex);
                        var NewJsonData = JsonConvert.SerializeObject(TracksProperties.tracksList);
                        File.WriteAllText(jsonPath, NewJsonData);
                        TracksProperties.SelectedTrack = null;
                        RefreshTrackListViewAndID();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete Error");
            }
        }

        private void Lv_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lv.SelectedIndex != -1)
            {
                int elementId = lv.SelectedIndex;
                TracksProperties.SelectedTrack = TracksProperties.tracksList.ElementAt(elementId);
            }
        }
        private void OnTrackSwitch(object sender, EventArgs e)
        {
            lv.SelectedIndex = TracksProperties.SelectedTrack.Id - 1;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            buttonPlay btnPlay = new buttonPlay();
            btnPlay.btnPlay_Click(sender, e);
            DoubleClickEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
    
