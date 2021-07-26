using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toggl;
using Toggl.Extensions;
using Toggl.QueryObjects;
using Toggl_API.APIHelper.ClassModel;

namespace Toggl_API.APIHelper
{
    public class Helper
    {

        public static string APIToken { get => ConfigurationManager.AppSettings["APIkey"]; }

        public Toggl.Services.ProjectService ProjectService { get; private set; }

        public Toggl.Services.TaskService TaskService { get ; private set; }

        public Toggl.Services.TimeEntryService TimeEntryService { get ; private set; }

        public  Toggl.Services.ClientService ClientService { get ; private set; }

        public  Toggl.Services.WorkspaceService WorkspaceService { get ; private set; }

        public int? WorkSpaceID { get; private set; }

        public List<Toggl.Project> Projects { get => ProjectService.List(); }

        public List<Toggl.Client> Clients { get => ClientService.List(); }

        public List<ProjectColor> projectColors { get; set; }


        public Helper()
        {
            ProjectService = new Toggl.Services.ProjectService(APIToken);
            TaskService = new Toggl.Services.TaskService(APIToken);
            TimeEntryService = new Toggl.Services.TimeEntryService(APIToken);
            ClientService = new Toggl.Services.ClientService(APIToken);
            WorkspaceService = new Toggl.Services.WorkspaceService(APIToken);
            WorkSpaceID = WorkspaceService.List()[0].Id;
            projectColors = new List<ProjectColor>();
            InitializeColors();

        }

        private void InitializeColors()
        {
           
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "color.json");
            if (File.Exists(filePath))
            {
                List<ProjectColor> descolors = JSONDeserialize<List<ProjectColor>>(filePath);
                updateColors(descolors, filePath);
            }
            else
            {
                foreach (var item in Projects)
                {
                    projectColors.Add(new ProjectColor(item.Name, (int)item.Id));
                }
                JSONSerialize(projectColors, filePath);
            }

        }

