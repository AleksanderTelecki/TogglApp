﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Toggl;
using Toggl.Extensions;
using Toggl.QueryObjects;
using Toggl_API;
using Toggl_API.APIHelper;
using Toggl_API.APIHelper.ChartClasses;

namespace Toggl_API
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {

            InitializeComponent();

         

            var helper = new Helper();
            Debug.WriteLine(helper.GetClientProjectTimeTrack("Klient 1", "07/05/2021", "07/08/2021"));
            Debug.WriteLine("");
            Debug.WriteLine(helper.GetTotalProjectWorkTime(helper.Projects[0], "07/05/2021", "07/08/2021"));
            LoadBarChartData(helper.GetProjectChart(helper.Projects[0]));


            








        }


        private void LoadBarChartData(ProjectChart projectChart)
        {

            List<KeyValuePair<string, double>> keyValuePairs = new List<KeyValuePair<string, double>>();
            foreach (var item in projectChart.TimePerTasks)
            {
                keyValuePairs.Add(new KeyValuePair<string, double>(item.Description,item.Time));
            }

            ((BarSeries)mcChart.Series[0]).ItemsSource = keyValuePairs;
            mcChart.Title = projectChart.ProjectName;


        }




        // xmlns:DVC="clr-namespace:System.Windows.Controls.DataVisualization.Charting.Compatible;assembly=DotNetProjects.DataVisualization.Toolkit"
        // xmlns:DVC="clr-namespace:System.Windows.Controls.DataVisualization.Charting.Primitives;assembly=DotNetProjects.DataVisualization.Toolkit"



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


    }
}
