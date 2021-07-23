using Csv;
using Microsoft.Win32;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        public static Helper helper;
        public static ObservableCollection<ProjectChart> MainWindowProjects;
        public EditChart editChart;
        public (DateTime? Start, DateTime? End) oldDate;

        public MainWindow()
        {

            InitializeComponent();
            helper = new Helper();
            MainWindowProjects = new ObservableCollection<ProjectChart>();


            DatePick_Start.SelectedDate = DateTime.Now.Date;
            DatePick_End.SelectedDate = DateTime.Now.AddDays(1).Date;
            oldDate.Start = DateTime.Now.Date;
            oldDate.End = DateTime.Now.AddDays(1).Date;
            DatePick_Start.SelectedDateChanged += DatePick_Start_SelectedDateChanged;
            DatePick_End.SelectedDateChanged += DatePick_End_SelectedDateChanged;

            var projects = helper.GetProjectChart(DatePick_Start.SelectedDate, DatePick_End.SelectedDate);
            LoadBarChartData(projects);
            MainWindowProjects = new ObservableCollection<ProjectChart>(projects);

            // TODO: EditWindow Synchronization with uncheked checkboxes when close the window

            //Trace.WriteLine(helper.GetClientProjectTimeTrack("Klient 1", "07/05/2021", "07/08/2021"));
            //Trace.WriteLine("");
            //Trace.WriteLine(helper.GetTotalProjectWorkTime(helper.Projects[0], "07/05/2021", "07/08/2021"));
            //LoadBarChartData(helper.GetProjectChart(helper.Projects[0]), helper.GetProjectChart(helper.Projects[1]));



        }

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


            }


            WpfPlot.Plot.YLabel("Hours");
            WpfPlot.Plot.Title($"Total Hours: {timesums.Sum()}");
            WpfPlot.Plot.SetAxisLimits(yMin: 0);






        }

        private void DatePick_Start_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            if (DatePick_Start.SelectedDate > DatePick_End.SelectedDate)
            {
                MessageBox.Show("StartDate can't be older then EndDate ", "Attention!", MessageBoxButton.OK);
                DatePick_Start.SelectedDate = oldDate.Start;
                return;
            }

            if (DatePick_End.SelectedDate!=null&& DatePick_Start.SelectedDate != null)
            {
                var projects = helper.GetProjectChart(DatePick_Start.SelectedDate, DatePick_End.SelectedDate);
                MainWindowProjects = new ObservableCollection<ProjectChart>(projects);
                if (editChart!=null)
                {
                    editChart.Refresh();
                }
                LoadBarChartData(projects);

                oldDate.Start = DatePick_Start.SelectedDate;
            }
        }

        private void DatePick_End_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            

            if (DatePick_Start.SelectedDate>DatePick_End.SelectedDate)
            {
                MessageBox.Show("StartDate can't be older then EndDate ", "Attention!", MessageBoxButton.OK);
                DatePick_End.SelectedDate = oldDate.End;
                return;
            }

            if (DatePick_End.SelectedDate != null && DatePick_Start.SelectedDate != null)
            {
                var projects = helper.GetProjectChart(DatePick_Start.SelectedDate, DatePick_End.SelectedDate);
                MainWindowProjects = new ObservableCollection<ProjectChart>(projects);
                if (editChart != null)
                {
                    editChart.Refresh();
                }
                LoadBarChartData(projects);
                oldDate.End = DatePick_End.SelectedDate;
            }
        }

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

                var columnNames = new[] { "Date", "Hours", "Projects" };

                List<ProjectChart> projectCharts = new List<ProjectChart>();
                if (editChart != null)
                {
                    projectCharts = EditChart.localProjects;
                }
                else
                {
                    projectCharts = helper.GetProjectChart(DatePick_Start.SelectedDate, DatePick_End.SelectedDate);
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
                string[][] rows = new string[days][];
                for (int i = 0; i < dateAray.Length; i++)
                {

                    double hours = 0;
                    StringBuilder projects = new StringBuilder();
                    projects.Append("");
                    foreach (var item in projectCharts)
                    {
                        hours += item.GetTimeSum(dateAray[i]);
                        string projectname = item.IsProjectWasInWork(dateAray[i]);
                        if (!String.IsNullOrEmpty(projectname))
                        {
                            projects.Append($"{projectname}, ");
                        }
                        
                    }

                    hours = Math.Round(hours * 4, MidpointRounding.ToEven) / 4;
                    rows[i] = new[] { $"{dateAray[i].ToShortDateString()}", hours.ToString(), projects.ToString() };
                }
                var csv = CsvWriter.WriteToText(columnNames, rows, ';');
                StringBuilder stringBuilder = new StringBuilder(csv);
                stringBuilder.Insert(0, Environment.NewLine);
                stringBuilder.Insert(0, $"Date Range: {((DateTime)DatePick_Start.SelectedDate).ToShortDateString()} - {((DateTime)DatePick_End.SelectedDate).ToShortDateString()}");
                File.WriteAllText(saveFileDialog.FileName, stringBuilder.ToString(), Encoding.UTF8);

            }
        }




        private void Edit_Chart_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            editChart = new EditChart();
            editChart.Owner = this;
            editChart.Show();
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

        private void UpdateDateWithoutPickerTriggers(DateTime startDate,DateTime endDate)
        {
            DatePick_Start.SelectedDateChanged -= DatePick_Start_SelectedDateChanged;
            DatePick_End.SelectedDateChanged -= DatePick_End_SelectedDateChanged;

            DatePick_End.SelectedDate = endDate;
            DatePick_Start.SelectedDate = startDate;

            var projects = helper.GetProjectChart(startDate, endDate);
            LoadBarChartData(projects);
            MainWindowProjects = new ObservableCollection<ProjectChart>(projects);
            DatePick_End.SelectedDateChanged += DatePick_End_SelectedDateChanged;
            DatePick_Start.SelectedDateChanged += DatePick_Start_SelectedDateChanged;

            oldDate.Start = startDate;
            oldDate.End = endDate;

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

        private void Refresh_MenuItem_Click(object sender, RoutedEventArgs e)
        {

            UpdateDateWithoutPickerTriggers((DateTime)DatePick_Start.SelectedDate, (DateTime)DatePick_End.SelectedDate);
        }

        public void Refresh()
        {
            UpdateDateWithoutPickerTriggers((DateTime)DatePick_Start.SelectedDate, (DateTime)DatePick_End.SelectedDate);
        }


        private void Edit_Color_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var editcolor = new EditColor();
            editcolor.Owner = this;
            editcolor.Show();
        }

        




        //Project
        //var project = new Project
        //{
        //    IsBillable = true,
        //    WorkspaceId = WorkspaceID,
        //    Name = "Project 2",
        //    IsAutoEstimates = false,
        //    ClientId = clients[0].Id,
        //    IsActive = true,
        //};
        //var act = PService.Add(project);









        //var TService = new Toggl.Services.TaskService(apiKey);
        //var task = TService.ForProject((int)ProjectID);
        //var loadedTask = TService.Get((int)task[0].Id);
        //loadedTask.IsActive = false;
        //var editedTask = TService.Edit(loadedTask);

        //helper.AddTask((int) helper.Projects[0].Id, "Requirements Specification",true,3600*20);
        //    helper.AddTask((int) helper.Projects[0].Id, "Project Planning", true, 3600*10);
        //    helper.AddTask((int) helper.Projects[0].Id, "System Creation", true, 3600*70);
        //    helper.AddTask((int) helper.Projects[0].Id, "DataBase Creation", true, 3600*30);
        //    helper.AddTask((int) helper.Projects[0].Id, "BugFixing", true, 3600*35);
        //    helper.AddTask((int) helper.Projects[0].Id, "Testing", true, 3600*25);

        //    helper.AddTask((int) helper.Projects[1].Id, "Requirements Specification", true, 3600*20);
        //    helper.AddTask((int) helper.Projects[1].Id, "Project Planning", true, 3600*12);
        //    helper.AddTask((int) helper.Projects[1].Id, "System Designing", true, 3600*15);
        //    helper.AddTask((int) helper.Projects[1].Id, "Server Creation", true, 3600*30);
        //    helper.AddTask((int) helper.Projects[1].Id, "Web-Services Configuration", true, 3600*35);
        //    helper.AddTask((int) helper.Projects[1].Id, "System Configuration", true, 3600*10);
        //    helper.AddTask((int) helper.Projects[1].Id, "Testing", true, 3600*10);

        //helper.AddTimeEntries(helper.Projects[0], "Requirements Specification", 3600 * 1, "Client Meeting 0.1");
        //helper.AddTimeEntries(helper.Projects[0], "Project Planning", 3600 * 2, "Gant Diagram 0.1");
        //helper.AddTimeEntries(helper.Projects[0], "System Creation", 3600 * 4, "System Main UseCase 0.1");
        //helper.AddTimeEntries(helper.Projects[0], "DataBase Creation", 3600 * 5, "Entity Diagram 0.1");
        //helper.AddTimeEntries(helper.Projects[0], "BugFixing", 3600 * 1, "System Check 0.1");
        //helper.AddTimeEntries(helper.Projects[0], "Testing", 3600 * 1, "Testing 0.1");



        //helper.AddTimeEntries(helper.Projects[1], "Requirements Specification", 3600 * 2, "Client Meeting 0.1");
        //helper.AddTimeEntries(helper.Projects[1], "Project Planning", 3600 * 3, "Main Project Plan 0.1");
        //helper.AddTimeEntries(helper.Projects[1], "System Designing", 3600 * 4, "Designer UI Template 0.1");
        //helper.AddTimeEntries(helper.Projects[1], "Server Creation", 3600 * 5, "Server Creation 0.1");
        //helper.AddTimeEntries(helper.Projects[1], "Web-Services Configuration", 3600 * 5, "Adding WebServices 0.1");
        //helper.AddTimeEntries(helper.Projects[1], "System Configuration", 3600 * 3, "System Configuration 0.1");
        //helper.AddTimeEntries(helper.Projects[1], "Testing", 3600 * 2, "Testing 0.1");

        //helper.AddTimeEntries(helper.Projects[0], "Requirements Specification", 3600 * 1, "Requirements Analyse");
        //helper.AddTimeEntries(helper.Projects[0], "Project Planning", 3600 * 2, "Gant Diagram 0.2");
        //helper.AddTimeEntries(helper.Projects[0], "System Creation", 3600 * 4, "System Main Class Diagram");
        //helper.AddTimeEntries(helper.Projects[0], "DataBase Creation", 3600 * 5, "DB projecting");
        //helper.AddTimeEntries(helper.Projects[0], "BugFixing", 3600 * 1, "System Check 0.2");
        //helper.AddTimeEntries(helper.Projects[0], "Testing", 3600 * 1, "Testing 0.2");



        //helper.AddTimeEntries(helper.Projects[1], "Requirements Specification", 3600 * 2, "Client Meeting 0.2");
        //helper.AddTimeEntries(helper.Projects[1], "Project Planning", 3600 * 3, "Team managment 0.1");
        //helper.AddTimeEntries(helper.Projects[1], "System Designing", 3600 * 1, "Designer UI Template 0.2");
        //helper.AddTimeEntries(helper.Projects[1], "Server Creation", 3600 * 2, "Server Configuration 0.1");
        //helper.AddTimeEntries(helper.Projects[1], "Web-Services Configuration", 3600 * 1, "WebServices Configuration 0.1");
        //helper.AddTimeEntries(helper.Projects[1], "System Configuration", 3600 * 3, "System Configuration 0.2");
        //helper.AddTimeEntries(helper.Projects[1], "Testing", 3600 * 2, "Testing 0.2");




        //var reports = new Toggl.Services.ReportService(Helper.APIToken);


        //var standardParams = new DetailedReportParams()
        //{
        //    UserAgent = "TogglAPI.Net",
        //    WorkspaceId = (int)helper.WorkSpaceID
        //};

        //var z = reports.Detailed(standardParams);




    }
}
