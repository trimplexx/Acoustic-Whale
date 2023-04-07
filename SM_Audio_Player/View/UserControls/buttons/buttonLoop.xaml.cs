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
    public partial class buttonLoop : UserControl
    {
        public buttonLoop()
        {
            InitializeComponent();
        }

        /*Powtarzaj aktualnie włączony utwór*/
        private void btnLoop_Click(object sender, RoutedEventArgs e)
        {
            if (!TracksProperties.isSchuffleOn)
            {
                if(TracksProperties.isLoopOn)
                    TracksProperties.isLoopOn = false;
                else
                    TracksProperties.isLoopOn = true;
            }
        }
    }
}
