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
using System.Windows.Threading;

namespace SM_Audio_Player.View.UserControls
{
    public partial class Player : UserControl
    {
        private bool isDraggingSlider = false;
        private DispatcherTimer timer = new DispatcherTimer();

        public Player()
        {
            InitializeComponent();
            buttonNext.NextButtonClicked += OnTrackSwitch;
            buttonPrevious.PreviousButtonClicked += OnTrackSwitch;
            buttonPlay.TrackEnd += OnTrackSwitch;
            Library.DoubleClickEvent += OnTrackSwitch;
            //timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
        }

        private void OnTrackSwitch(object sender, EventArgs e)
        {
            title.Text = TracksProperties.SelectedTrack.Title;
            author.Text = TracksProperties.SelectedTrack.Author;
            CD.Text = TracksProperties.SelectedTrack.Album;
            tbTime.Text = TracksProperties.SelectedTrack.Time;
            sldTime.Value = 0;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (TracksProperties.audioFileReader != null)
                {
                    double totalSeconds = TracksProperties.audioFileReader.TotalTime.TotalSeconds;
                    double currentPosition = TracksProperties.audioFileReader.CurrentTime.TotalSeconds;
                    double progress = currentPosition / totalSeconds;
                    tbCurrTime.Text = TimeSpan.FromSeconds(currentPosition).ToString(@"hh\:mm\:ss");
                    sldTime.Value = progress * sldTime.Maximum;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Timer error: {ex.Message}");
            }
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (TracksProperties.audioFileReader != null && !isDraggingSlider)
                {
                    double totalSeconds = TracksProperties.audioFileReader.TotalTime.TotalSeconds;
                    double progress = sldTime.Value / sldTime.Maximum;
                    double newPosition = totalSeconds * progress;
                    TracksProperties.audioFileReader.CurrentTime = TimeSpan.FromSeconds(newPosition);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Position change error: {ex.Message}");
            }
        }
    }
}
