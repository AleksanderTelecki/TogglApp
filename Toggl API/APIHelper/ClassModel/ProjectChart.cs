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

        public int ClientID { get; set; }

        public ProjectChart(string projectName,List<TimePerTask> timepertasks)
        {
            ProjectName = projectName;
            TimePerTasks = timepertasks;
            TimePerTasks = new List<TimePerTask>();

        }

        public ProjectChart(string projectName)
        {
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

            return sum;
        }

        public string TasksToCsv()
        {

            StringBuilder sb = new StringBuilder();
            foreach (var item in TimePerTasks)
            {
                sb.Append($"({item.Time} h)-{item.Description}, ");


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
            ProjectChart projectChart = new ProjectChart(ProjectName);
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
            Time =Math.Round(time * 4, MidpointRounding.ToEven) / 4; ;
            Date = date;
        }

        public override string ToString()
        {
            return base.ToString();
        }


    }
}
