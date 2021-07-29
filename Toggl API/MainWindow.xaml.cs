using Csv;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Toggl;
using Toggl.Extensions;
using Toggl.QueryObjects;
using Toggl_API;
using Toggl_API.APIHelper;
using Toggl_API.APIHelper.ClassModel;
using Toggl_API.UserWindows.EditChart;
using Toggl_API.UserWindows.EditColor;

namespace Toggl_API
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region GlobalVariables
        public static Helper helper;//Helper Class
        public static ObservableCollection<ProjectChart> MainWindowProjects;//Observable Collection for editing chart and bar tooltip
        public EditChart editChart;//Edit window instance
        public (DateTime? Start, DateTime? End) oldDate;//Variable for saving previous dates

        private System.Windows.Forms.NotifyIcon notifyIcon;//To put app to windows tray
        private WindowState storedWindowState = WindowState.Normal;//Windows state
        #endregion


        public MainWindow()
        {

            InitializeComponent();
            //Global Variables initialization
            helper = new Helper();
            MainWindowProjects = new ObservableCollection<ProjectChart>();


            //Setting today's date and events to DatePicker's
            DatePick_Start.SelectedDate = DateTime.Now.Date;
            DatePick_End.SelectedDate = DateTime.Now.AddDays(1).Date;
            oldDate.Start = DateTime.Now.Date;
            oldDate.End = DateTime.Now.AddDays(1).Date;
            DatePick_Start.SelectedDateChanged += DatePick_Start_SelectedDateChanged;
            DatePick_End.SelectedDateChanged += DatePick_End_SelectedDateChanged;


            //Getting Today Data from Toggl API and Chart initialization
            InitializeChart();


            //Initialize NotifiIcon
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.BalloonTipText = "The app has been minimised. Click the tray icon to show.";
            notifyIcon.BalloonTipTitle = "Toggl Chart";
            notifyIcon.Text = "Toggl Chart";
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Toggl API;component/Resources/StackedColumnSeries.ico")).Stream;
            notifyIcon.Icon = new System.Drawing.Icon(iconStream);
            notifyIcon.Click += notifyIcon_Click;

        }

        public async void InitializeChart()
        {
            var projects = await helper.GetProjectChartAsync(DatePick_Start.SelectedDate, DatePick_End.SelectedDate);
            MainWindowProjects = new ObservableCollection<ProjectChart>(projects);
            LoadBarChartData(projects);
        }

        #region AppToTrayImplementation
        private void notifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = storedWindowState;

        }

        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }

        void ShowTrayIcon(bool show)
        {
            if (notifyIcon != null)
                notifyIcon.Visible = show;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CheckTrayIcon();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                if (notifyIcon != null)
                    notifyIcon.ShowBalloonTip(2000);
            }
            else
                storedWindowState = WindowState;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Dispose();
            notifyIcon = null;

        }

        #endregion


        #region ChartImplementation

        public void RefreshChart(List<ProjectChart> projectCharts)
        {

            LoadBarChartData(projectCharts);
        }



        private void LoadBarChartData(List<ProjectChart> projects)
        {

            WpfPlot.Plot.Clear();
            WpfPlot.Plot.Title("");
            WpfPlot.Plot.XTicks(new string[] { });

            if (projects.Count==0)
            {
                LoadingIndicator.IsActive = false;
                return;
            }

            Random rnd = new Random();
          
            //initialize
            List<double> initbarcount = new List<double>();
            List<double> initpositions = new List<double>();
            List<string> lables = new List<string>();
            List<double> timesums = new List<double>();
            List<int> ids = new List<int>();
            int counter = 0;

            foreach (var item in projects)
            {
                initbarcount.Add(0);
                initpositions.Add(counter);
                lables.Add(item.ProjectName);
                timesums.Add(item.TimeSum);
                ids.Add(item.ProjectID);
                counter++;

            }


            WpfPlot.Plot.AddBar(initbarcount.ToArray(), initpositions.ToArray());
            WpfPlot.Plot.XTicks(initpositions.ToArray(), lables.ToArray());
            WpfPlot.Plot.XAxis.TickLabelStyle(rotation: 30);
            
            for (int j = 0; j < initbarcount.ToArray().Length; j++)
            {

                var bar = WpfPlot.Plot.AddBar(new double[] {timesums[j] }, new double[] { initpositions[j] });
                bar.ShowValuesAboveBars = true;
                bar.FillColor = helper.GetColor(ids[j]);
                bar.Label = lables[j];
                var project = MainWindowProjects.First(w => w.ProjectID == ids[j]);
                project.X = initpositions[j];
                project.Y = timesums[j];
                project.BarWidth = bar.BarWidth;


            }

            
            WpfPlot.Plot.YLabel("Hours");
            WpfPlot.Plot.Title($"Total Hours: {timesums.Sum()}");
            WpfPlot.Plot.SetAxisLimits(yMin: 0);

            LoadingIndicator.IsActive = false;




        }

        private void WpfPlot_MouseMove(object sender, MouseEventArgs e)
        {

            (double x, double y) = WpfPlot.GetMouseCoordinates();
            
            foreach (var project in MainWindowProjects)
            {
                var px = project.X;
                var py = project.Y;
                var barwidth = project.BarWidth/2;

                bool exist_x = (x >= (px - barwidth) && x <= (px + barwidth));
                bool exist_y = (y >= 0 && y <= py);

                if (exist_x && exist_y)
                {
                   
                    TextBox textBox = new TextBox();
                    textBox.Text = $"{project.TasksToPointLabel()}";
                    PlotPopUp.Child = null;
                    PlotPopUp.Child = textBox;
                    PlotPopUp.IsOpen = true;

                    return;
                    
                   
                }
               
                
               
            }

            PlotPopUp.IsOpen = false;



        }





        private async void DatePick_Start_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            if (DatePick_Start.SelectedDate > DatePick_End.SelectedDate)
            {
                MessageBox.Show("StartDate can't be older then EndDate ", "Attention!", MessageBoxButton.OK);
                DatePick_Start.SelectedDate = oldDate.Start;
                return;
            }

            if (DatePick_End.SelectedDate!=null&& DatePick_Start.SelectedDate != null)
            {
                LoadingIndicator.IsActive = true;
                var projects = await helper.GetProjectChartAsync(DatePick_Start.SelectedDate, DatePick_End.SelectedDate);
                MainWindowProjects = new ObservableCollection<ProjectChart>(projects);
                if (editChart!=null)
                {
                    editChart.Refresh();
                }
                LoadBarChartData(projects);

                oldDate.Start = DatePick_Start.SelectedDate;
            }
        }

        private async void DatePick_End_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            

            if (DatePick_Start.SelectedDate>DatePick_End.SelectedDate)
            {
                MessageBox.Show("StartDate can't be older then EndDate ", "Attention!", MessageBoxButton.OK);
                DatePick_End.SelectedDate = oldDate.End;
                return;
            }

            if (DatePick_End.SelectedDate != null && DatePick_Start.SelectedDate != null)
            {

                LoadingIndicator.IsActive = true;
                var projects =await helper.GetProjectChartAsync(DatePick_Start.SelectedDate, DatePick_End.SelectedDate);
                MainWindowProjects = new ObservableCollection<ProjectChart>(projects);
                if (editChart != null)
                {
                    editChart.Refresh();
                }
                LoadBarChartData(projects);
                oldDate.End = DatePick_End.SelectedDate;
            }
        }

        private async void UpdateDateWithoutPickerTriggers(DateTime startDate, DateTime endDate)
        {
            DatePick_Start.SelectedDateChanged -= DatePick_Start_SelectedDateChanged;
            DatePick_End.SelectedDateChanged -= DatePick_End_SelectedDateChanged;

            DatePick_End.SelectedDate = endDate;
            DatePick_Start.SelectedDate = startDate;

            LoadingIndicator.IsActive = true;
            var projects = await helper.GetProjectChartAsync(startDate, endDate);
            MainWindowProjects = new ObservableCollection<ProjectChart>(projects);
            LoadBarChartData(projects);
            DatePick_End.SelectedDateChanged += DatePick_End_SelectedDateChanged;
            DatePick_Start.SelectedDateChanged += DatePick_Start_SelectedDateChanged;

            oldDate.Start = startDate;
            oldDate.End = endDate;

        }

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = ((DateTime)DatePick_Start.SelectedDate).AddDays(-1).Date;
            DateTime endDate = ((DateTime)DatePick_End.SelectedDate).AddDays(-1).Date;
            UpdateDateWithoutPickerTriggers(startDate, endDate);

        }

        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = ((DateTime)DatePick_Start.SelectedDate).AddDays(1).Date;
            DateTime endDate = ((DateTime)DatePick_End.SelectedDate).AddDays(1).Date;
            UpdateDateWithoutPickerTriggers(startDate, endDate);

        }



        private void CurrMonth_Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime now = DateTime.Now.Date;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
            UpdateDateWithoutPickerTriggers(startDate, endDate);

        }


        private void CurrWeek_Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime now = DateTime.Now.Date;
            var startDate = now.FirstDayOfWeek();
            var endDate = now.LastDayOfWeek().AddHours(23).AddMinutes(59).AddSeconds(59);
            UpdateDateWithoutPickerTriggers(startDate, endDate);
        }

        private void CurrDay_Button_Click(object sender, RoutedEventArgs e)
        {
            var startDate = DateTime.Now.Date;
            var endDate = DateTime.Now.AddDays(1).Date;
            UpdateDateWithoutPickerTriggers(startDate, endDate);
        }

        #endregion


        #region ExportToCVS

        private void ExportCVS_MenuItem_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            saveFileDialog.FileName = $"{((DateTime)helper.Projects.Min(w => w.UpdatedOn)).ToShortDateString()} - {DateTime.Now.AddDays(1).ToShortDateString()}";
            saveFileDialog.DefaultExt = ".csv"; 


            if (saveFileDialog.ShowDialog()==true)
            {

                var columnNames = new[] {"Date","ProjectName", "Hours","Tasks" };
                var projectCharts = helper.GetProjectChartFull();
                if (projectCharts.Count == 0)
                {
                    MessageBox.Show("Empty Chart!");
                    return;
                }
                List<string[]> ArrayList = new List<string[]>();
                foreach (var projectchart in projectCharts)
                {
                    var dateDistinc = projectchart.GetDistinctDate();
                    foreach (var dateTime in dateDistinc)
                    {

                        var project = projectchart.GetProjectByDate(dateTime);
                        ArrayList.Add(new[] { $"{dateTime.ToShortDateString()}", project.ProjectName, project.TimeSum.ToString(), project.TasksToCsv() });

                    }

                }
                var csv = CsvWriter.WriteToText(columnNames, ArrayList.ToArray(), ';');
                StringBuilder stringBuilder = new StringBuilder(csv);
                stringBuilder.Insert(0, Environment.NewLine);
                stringBuilder.Insert(0,$"Date Range: {((DateTime)helper.Projects.Min(w => w.UpdatedOn)).ToShortDateString()} - {DateTime.Now.AddDays(1).ToShortDateString()}");
                File.WriteAllText(saveFileDialog.FileName, stringBuilder.ToString(),Encoding.UTF8);

            }






        }

        private void ExportChartCVS_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            saveFileDialog.FileName = $"{((DateTime)DatePick_Start.SelectedDate).ToShortDateString()} - {((DateTime)DatePick_End.SelectedDate).ToShortDateString()}";
            saveFileDialog.DefaultExt = ".csv";



            if (saveFileDialog.ShowDialog() == true)
            {

                var columnNames = new[] { "Date", "ProjectName", "Hours", "Tasks" };

                List<ProjectChart> projectCharts = new List<ProjectChart>();
                if (editChart!=null)
                {
                    projectCharts = EditChart.localProjects;
                }
                else
                {
                    projectCharts= helper.GetProjectChart(DatePick_Start.SelectedDate, DatePick_End.SelectedDate);
                }

                if (projectCharts.Count==0)
                {
                    MessageBox.Show("Empty Chart!");
                    return;
                }

                string[][] rows = new string[projectCharts.Count][];
                for (int i = 0; i < projectCharts.Count; i++)
                {
                    rows[i] = new[] { $"{((DateTime)DatePick_Start.SelectedDate).ToShortDateString()} - {((DateTime)DatePick_End.SelectedDate).ToShortDateString()}", projectCharts[i].ProjectName, projectCharts[i].TimeSum.ToString(), projectCharts[i].TasksToCsv() };
                }
                var csv = CsvWriter.WriteToText(columnNames, rows, ';');
                StringBuilder stringBuilder = new StringBuilder(csv);
                stringBuilder.Insert(0, Environment.NewLine);
                stringBuilder.Insert(0, $"Date Range: {((DateTime)DatePick_Start.SelectedDate).ToShortDateString()} - {((DateTime)DatePick_End.SelectedDate).ToShortDateString()}");
                File.WriteAllText(saveFileDialog.FileName, stringBuilder.ToString(), Encoding.UTF8);

            }
        }

        private void ExportReportByDayCVS_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            saveFileDialog.FileName = $"{((DateTime)DatePick_Start.SelectedDate).ToShortDateString()} - {((DateTime)DatePick_End.SelectedDate).ToShortDateString()}";
            saveFileDialog.DefaultExt = ".csv";



            if (saveFileDialog.ShowDialog() == true)
            {

                var columnNames = new[] { "Date", "Project", "Hours","Description" };

                List<ProjectChart> projectCharts = new List<ProjectChart>();
                if (editChart != null)
                {
                    projectCharts = EditChart.localProjects;
                }
                else
                {
                    projectCharts = helper.GetProjectChartAsync(DatePick_Start.SelectedDate, DatePick_End.SelectedDate).Result;
                }

                if (projectCharts.Count == 0)
                {
                    MessageBox.Show("Empty Chart!");
                    return;
                }

                int days = (((DateTime)DatePick_End.SelectedDate).Date - ((DateTime)DatePick_Start.SelectedDate).Date).Days+1;
                List<DateTime> dateTimes = new List<DateTime>();
                DateTime max = ((DateTime)DatePick_End.SelectedDate).Date;

                for(int i = 0; i < days; i++)
                {
                    dateTimes.Add(max.AddDays(-i));
                }

                var dateAray = dateTimes.ToArray().Reverse().ToArray();
                List<string[]> rows = new List<string[]>();
           

                foreach (var date in dateAray)
                {
                  
                    foreach (var project in projectCharts)
                    {
                        if (project.IsProjectHasTask(date))
                        {
                            rows.Add(new[] { $"{date.ToShortDateString()}", project.ProjectName, project.GetTimeSum(date).ToString(), project.TasksToCsv(date) });
                        }
                    }

                }


                var csv = CsvWriter.WriteToText(columnNames, rows, ';');
                StringBuilder stringBuilder = new StringBuilder(csv);
                stringBuilder.Insert(0, Environment.NewLine);
                stringBuilder.Insert(0, $"Date Range: {((DateTime)DatePick_Start.SelectedDate).ToShortDateString()} - {((DateTime)DatePick_End.SelectedDate).ToShortDateString()}");
                File.WriteAllText(saveFileDialog.FileName, stringBuilder.ToString(), Encoding.UTF8);

            }
        }


        #endregion

        private void Edit_Chart_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            editChart = new EditChart();
            editChart.Owner = this;
            editChart.Show();
        }


        private void Edit_Color_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var editcolor = new EditColor();
            editcolor.Owner = this;
            editcolor.Show();
        }

        public void Refresh()
        {
            UpdateDateWithoutPickerTriggers((DateTime)DatePick_Start.SelectedDate, (DateTime)DatePick_End.SelectedDate);
        }

        private void Refresh_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
      
        }



    }
}
