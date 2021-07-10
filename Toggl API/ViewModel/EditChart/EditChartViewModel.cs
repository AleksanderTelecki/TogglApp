using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Toggl_API.APIHelper;
using Toggl_API.APIHelper.ClassModel;

namespace Toggl_API.ViewModel.EditChart
{
    class EditChartViewModel : INotifyPropertyChanged
    {

        private CheckBoxModel selectedProject;

        private ClientsCheckBox selectedClient;

        public ObservableCollection<CheckBoxModel> Projects { get; set; }

        public ObservableCollection<ClientsCheckBox> Clients { get; set; }

        public CheckBoxModel SelectedProject
        {
            get { return selectedProject; }
            set
            {
                selectedProject = value;
                OnPropertyChanged("SelectedProject");
            }
        }

        public ClientsCheckBox SelectedClient
        {
            get { return selectedClient; }
            set
            {
                selectedClient = value;
                OnPropertyChanged("SelectedClient");
            }
        }

        public EditChartViewModel(ObservableCollection<ProjectChart> projectCharts,Helper helper)
        {
            ObservableCollection<CheckBoxModel> projects = new ObservableCollection<CheckBoxModel>();
            ObservableCollection<ClientsCheckBox> _clients = new ObservableCollection<ClientsCheckBox>();
            foreach (var item in projectCharts)
            {
                projects.Add(new CheckBoxModel(item));
            }

            var clientsId = projectCharts.Select(w => w.ClientID).Distinct().ToList();
            
            foreach (var item in clientsId)
            {
                _clients.Add(new ClientsCheckBox(helper.ClientService.Get(item)));
            }

            Projects = projects;
            Clients = _clients;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
