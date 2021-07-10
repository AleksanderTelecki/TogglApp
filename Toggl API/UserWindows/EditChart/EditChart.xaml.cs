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
        EditChartViewModel editChartViewModel;
        List<ProjectChart> localProjects;
        public EditChart()
        {
            InitializeComponent();

            editChartViewModel = new EditChartViewModel(MainWindow.MainWindowProjects, MainWindow.helper);
            DataContext = editChartViewModel;
            localProjects = MainWindow.MainWindowProjects.ToList();
        }

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

        private void CheckBox_Checked_Client(object sender, RoutedEventArgs e)
        {
            var addedprojects = ((EditChartViewModel)DataContext).Projects.Where(w => w.Project.ClientID == MainWindow.helper.ClientService.GetByName(((CheckBox)sender).Content.ToString()).Id);
            foreach (var item in addedprojects)
            {
                item.IsSelected = true;
            }

        }

        private void CheckBox_Unchecked_Client(object sender, RoutedEventArgs e)
        {
            var deletedproject = ((EditChartViewModel)DataContext).Projects.Where(w => w.Project.ClientID == MainWindow.helper.ClientService.GetByName(((CheckBox)sender).Content.ToString()).Id);
            foreach (var item in deletedproject)
            {
                item.IsSelected = false;
            }

        }

        private void RefreshChart()
        {

            MainWindow mainWindow = (MainWindow)this.Owner;
            mainWindow.RefreshChart(localProjects);


        }

        public void Refresh()
        {
            editChartViewModel = new EditChartViewModel(MainWindow.MainWindowProjects, MainWindow.helper);
            DataContext = editChartViewModel;
            localProjects = MainWindow.MainWindowProjects.ToList();
        }
    }
}
