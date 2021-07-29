using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Toggl_API.APIHelper.ClassModel;
using Toggl_API.ViewModel.EditChart;

namespace Toggl_API.UserWindows.EditChart
{
    /// <summary>
    /// Interaction logic for EditChart.xaml
    /// </summary>
    public partial class EditChart : Window
    {
        EditChartViewModel editChartViewModel;//Global variable for EditChartViewModel instance
        public static List<ProjectChart> localProjects;// Local project on chart
        public EditChart()
        {
            InitializeComponent();

            editChartViewModel = new EditChartViewModel(MainWindow.MainWindowProjects, MainWindow.helper);
            DataContext = editChartViewModel;
            localProjects = MainWindow.MainWindowProjects.ToList();
        }


        /// <summary>
        /// EventHandler for CheckBox Checked event, changes the project side property in viewmodel and recieve new values to chart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var addedprojects = MainWindow.MainWindowProjects.Where(w => w.ProjectName == ((CheckBox)sender).Content.ToString()).ToList();
            foreach (var item in addedprojects)
            {
                localProjects.Add(item);
            }


            var clientId = MainWindow.helper.ClientService.Get(addedprojects[0].ClientID).Id;
            var projectsbyclient = ((EditChartViewModel)DataContext).Projects.Where(w => w.Project.ClientID == clientId);

            bool clientUncheked = true;
            foreach (var item in projectsbyclient)
            {
                if (!item.IsSelected)
                {
                    clientUncheked = false;
                    break;
                }
            }

            if (clientUncheked)
            {
                var clientcheckbox = ((EditChartViewModel)DataContext).Clients.Where(w => w.Client.Id == clientId).ToList();
                foreach (var item in clientcheckbox)
                {
                    if (!item.IsSelected)
                    {
                        item.IsSelected = true;
                    }

                }
            }

            RefreshChart();

        }


        /// <summary>
        ///  EventHandler for CheckBox Uncheked event, changes the project side property in viewmodel and recieve new values to chart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            
            var deletedprojects = MainWindow.MainWindowProjects.Where(w => w.ProjectName == ((CheckBox)sender).Content.ToString()).ToList();
            foreach (var item in deletedprojects)
            {
                localProjects.Remove(item);
            }


            var clientId = MainWindow.helper.ClientService.Get(deletedprojects[0].ClientID).Id;
            var projectsbyclient = ((EditChartViewModel)DataContext).Projects.Where(w => w.Project.ClientID == clientId);

            bool clientUncheked = true;
            foreach (var item in projectsbyclient)
            {
                if (item.IsSelected)
                {
                    clientUncheked = false;
                    break;
                }
            }

            if(clientUncheked)
            {
                var clientcheckbox = ((EditChartViewModel)DataContext).Clients.Where(w => w.Client.Id == clientId).ToList();
                foreach (var item in clientcheckbox)
                {
                    if (item.IsSelected)
                    {
                        item.IsSelected = false;
                    }
                    
                }
            }

            RefreshChart();
        }


        /// <summary>
        /// EventHandler for CheckBox Cheked event, changes the client side property in viewmodel and recieve new values to chart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked_Client(object sender, RoutedEventArgs e)
        {
            var addedprojects = ((EditChartViewModel)DataContext).Projects.Where(w => w.Project.ClientID == MainWindow.helper.ClientService.GetByName(((CheckBox)sender).Content.ToString()).Id);
            foreach (var item in addedprojects)
            {
                item.IsSelected = true;
            }

        }

        /// <summary>
        /// EventHandler for CheckBox Cheked event, changes the client side property in viewmodel and recieve new values to chart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Unchecked_Client(object sender, RoutedEventArgs e)
        {
            var deletedproject = ((EditChartViewModel)DataContext).Projects.Where(w => w.Project.ClientID == MainWindow.helper.ClientService.GetByName(((CheckBox)sender).Content.ToString()).Id);
            foreach (var item in deletedproject)
            {
                item.IsSelected = false;
            }

        }

        /// <summary>
        /// Function that refreshs chart 
        /// </summary>
        private void RefreshChart()
        {

            MainWindow mainWindow = (MainWindow)this.Owner;
            mainWindow.RefreshChart(localProjects);


        }

        /// <summary>
        /// Function that refreshs local projects and viewmodel data context 
        /// </summary>
        public void Refresh()
        {
            editChartViewModel = new EditChartViewModel(MainWindow.MainWindowProjects, MainWindow.helper);
            DataContext = editChartViewModel;
            localProjects = MainWindow.MainWindowProjects.ToList();
        }
    }
}
