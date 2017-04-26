using Microsoft.HackChecklist.UWP.ViewModels.Base;
using Microsoft.HackChecklist.Models;
using Microsoft.HackChecklist.Services;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using Windows.Foundation.Collections;
using Microsoft.HackChecklist.UWP.Services;
using System.Linq;

namespace Microsoft.HackChecklist.UWP.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        public const string ConfigurationFileName = "configuration";
        private string _message;
        private bool _goChecking = false;
        private AppDataService _appDataService = new AppDataService();
        private List<Requirement> _requirements = new List<Requirement>();

        public MainViewModel()
        {
            Init();
        }

        public ICommand SendRequest => new RelayCommand(DoSendRequest);
        public ICommand CheckNowCommand => new RelayCommand(CheckRegistry);
        public Configuration Configuration { get; set; }

        public List<Requirement> Requirements
        {
            get
            {
                return _requirements;
            }
            set
            {
                _requirements = value;
                OnPropertyChanged();
            }
        }

        public bool GoChecking
        {
            get { return _goChecking; }
            set
            {
                _goChecking = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                if (value == _message) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public async void Init()
        {
            var strConfiguration = await _appDataService.GetDataFile(ConfigurationFileName);
            var serializer = new JsonSerializerService();
            var configuration = serializer.Deserialize<Configuration>(strConfiguration);
            Configuration = configuration;
            DoSendRequest();
        }

        public void CheckRegistry()
        {
            GoChecking = true;
            Requirements = Configuration.Requirements.ToList();
        }

        private async void DoSendRequest()
        {
            await LaunchBackgroundProcess();

            if (App.Connection == null) return;

            Message = "running";

            ValueSet valueSet;
            foreach (var requirement in Requirements)
            {
                valueSet = new ValueSet { { "runChecks", requirement } };
                var response = await App.Connection.SendMessageAsync(valueSet);
                if (response?.Message.Keys.Contains(requirement.Name) ?? false)
                {
                    // TODO: WIP Update the list of requirements with the results of the check.                    
                }
            }

            // need to terminate the BackGround process!
            valueSet = new ValueSet { { "terminate", true } };
            await App.Connection.SendMessageAsync(valueSet);
        }

        private static async Task LaunchBackgroundProcess()
        {
            try
            {
                // Make sure the BackgroundProcess is in your AppX folder, if not rebuild the solution
                await Windows.ApplicationModel.FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                await Task.Delay(1000); // quick fix, need to make it better
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
