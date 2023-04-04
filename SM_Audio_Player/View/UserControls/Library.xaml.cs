using Microsoft.Win32;
using SM_Audio_Player.Music;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using TagLib;

namespace SM_Audio_Player.View.UserControls
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    /// 

    public partial class Library : UserControl
    {
        private List<Tracks> tracksList = new List<Tracks>();
        public Library()
        {
            InitializeComponent();
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

                if (tracksList.Any(track => track.Path == newPath))
                {
                    MessageBoxResult result = MessageBox.Show("This music is already in the list. Do you want to add it again?", "Duplicate Music",
                        MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            TagLib.File file = TagLib.File.Create(newPath);

                            string newTitle = file.Tag.Title ?? title;
                            string newAuthor = file.Tag.FirstPerformer ?? "Unknown";
                            string newAlbum = file.Tag.Album ?? "Unknown";

                            int newId = tracksList.Count + 1;

                            Tracks newTrack = new Tracks(newId, newTitle, newAuthor, newAlbum, newPath);
                            tracksList.Add(newTrack);

                            // Refresh the ListView to display the new track
                            lv.ItemsSource = null;
                            lv.ItemsSource = tracksList;

                            MessageBox.Show($"Successfully added {newTitle} to the list.", "Add Music");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error reading metadata from file: {ex.Message}", "Error");
                        }
                    }
                }
                else
                {
                    try
                    {
                        TagLib.File file = TagLib.File.Create(newPath);

                        string newTitle = file.Tag.Title ?? title;
                        string newAuthor = file.Tag.FirstPerformer ?? "Unknown";
                        string newAlbum = file.Tag.Album ?? "Unknown";

                        int newId = tracksList.Count + 1;

                        Tracks newTrack = new Tracks(newId, newTitle, newAuthor, newAlbum, newPath);
                        tracksList.Add(newTrack);

                        // Refresh the ListView to display the new track
                        lv.ItemsSource = null;
                        lv.ItemsSource = tracksList;

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
            if (lv.SelectedItems.Count > 0)
            {
                // Get the selected track
                Tracks selectedTrack = lv.SelectedItem as Tracks;

                // Ask the user for confirmation
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {selectedTrack.Title}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Remove the selected track from the tracksList
                    tracksList.Remove(selectedTrack);

                    // Refresh the ListView to update the list of tracks
                    lv.ItemsSource = null;
                    lv.ItemsSource = tracksList;
                }
            }
        }
    }
}
