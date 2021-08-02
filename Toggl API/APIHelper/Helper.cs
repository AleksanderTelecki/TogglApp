using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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


        #region JSON and Colors

        /// <summary>
        /// Method for color initialization 
        /// </summary>
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


        /// <summary>
        /// Returns Color by id from projectColors
        /// </summary>
        /// <param name="id"></param>
        /// <returns>System.Drawing.Color</returns>
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





        /// <summary>
        /// Method that updates colors in color.json and projectColors
        /// </summary>
        /// <param name="updColors"></param>
        /// <param name="filePath"></param>
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


        /// <summary>
        /// Method that saves colors in color.json
        /// </summary>
        public void SaveColors()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "color.json");
            JSONSerialize(projectColors, filePath);
        }


        /// <summary>
        /// Method that saves/export colors from projectColors
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        public void SaveColors(string filePath)
        {
            JSONSerialize(projectColors, filePath);
        }


        /// <summary>
        /// JSON Serializetion method 
        /// </summary>
        /// <param name="data">Date to serialize</param>
        /// <param name="filePath">Path to file</param>
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

        /// <summary>
        /// JSON Deserialization method 
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="filePath">Path to file</param>
        /// <returns></returns>
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

        #endregion


        #region ProjectChart

        /// <summary>
        /// Method that returns ProjectChart by Project from Toggl API
        /// </summary>
        /// <param name="project">Project from Toggl API</param>
        /// <returns>ProjectChart</returns>
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



        /// <summary>
        /// Async Method that returns List of ProjectChart in specified range between startdate and enddate
        /// </summary>
        /// <param name="startdate">Start Date</param>
        /// <param name="enddate">End Date</param>
        /// <returns>List<ProjectChart></returns>
        public async Task<List<ProjectChart>> GetProjectChartAsync(DateTime? startdate, DateTime? enddate)
        {


            var prams = new TimeEntryParams();

            prams.StartDate = startdate;
            prams.EndDate = enddate;

            var hours = await System.Threading.Tasks.Task.FromResult(TimeEntryService.List(prams));

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


        /// <summary>
        /// Method that returns List of ProjectChart in specified range between startdate and enddate
        /// </summary>
        /// <param name="startdate">Start Date</param>
        /// <param name="enddate">End Date</param>
        /// <returns>List<ProjectChart></returns>
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

                var projectChart = new ProjectChart(project.Name, (int)project.Id);

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

        /// <summary>
        /// Method that returns Full List of ProjectChart of all time
        /// </summary>
        /// <returns>List<ProjectChart></returns>
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

        #endregion

        #region OtherMethods
        /// <summary>
        /// Method returns task name by Id
        /// </summary>
        /// <param name="Id">Task ID from Toggl API</param>
        /// <param name="tasks">Task list from Toggl API</param>
        /// <returns>String Name of Task by Id</returns>
        private string GetTaskNameByID(int Id, List<Toggl.Task> tasks)
        {
            var task = tasks.Where(w => w.Id == Id).ToList();
            return task[0].Name;

        }


        /// <summary>
        /// Method returns task Id by name
        /// </summary>
        /// <param name="taskName">Task Name from Toggl API</param>
        /// <param name="project">Project from Toggl API</param>
        /// <returns>Int id of Task by taskName</returns>
        private int GetTaskIDByName(string taskName,Project project)
        {
            return (int)TaskService.ForProject((int)project.Id).First(w => w.Name == taskName).Id;
        }

        /// <summary>
        /// Method for adding task to Toggl API
        /// </summary>
        /// <param name="projectId">Project Id from Toggl API</param>
        /// <param name="taskName">Task Name from Toggl API</param>
        /// <param name="isActive">Is task active</param>
        /// <param name="estimatedseconds">Estimated seconds</param>
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

        /// <summary>
        /// Method for adding timeentries to Toggl API
        /// </summary>
        /// <param name="projectId">Project Id from Toggle API</param>
        /// <param name="taskId">Task Id from Toggle API</param>
        /// <param name="duration">Duration</param>
        /// <param name="description">Description</param>
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

        /// <summary>
        /// Method for adding timeentries to Toggl API
        /// </summary>
        /// <param name="project">Project form Toggle API</param>
        /// <param name="taskName">Task name from Toggle API</param>
        /// <param name="duration">Duration</param>
        /// <param name="description">Description</param>
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

        /// <summary>
        /// Methods for remove project tasks
        /// </summary>
        /// <param name="projectId">Project Id from Toggl API</param>
        public void RemoveProjectTasks(int projectId)
        {
            var projecttasks = TaskService.ForProject(projectId);
            foreach (var item in projecttasks)
            {
                TaskService.Delete((int)item.Id);
            }

        }

        #endregion



    }
}
