using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toggl_API.APIHelper.ChartClasses
{
    public class ProjectChart
    {
        public string ProjectName { get; set; }

        public  List<TimePerTask> TimePerTasks{ get; set; }

        

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

        public void AddTask(string description,double time)
        {
            TimePerTasks.Add(new TimePerTask(description, time));

        }

        public double GetTimeSum()
        {
            double sum = 0;
            foreach (var item in TimePerTasks)
            {
                sum += item.Time;
            }

            return sum;
        }

    }

    public class TimePerTask
    {
        public string Description { get; set; }

        public double Time { get; set; }

        public TimePerTask(string description,double time)
        {
            Description = description;
            Time = time;
        }


    }
}