        public Color GetColor(int id)
        {
            Color result = projectColors.First(w => w.ID == id).GetCurrentColor();
            if (result==null)
            {
                updateColors(projectColors, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "color.json"));
                result = projectColors.First(w => w.ID == id).GetCurrentColor();
            }
            return result;
        }

        private void updateColors(List<ProjectColor> updColors,string filePath)
        {

            bool allprojectsin = true;
            foreach (var item in Projects)
            {
                if (!updColors.Exists(w => (w.ID == item.Id) && (w.Name == item.Name)))
                {
                    updColors.Add(new ProjectColor(item.Name, (int)item.Id));
                    allprojectsin = false;
                }
            }
         
            if (!allprojectsin)
            {
                JSONSerialize(projectColors, filePath);
            }

            projectColors = updColors;

        }

        public void SaveColors()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "color.json");
            JSONSerialize(projectColors, filePath);
        }

        public void SaveColors(string filePath)
        {
            JSONSerialize(projectColors, filePath);
        }

        private void JSONSerialize(object data,string filePath)
        {

            JsonSerializer jsonSerializer = new JsonSerializer();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                JsonWriter jsonWriter = new JsonTextWriter(sw);
                jsonSerializer.Serialize(jsonWriter, data);
            }

        }

        private T JSONDeserialize<T>(string filePath)
        {
            object jObject = null;
            JsonSerializer jsonSerializer = new JsonSerializer();
            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    JsonReader jsonReader = new JsonTextReader(sr);
                    jObject = jsonSerializer.Deserialize(jsonReader);
                    
                }
            }
            var x = JsonConvert.DeserializeObject<T>(jObject.ToString());
            return x;


        }

        public string GetClientProjectTimeTrack(string Name, string startDate, string endDate)
        {
            StringBuilder result = new StringBuilder();

            var prams = new TimeEntryParams();

            prams.StartDate = Convert.ToDateTime(startDate);
            prams.EndDate = Convert.ToDateTime(endDate);

            var hours = TimeEntryService.List(prams);

            var client = ClientService.GetByName(Name);
            var clientproj = ProjectService.List().Where(w => w.ClientId == client.Id);




            result.AppendLine($"{client.Name}");
            foreach (var item in clientproj)
            {
                result.AppendLine($"   {item.Name}");
                var choosedtimestamp = hours.Where(w => w.ProjectId == item.Id).ToList();
                var choosedtasks = TaskService.ForProject((int)item.Id);
                foreach (var timestamp in choosedtimestamp)
                {
                    DateTime start = DateTime.Parse(timestamp.Start, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    DateTime stop = DateTime.Parse(timestamp.Stop, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    timestamp.TaskName = GetTaskNameByID((int)timestamp.TaskId, choosedtasks);
                  
                    result.AppendLine($"({timestamp.TaskName}) Duration: {TimeSpan.FromSeconds((double)timestamp.Duration).TotalHours} h | {start.ToString("HH:mm")} - {stop.ToString("HH:mm")} | {timestamp.Description}");
          

                }


            }


            return result.ToString();


        }

        public string GetTotalProjectWorkTime(Toggl.Project project, string startDate, string endDate)
        {

            StringBuilder result = new StringBuilder();

            var prams = new TimeEntryParams();

            prams.StartDate = Convert.ToDateTime(startDate);
            prams.EndDate = Convert.ToDateTime(endDate);

            var hours = TimeEntryService.List(prams);

            var choosedtimestamp = hours.Where(w => w.ProjectId == project.Id).ToList();
            double totaltime = 0;


            result.AppendLine($"{project.Name}");
            

            foreach (var item in choosedtimestamp)
            {
                totaltime += TimeSpan.FromSeconds((double)item.Duration).TotalHours;
            }

            result.AppendLine($"   Total time: {totaltime} h");

            result.AppendLine($"       Task Description:");
            foreach (var item in choosedtimestamp)
            {
                result.AppendLine($"          {item.Description}");
            }

            return result.ToString();

        }

        public ProjectChart GetProjectChart(Toggl.Project project)
        {

           
            var prams = new TimeEntryParams();

            prams.StartDate = DateTime.Now.AddDays(-1);
            prams.EndDate = DateTime.Now;

            var hours = TimeEntryService.List(prams);

            var choosedtimestamp = hours.Where(w => w.ProjectId == project.Id).ToList();

            var projectChart = new ProjectChart(project.Name,(int)project.Id);
            projectChart.ClientID = (int)project.ClientId;


            foreach (var item in choosedtimestamp)
            {
                if (TimeSpan.FromSeconds(Convert.ToDouble(item.Duration)).TotalHours<0)
                {
                    continue;
                }
                projectChart.AddTask(item.Description, TimeSpan.FromSeconds(Convert.ToDouble(item.Duration)).TotalHours,Convert.ToDateTime(item.Stop));
            }




            return projectChart;

        }

        public ProjectChart GetProjectChart(Toggl.Project project,DateTime? startdate, DateTime? enddate)
        {


            var prams = new TimeEntryParams();

            prams.StartDate = startdate;
            prams.EndDate = enddate;

            var hours = TimeEntryService.List(prams);

            var choosedtimestamp = hours.Where(w => w.ProjectId == project.Id).ToList();

            var projectChart = new ProjectChart(project.Name,(int)project.Id);
            projectChart.ClientID = (int)project.ClientId;


            foreach (var item in choosedtimestamp)
            {
                if (TimeSpan.FromSeconds(Convert.ToDouble(item.Duration)).TotalHours < 0)
                {
                    continue;
                }
                projectChart.AddTask(item.Description, TimeSpan.FromSeconds(Convert.ToDouble(item.Duration)).TotalHours, Convert.ToDateTime(item.Stop));
            }




            return projectChart;

        }

        public List<ProjectChart> GetProjectChart(DateTime? startdate, DateTime? enddate)
        {


            var prams = new TimeEntryParams();

            prams.StartDate = startdate;
            prams.EndDate = enddate;

            var hours = TimeEntryService.List(prams);

            List<ProjectChart> projectCharts = new List<ProjectChart>();
            foreach (var project in Projects)
            {


                var choosedtimestamp = hours.Where(w => w.ProjectId == project.Id).ToList();

                var projectChart = new ProjectChart(project.Name,(int)project.Id);

                projectChart.ClientID = (int)project.ClientId;

                
                foreach (var item in choosedtimestamp)
                {
                    if (TimeSpan.FromSeconds(Convert.ToDouble(item.Duration)).TotalHours < 0)
                    {
                        continue;
                    }
                    projectChart.AddTask(item.Description, TimeSpan.FromSeconds(Convert.ToDouble(item.Duration)).TotalHours, Convert.ToDateTime(item.Stop));
                }


                if (projectChart.TimePerTasks.Count!=0)
                {
                    projectCharts.Add(projectChart);
                }

            }



            return projectCharts;

        }

        public List<ProjectChart> GetProjectChartFull()
        {


            var prams = new TimeEntryParams();

            prams.StartDate = Projects.Min(w => w.UpdatedOn);
            prams.EndDate = DateTime.Now.AddDays(1);

            var hours = TimeEntryService.List(prams);

            List<ProjectChart> projectCharts = new List<ProjectChart>();
            foreach (var project in Projects)
            {


                var choosedtimestamp = hours.Where(w => w.ProjectId == project.Id).ToList();

                var projectChart = new ProjectChart(project.Name,(int)project.Id);

                projectChart.ClientID = (int)project.ClientId;


                foreach (var item in choosedtimestamp)
                {
                    if (TimeSpan.FromSeconds(Convert.ToDouble(item.Duration)).TotalHours < 0)
                    {
                        continue;
                    }
                    projectChart.AddTask(item.Description, TimeSpan.FromSeconds(Convert.ToDouble(item.Duration)).TotalHours, Convert.ToDateTime(item.Stop));
                }


                if (projectChart.TimePerTasks.Count != 0)
                {
                    projectCharts.Add(projectChart);
                }

            }



            return projectCharts;

        }

        private string GetTaskNameByID(int Id, List<Toggl.Task> tasks)
        {
            var task = tasks.Where(w => w.Id == Id).ToList();
            return task[0].Name;

        }

        private int GetTaskIDByName(string taskName,Project project)
        {


            return (int)TaskService.ForProject((int)project.Id).First(w => w.Name == taskName).Id;
        }

        public void AddTask(int projectId,string taskName,bool isActive,int estimatedseconds)
        {
            
            TaskService.Add(new Toggl.Task
            {
                IsActive = isActive,
                Name = taskName,
                EstimatedSeconds = estimatedseconds,
                WorkspaceId = WorkSpaceID,
                ProjectId = projectId
            });



        }

        
        public void AddTimeEntries(int projectId, int taskId, int duration,string description)
        {

            TimeEntryService.Add(new TimeEntry()
            {
                IsBillable = true,
                CreatedWith = "TogglAPI.Net",
                Duration = duration,
                Start = DateTime.Now.ToIsoDateStr(),
                WorkspaceId = WorkSpaceID,
                ProjectId = projectId,
                TaskId = taskId,
                Description= description
            });

        }

        public void AddTimeEntries(Project project, string taskName, int duration, string description)
        {

            TimeEntryService.Add(new TimeEntry()
            {
                IsBillable = true,
                CreatedWith = "TogglAPI.Net",
                Duration = duration,
                Start = DateTime.Now.ToIsoDateStr(),
                WorkspaceId = WorkSpaceID,
                ProjectId = project.Id,
                TaskId = GetTaskIDByName(taskName,project),
                Description = description
            });

        }

        public void RemoveProjectTasks(int projectId)
        {
            var projecttasks = TaskService.ForProject(projectId);
            foreach (var item in projecttasks)
            {
                TaskService.Delete((int)item.Id);
            }

        }

       



    }
}
