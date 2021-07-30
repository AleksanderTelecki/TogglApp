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


        /// <summary>
        /// Method for getting sum of time entries in tasks
        /// </summary>
        /// <returns>Double Time Sum</returns>
        private double GetTimeSum()
        {
            double sum = 0;
            foreach (var item in TimePerTasks)
            {
                sum += item.Time;
            }

            return Math.Round(sum * 4, MidpointRounding.ToEven) / 4;
        }

        /// <summary>
        /// Method for getting sum of time entries in tasks by date
        /// </summary>
        /// <param name="date">Specified Date</param>
        /// <returns>Double Time Sum by date</returns>
        public double GetTimeSum(DateTime date)
        {

            double sum = 0;
            foreach (var item in TimePerTasks.Where(w=>w.Date.Date==date))
            {
                sum += item.Time;
            }

            return Math.Round(sum * 4, MidpointRounding.ToEven) / 4;

        }

        /// <summary>
        /// Method that checks is project has active tasks in the range of specified date
        /// </summary>
        /// <param name="date">Specified Date</param>
        /// <returns>Boolean value that represents is project has active task in the range of specified date</returns>
        public bool IsProjectHasTask(DateTime date)
        {
            bool result = false;
            if (GetTimeSum(date)!=0)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Method that converts task descriptions to cvs format 
        /// </summary>
        /// <returns>String with tasks description in cvs format</returns>
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

        /// <summary>
        /// Method that converts tasks description to label point format
        /// </summary>
        /// <returns>String with tasks description which converted to label point format </returns>
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


       
        /// <summary>
        ///  Method that converts task descriptions to cvs format by specified date
        /// </summary>
        /// <param name="date">Specified Date</param>
        /// <returns>String with tasks description in cvs format by date</returns>
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


        /// <summary>
        /// Method that gets list with task distinct date 
        /// </summary>
        /// <returns>List of task distinct date</returns>
        public List<DateTime> GetDistinctDate()
        {

            var dateDistinct = TimePerTasks.Select(o => o.Date.Date).Distinct().ToList();
            List<DateTime> dateTimes = new List<DateTime>();
            foreach (var item in dateDistinct)
            {
                dateTimes.Add(Convert.ToDateTime(item));
            }

            return dateTimes;

        }

        /// <summary>
        /// Method that gets ProjectChart object by date
        /// </summary>
        /// <param name="dateTime">Specified Date</param>
        /// <returns>ProjectChart by date</returns>
        public ProjectChart GetProjectByDate(DateTime dateTime)
        {
            ProjectChart projectChart = new ProjectChart(ProjectName, ProjectID);
            projectChart.TimePerTasks = this.TimePerTasks.Where(w => w.Date.Date == dateTime.Date).ToList();

            return projectChart;

        }

    }

    public class TimePerTask
    {
        public string Description { get; set; }

        public double Time { get; set; }

        public DateTime Date { get; set; }


        public TimePerTask(string description,double time, DateTime date)
        {
            Description = description;
            Time =time;
            Date = date;
        }

    }
}
