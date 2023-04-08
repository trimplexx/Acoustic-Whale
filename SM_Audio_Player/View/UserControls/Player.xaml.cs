using NAudio.Wave;
using SM_Audio_Player.Music;
using SM_Audio_Player.View.UserControls.buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

    namespace SM_Audio_Player.View.UserControls
    {
        public partial class Player : UserControl
        {

            public Player()
            {
                InitializeComponent();
                buttonNext.NextButtonClicked += OnTrackSwitch;
                buttonPrevious.PreviousButtonClicked += OnTrackSwitch;
                buttonPlay.TrackEnd += OnTrackSwitch;
                Library.DoubleClickEvent += OnTrackSwitch;
            }

            private void OnTrackSwitch(object sender, EventArgs e)
            {
                title.Text = TracksProperties.SelectedTrack.Title;
                author.Text = TracksProperties.SelectedTrack.Author;
                CD.Text = TracksProperties.SelectedTrack.Album;
                tbTime.Text = TracksProperties.SelectedTrack.Time;
            }
        }
    }
