using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons
{
    public partial class buttonShuffle : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        private string shuffleColor;
        private string shuffleIcon;
        private string shuffleMouseColor;

        public buttonShuffle()
        {
            DataContext = this;
            ShuffleColor = "#037994";
            ShuffleMouseColor = "#2FC7E9";
            ShuffleIcon = Icons.GetShuffleIconOFF();
            InitializeComponent();
        }

        public string ShuffleIcon
        {
            get { return shuffleIcon; }
            set
            {
                shuffleIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShuffleIcon"));
            }
        }

        public string ShuffleColor
        {
            get { return shuffleColor; }
            set
            {
                shuffleColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShuffleColor"));
            }
        }

        public string ShuffleMouseColor
        {
            get { return shuffleMouseColor; }
            set
            {
                shuffleMouseColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShuffleMouseColor"));
            }
        }

        /*Włącz losowe odtwarzanie utworów*/
        private void btnShuffle_Click(object sender, RoutedEventArgs e)
        {
            // Jeżeli przycisk schuffle jest włączony 
            if (TracksProperties.isSchuffleOn)
            {
                // Zresetuj wartości i wyczyść zapamiętane poprzednie utwory.
                TracksProperties.availableNumbers = Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
                TracksProperties.PrevTrack.Clear();
                ShuffleIcon = Icons.GetShuffleIconOFF();
                ShuffleColor = "#037994";
                ShuffleMouseColor = "#2FC7E9";
                TracksProperties.isSchuffleOn = false
            }
            // Jeżeli był wyłączony.
            else
            {
                if (TracksProperties.SelectedTrack != null)
                {
                    // Zapamiętaj pierwszy utwór, jako obecnie wybrany oraz zresetuj dostępne opcje.
                    TracksProperties.firstPlayed = TracksProperties.SelectedTrack;
                    TracksProperties.availableNumbers = Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
                    TracksProperties.availableNumbers.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
                }
                ShuffleIcon = Icons.GetShuffleIconON();
                ShuffleColor = "#2FC7E9";
                ShuffleMouseColor = "#45a7bc";
                TracksProperties.isSchuffleOn = true;
            }
        }
    }
}
