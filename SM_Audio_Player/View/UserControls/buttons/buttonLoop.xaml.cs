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
    public partial class buttonLoop : UserControl, INotifyPropertyChanged
    {
        public buttonLoop()
        {
            DataContext = this;
            LoopColor = "#037994";
            LoopMouseColor = "#2FC7E9";
            LoopIcon = Icons.GetLoopOff();
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private string loopIcon;

        public string LoopIcon
        {
            get { return loopIcon; }
            set 
            { 
                loopIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoopIcon"));
            }
        }


        private string loopColor;

        public string LoopColor
        {
            get { return loopColor; }
            set 
            { 
                loopColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoopColor"));
            }
        }

        private string loopMouseColor;

        public string LoopMouseColor
        {
            get { return loopMouseColor; }
            set 
            { 
                loopMouseColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoopMouseColor"));
            }
        }
        
        /*Powtarzaj aktualnie włączony utwór*/
        private void btnLoop_Click(object sender, RoutedEventArgs e)
        {
            // Gdy loop jest włączony
            if (TracksProperties.isLoopOn)
                {
                    LoopIcon = Icons.GetLoopOff();
                    LoopColor = "#037994";
                    LoopMouseColor = "#2FC7E9";
                    TracksProperties.isLoopOn = false;
                }
            // Gdy loop jest wyłączony
                else
                {
                    LoopIcon = Icons.GetLoopOn();
                    LoopColor = "#2FC7E9";
                    LoopMouseColor = "#45a7bc";
                    TracksProperties.isLoopOn = true;
                }
        }
    }
}
