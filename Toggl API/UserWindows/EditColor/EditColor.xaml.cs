using Microsoft.Win32;
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

namespace Toggl_API.UserWindows.EditColor
{
    /// <summary>
    /// Interaction logic for EditColor.xaml
    /// </summary>
    public partial class EditColor : Window
    {
        public EditColor()
        {

            InitializeComponent();
            colorList.ItemsSource = MainWindow.helper.projectColors;

        }


        /// <summary>
        /// EventHandler for 'Export' menuitem click, export color.json file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Export_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON file (*.json)|*.json| All Files (*.*)|*.*";
            saveFileDialog.FileName = $"color";
            saveFileDialog.DefaultExt = ".json";

            if (saveFileDialog.ShowDialog() == true)
            {
                MainWindow.helper.SaveColors(saveFileDialog.FileName);
            }
        }


        /// <summary>
        /// Function that updates and save choosed colors on chart 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorPicker_LostFocus(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Owner).Refresh();
            MainWindow.helper.SaveColors();
        }
    }
}
