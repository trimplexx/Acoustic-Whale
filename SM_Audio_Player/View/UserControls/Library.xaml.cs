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
            openFileDialog.Filter = "Music files (*.mp3)|*.mp3|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string newTitle = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                string newPath = openFileDialog.FileName;

                if (tracksList.Any(track => track.Path == newPath))
                {
                    MessageBoxResult result = MessageBox.Show("This music is already in the list. Do you want to add it again?", "Duplicate Music", 
                        MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        int newId = tracksList.Count + 1;
                        string newAuthor = "Unknown";
                        string newAlbum = "Unknown";

                        Tracks newTrack = new Tracks(newId, newTitle, newAuthor, newAlbum, newPath);
                        tracksList.Add(newTrack);

                        // Refresh the ListView to display the new track
                        lv.ItemsSource = null;
                        lv.ItemsSource = tracksList;

                        MessageBox.Show($"Successfully added {newTitle} to the list.", "Add Music");
                    }
                }
                else
                {
                    int newId = tracksList.Count + 1;
                    string newAuthor = "Unknown";
                    string newAlbum = "Unknown";

                    Tracks newTrack = new Tracks(newId, newTitle, newAuthor, newAlbum, newPath);
                    tracksList.Add(newTrack);

                    // Refresh the ListView to display the new track
                    lv.ItemsSource = null;
                    lv.ItemsSource = tracksList;

                    MessageBox.Show($"Successfully added {newTitle} to the list.", "Add Music");
                }
            }
        }

        private void Delete_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (lv.SelectedItems.Count > 0)
            {
                // Get the selected track
                Tracks selectedTrack = lv.SelectedItem as Tracks;

                // Remove the selected track from the tracksList
                tracksList.Remove(selectedTrack);

                // Refresh the ListView to update the list of tracks
                lv.ItemsSource = null;
                lv.ItemsSource = tracksList;
            }
        }
    }
}
