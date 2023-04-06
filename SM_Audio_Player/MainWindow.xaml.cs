using NAudio.Wave;
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
    }
}

