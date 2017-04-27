using Microsoft.HackChecklist.Models;
using Windows.ApplicationModel;
using Microsoft.HackChecklist.Models.Consts;
using Microsoft.HackChecklist.Services.Contracts;
using Microsoft.HackChecklist.UWP.Contracts;
using Microsoft.HackChecklist.UWP.Services;
using Microsoft.HackChecklist.UWP.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation.Collections;

namespace Microsoft.HackChecklist.UWP.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public const string ConfigurationFileName = "configuration";

        private readonly IJsonSerializerService _jsonSerializerService;
        private readonly IAppDataService _appDataService;
        private readonly IAnalyticsService _analyticsService;

        private string _message;
        private bool _isChecking;
        private List<Requirement> _requirements = new List<Requirement>();

        public MainViewModel(IJsonSerializerService jsonSerializerService, IAppDataService appDataService, IAnalyticsService analyticsService)
        {
            _jsonSerializerService = jsonSerializerService;
            _appDataService = appDataService;
            _analyticsService = analyticsService;
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
            _analyticsService.TrackScreen(AnalyticsConfiguration.MainViewScreenName);
        }

        private async void CheckRequirementsAction()
        {
            _analyticsService.TrackEvent(
                AnalyticsConfiguration.CheckCategory,
                AnalyticsConfiguration.CheckAllRequirementsAction,
                null,
                0);
            IsChecking = true;

            await LaunchBackgroundProcess();

            if (App.Connection == null) return;

            Message = "running";

            ValueSet valueSet;
            foreach (var requirement in Requirements)
            {
                valueSet = new ValueSet { { BackgroundProcessCommand.RunChecks, _jsonSerializerService.Serialize(requirement) } };
                var response = await App.Connection.SendMessageAsync(valueSet);
                var passed = false;
                if (response?.Message.Keys.Contains(requirement.Name) ?? false)
                {
                    // TODO: WIP Update the list of requirements with the results of the check.                    
                }
                _analyticsService.TrackEvent(
                    AnalyticsConfiguration.CheckCategory, 
                    AnalyticsConfiguration.CheckRequirementAction, 
                    requirement.Name, 
                    passed ? 1 : 0);
            }

            // need to terminate the BackGround process!
            valueSet = new ValueSet { { BackgroundProcessCommand.Terminate, true } };
            await App.Connection.SendMessageAsync(valueSet);
            IsChecking = false;
        }

        private bool CheckRequirementsCan()
        {
            return !IsChecking;
        }

        private async Task LaunchBackgroundProcess()
        {
            try
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                await Task.Delay(1000); // quick fix, need to make it better
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }       
    }
}
