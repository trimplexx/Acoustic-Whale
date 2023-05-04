using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace SM_Audio_Player.View.Window
{
    /// <summary>
    /// Interaction logic for MessageBoxYesNo.xaml
    /// </summary>
    public partial class MessageBoxYesNo
    {
        public MessageBoxYesNo()
        {
            InitializeComponent();

        }
        static MessageBoxYesNo _MessageBoxYesNo;
        static DialogResult result;
        public static string addTxt = "A track is already in the list. Do you want to add it again? Track name: ";
        public static string delTxt = "Are you sure you want to delete these track(s)? number of selected tracks: ";

        private void YES_OnClick(object sender, RoutedEventArgs e)
        {
            result = System.Windows.Forms.DialogResult.Yes;
            this.Close();
        }

        private void NO_OnClick(object sender, RoutedEventArgs e)
        {
            result = System.Windows.Forms.DialogResult.No;
            this.Close();
        }

        public static DialogResult Show(string message)
        {

            _MessageBoxYesNo = new MessageBoxYesNo();
            _MessageBoxYesNo.Message.Text = message;
            _MessageBoxYesNo.ShowDialog();
            return result;
        }

        private void YESNO_OnKeys(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                result = System.Windows.Forms.DialogResult.No;
                this.Close();
            }
            if (e.Key == Key.Enter)
            {
                result = System.Windows.Forms.DialogResult.Yes;
                this.Close();
            }
        }
    }
}
