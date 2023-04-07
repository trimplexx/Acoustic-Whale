using Microsoft.Win32;
using SM_Audio_Player.Music;
using System;
using System.Collections.Generic;
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
        

        public Library()
        {
            InitializeComponent();
            if(File.Exists(jsonPath))
            {
                RefreshTrackListViewAndID();
            }
        }

        public void RefreshTrackListViewAndID()
        {
            try
            {
                lv.Items.Clear();
                TracksProperties.tracksList.Clear();
                string json = File.ReadAllText(jsonPath);
                TracksProperties.tracksList = JsonConvert.DeserializeObject<List<Tracks>>(json);
                int coutTracksOnJson = TracksProperties.tracksList.Count;
                for(int i = 0; i < coutTracksOnJson; i++)
                {
                    if (!File.Exists(TracksProperties.tracksList.ElementAt(i).Path))
                    {
                        TracksProperties.tracksList.Remove(TracksProperties.tracksList.ElementAt(i));
                        coutTracksOnJson--;
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
            catch (Exception ex)
            {
                MessageBox.Show($"Refresh Listview error");
            }
        }

        void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {

        }


        private void Add_Btn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Music files (*.mp3)|*.mp3|Waveform Audio File Format (.wav)|.wav|Windows Media Audio Professional (.wma)|.wma|MPEG-4 Audio (.mp4)|.mp4|" +
                "Free Lossless Audio Codec (.flac)|.flac|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string title = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                string newPath = openFileDialog.FileName;

                if (TracksProperties.tracksList.Any(track => track.Path == newPath))
                {
                    MessageBoxResult result = MessageBox.Show("This music is already in the list. Do you want to add it again?", "Duplicate Music",
                        MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.No)
                    {
                        return;
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
    }
}
    
