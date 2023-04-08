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
using SM_Audio_Player.Music;

namespace SM_Audio_Player.View.UserControls.buttons
{
    public partial class buttonShuffle : UserControl
    {
        public buttonShuffle()
        {
            InitializeComponent();
        }

        /*Włącz losowe odtwarzanie utworów*/
        private void btnShuffle_Click(object sender, RoutedEventArgs e)
        {
            // Sprawdzanie, czy funckaj loop nie została już użyta 
            if (!TracksProperties.isLoopOn)
            {
                // Jeżeli przycisk schuffle jest włączony 
                if (TracksProperties.isSchuffleOn)
                {
                    // Zresetuj wartości i wyczyść zapamiętane poprzednie utwory.
                    TracksProperties.availableNumbers = Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
                    TracksProperties.PrevTrack.Clear();
                    TracksProperties.isSchuffleOn = false;
                }
                // Jeżeli był wyłączony.
                else
                {
                    // Zapamiętaj pierwszy utwór, jako obecnie wybrany oraz zresetuj dostępne opcje.
                    TracksProperties.firstPlayed = TracksProperties.SelectedTrack;
                    TracksProperties.availableNumbers = Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
                    TracksProperties.availableNumbers.RemoveAt(TracksProperties.SelectedTrack.Id - 1);
                    TracksProperties.isSchuffleOn = true;
                }
                    
            }
        }
    }
}
