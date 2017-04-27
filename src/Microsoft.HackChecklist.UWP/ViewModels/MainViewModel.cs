using Microsoft.HackChecklist.UWP.ViewModels.Base;
using Microsoft.HackChecklist.Models;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using Windows.Foundation.Collections;
using System.Linq;
using Microsoft.HackChecklist.Models.Consts;
using Microsoft.HackChecklist.Services.Contracts;
using Microsoft.HackChecklist.UWP.Contracts;

namespace Microsoft.HackChecklist.UWP.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public const string ConfigurationFileName = "configuration";

        private readonly IJsonSerializerService _jsonSerializerService;
        private readonly IAppDataService _appDataService;

        private string _message;
        private bool _isChecking;
        private List<Requirement> _requirements = new List<Requirement>();

        public MainViewModel(IJsonSerializerService jsonSerializerService, IAppDataService appDataService)
        {
            _jsonSerializerService = jsonSerializerService;
            _appDataService = appDataService;
            Init();
        }

        public ICommand CheckRequirementsCommand => new RelayCommand(CheckRequirementsAction, CheckRequirementsCan);

        public Configuration Configuration { get; set; }

        public List<Requirement> Requirements
        {
            get => _requirements;
            set
            {
                _requirements = value;
                OnPropertyChanged();
            }
        }

        public bool IsChecking
        {
            get => _isChecking;
            set
            {
                _isChecking = value;
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
            var configuration = _jsonSerializerService.Deserialize<Configuration>(strConfiguration);
            Configuration = configuration;
            Requirements = Configuration.Requirements.ToList();
            CheckRequirementsAction();
        }

        private async void CheckRequirementsAction()
        {
            IsChecking = true;

            await LaunchBackgroundProcess();

            if (App.Connection == null) return;

            Message = "running";

            ValueSet valueSet;
            foreach (var requirement in Requirements)
            {
                valueSet = new ValueSet {{ BackgroundProcessCommand.RunChecks, _jsonSerializerService.Serialize(requirement) }};
                var response = await App.Connection.SendMessageAsync(valueSet);
                if (response?.Message.Keys.Contains(requirement.Name) ?? false)
                {
                    // TODO: WIP Update the list of requirements with the results of the check.                    
                }
            }

            // need to terminate the BackGround process!
            valueSet = new ValueSet { { BackgroundProcessCommand.Terminate, true } };
            await App.Connection.SendMessageAsync(valueSet);
        }

        private bool CheckRequirementsCan()
        {
            return false;
        }

        private async Task LaunchBackgroundProcess()
        {
            try
            {
                await Windows.ApplicationModel.FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                await Task.Delay(1000); // quick fix, need to make it better
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }
    }
}
