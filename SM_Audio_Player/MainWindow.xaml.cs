using NAudio.Wave;
using SM_Audio_Player.View.UserControls.buttons;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace SM_Audio_Player
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        /*Reakcja okna na użycie LMB na pasek menu*/
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            /*Możliwość poruszania oknem*/
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();

            /*Możliwość poruszania oknem oraz przywrócenie go do normalnego rozmiaru z trybu pełnego okna*/
            if (e.LeftButton == MouseButtonState.Pressed && Application.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                btnMAX.MaximizeIcon = "M.3 89.5C.1 91.6 0 93.8 0 96V224 416c0 35.3 28.7 64 64 64l384 0c35.3 0 64-28.7 64-64V224 96c0-35.3-28.7-64-64-64H64c-2.2 0-4.4 .1-6.5 .3c-9.2 .9-17.8 3.8-25.5 8.2C21.8 46.5 13.4 55.1 7.7 65.5c-3.9 7.3-6.5 15.4-7.4 24zM48 224H464l0 192c0 8.8-7.2 16-16 16L64 432c-8.8 0-16-7.2-16-16l0-192z";

                /*Pobieranie pozycji kursora*/
                Point mousePosition = Mouse.GetPosition(this);
                double newX = mousePosition.X - (this.Width / 2);
                double newY = mousePosition.Y - (this.Height / 2);

                /*Sprawdzenie czy okno wychodzi poza lewą krawędź ekranu*/
                if (newX < 0)
                {
                    newX = 0;
                }

                /*Sprawdzenie czy okno wychodzi poza górną krawędź ekranu*/
                if (newY < 0)
                {
                    newY = 0;
                }

                /*Sprawdzenie czy okno wychodzi poza prawą krawędź ekranu*/
                if (newX + this.Width > SystemParameters.PrimaryScreenWidth)
                {
                    newX = SystemParameters.PrimaryScreenWidth - this.Width;
                }

                /*Sprawdzenie czy okno wychodzi poza dolną krawędź ekranu*/
                if (newY + this.Height > SystemParameters.PrimaryScreenHeight)
                {
                    newY = SystemParameters.PrimaryScreenHeight - this.Height;
                }

                /*Przypisanie pozycji lewej i górnej granicy okna*/
                this.Left = newX;
                this.Top = newY;
            }
        }

        private void Equalizer_Btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Playlist_Btn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

