using NAudio.Wave;
using SM_Audio_Player.Music;
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

namespace SM_Audio_Player.View.UserControls.buttons
{
    public partial class buttonPrevious : UserControl
    {
        private buttonPlay btnPlay = new buttonPlay();
        public buttonPrevious()
        {
            InitializeComponent();
        }

        /*Włącz pooprzedni utwór*/
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /*
                 * Sprawdzenie, czy została użyta funkcja schuffle, w tym wypadku, jako poprzedni utwór nie zostanie
                 * wzięty utwór z mniejszym id, tylko ten zapamiętany na liśćie PrevTrack, aby móc powrócić do
                 * wylosowanego wcześniej kawałka.
                 */
                if (TracksProperties.isSchuffleOn)
                {
                    // Sprawdzanie, czy lista nie jest pusta, jeżeli tak to odtworzy obecny utwór.
                    if (TracksProperties.PrevTrack.Count == 0)
                    {
                        TracksProperties.availableNumbers = Enumerable.Range(0, TracksProperties.tracksList.Count).ToList();
                        btnPlay.PlayNewTrack();
                    }
                    // Jeżeli lista posiada poprzedni utwór, zostanie on wpisany jako obecny oraz zostanie odtworzony
                    else
                    {
                        /*
                         * Jezeli track, nie jest pierwszym, także jego numer zostanie zwrócony do listy dostępnych
                         * numerów, aby mógł być ponownie wybrany do odsłuchu
                         */
                        if(TracksProperties.SelectedTrack != TracksProperties.firstPlayed)
                            TracksProperties.availableNumbers.Add(TracksProperties.SelectedTrack.Id - 1);
                        TracksProperties.SelectedTrack =
                            TracksProperties.PrevTrack.ElementAt(TracksProperties.PrevTrack.Count - 1);
                        TracksProperties.PrevTrack.RemoveAt(TracksProperties.PrevTrack.Count - 1);
                        btnPlay.PlayNewTrack();
                    }
                }
                else
                {
                    // Sprawdzanie czy to pierwszy utwór na liście, jeżeli tak odtworzony zostanie od nowa.
                    if (TracksProperties.SelectedTrack.Id == 1)
                        btnPlay.PlayNewTrack();
                    else
                    {
                        // W innym wypadku zostanie odtworzony poprzedni utwór.
                        TracksProperties.SelectedTrack = TracksProperties.tracksList.ElementAt(TracksProperties.SelectedTrack.Id-2);
                        btnPlay.PlayNewTrack();
                    }
                }
            }
                catch (Exception ex)
            {
                MessageBox.Show($"Previous button error!");
            }
        }
    }
}
