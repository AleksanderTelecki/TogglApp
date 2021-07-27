using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toggl_API.APIHelper.ClassModel
{
    public class ProjectChart
    {
        public string ProjectName { get; set; }

        public  List<TimePerTask> TimePerTasks{ get; set; }

        public double TimeSum { get => GetTimeSum(); }

        public int ProjectID { get; set; }

        public int ClientID { get; set; }

        public double X { get; set; }

        public double Y { get; set; }
        public double BarWidth { get; set; }


        public ProjectChart(string projectName,List<TimePerTask> timepertasks,int id)
        {
            ProjectID = id;
            ProjectName = projectName;
            TimePerTasks = timepertasks;
            TimePerTasks = new List<TimePerTask>();

        }

        public ProjectChart(string projectName,int id)
        {
            ProjectID = id;
            ProjectName = projectName;
            TimePerTasks = new List<TimePerTask>();

        }

        public ProjectChart()
        {
            TimePerTasks = new List<TimePerTask>();
        }

        public void AddTask(string description,double time,DateTime date)
        {
            TimePerTasks.Add(new TimePerTask(description, time, date));

        }

        private double GetTimeSum()
        {
            double sum = 0;
            foreach (var item in TimePerTasks)
            {
                sum += item.Time;
            }

            return Math.Round(sum * 4, MidpointRounding.ToEven) / 4;
        }

        public double GetTimeSum(DateTime date)
        {

            double sum = 0;
            foreach (var item in TimePerTasks.Where(w=>w.Date.Date==date))
            {
                sum += item.Time;
            }

            return Math.Round(sum * 4, MidpointRounding.ToEven) / 4;

        }

        public bool IsProjectHasTask(DateTime date)
        {
            bool result = false;
            if (GetTimeSum(date)!=0)
            {
                result = true;
            }

            return result;
        }

        public string TasksToCsv()
        {

            StringBuilder sb = new StringBuilder();
            int taskswithoutnames = 0;
            foreach (var item in TimePerTasks)
            {
               
                if (String.IsNullOrEmpty(item.Description))
                {
                    taskswithoutnames++;
                }
                else
                {
                    sb.Append($"{item.Description}, ");
                }
               
                

            }

            if (taskswithoutnames!=0)
            {
                
                sb.Append($"{taskswithoutnames} - Task Without Description, ");
            }

            return sb.ToString();

        }

        public string TasksToPointLabel()
        {

            StringBuilder sb = new StringBuilder();
            int taskswithoutnames = 0;
            foreach (var item in TimePerTasks)
            {

                if (String.IsNullOrEmpty(item.Description))
                {
                    taskswithoutnames++;
                }
                else
                {
                    sb.AppendLine($"{item.Description}");
                }



            }

            if (taskswithoutnames != 0)
            {

                sb.AppendLine($"{taskswithoutnames} - Task Without Description");
            }

            return sb.ToString();

        }



        public string TasksToCsv(DateTime date)
        {

            StringBuilder sb = new StringBuilder();
            int taskswithoutnames = 0;
            foreach (var item in TimePerTasks.Where(w=>w.Date.Date==date))
            {

                if (String.IsNullOrEmpty(item.Description))
                {
                    taskswithoutnames++;
                }
                else
                {
                    sb.Append($"{item.Description}, ");
                }



            }

            if (taskswithoutnames != 0)
            {

                sb.Append($"{taskswithoutnames} - Task Without Description, ");
            }

            return sb.ToString();

        }

        public List<DateTime> GetDistinctDate()
        {

            var dateDistinct = TimePerTasks.Select(o => o.ShortDateString).Distinct().ToList();
            List<DateTime> dateTimes = new List<DateTime>();
            foreach (var item in dateDistinct)
            {
                dateTimes.Add(Convert.ToDateTime(item));
            }

            return dateTimes;

        }

        public ProjectChart GetProjectByDate(DateTime dateTime)
        {
            ProjectChart projectChart = new ProjectChart(ProjectName, ProjectID);
            projectChart.TimePerTasks = this.TimePerTasks.Where(w => w.ShortDateString == dateTime.ToShortDateString()).ToList();

            return projectChart;

        }

    }

    public class TimePerTask
    {
        public string Description { get; set; }

        public double Time { get; set; }

        public DateTime Date { get; set; }

        public string ShortDateString { get => Date.ToShortDateString(); }


        public TimePerTask(string description,double time, DateTime date)
        {
            Description = description;
            Time =time;
            Date = date;
        }

        public override string ToString()
        {
            return base.ToString();
        }


    }
}
