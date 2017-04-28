//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using Microsoft.HackChecklist.Models;
using Microsoft.HackChecklist.Models.Consts;
using Microsoft.HackChecklist.Services.Contracts;
using Microsoft.HackChecklist.UWP.Contracts;
using Microsoft.HackChecklist.UWP.Services;
using Microsoft.HackChecklist.UWP.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Collections;
using ResponseStatus = Microsoft.HackChecklist.Models.Enums.ResponseStatus;

namespace Microsoft.HackChecklist.UWP.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public const string ConfigurationFileName = "configuration";

        private readonly IJsonSerializerService _jsonSerializerService;
        private readonly IAppDataService _appDataService;
        private readonly IAnalyticsService _analyticsService;

        private readonly ResourceLoader _resourceLoader = new ResourceLoader();

        private string _message;
        private bool _isChecking;
        private string _messageChecking;
        private string _messageChecked;

        public MainViewModel(IJsonSerializerService jsonSerializerService, IAppDataService appDataService, IAnalyticsService analyticsService)
        {
            _jsonSerializerService = jsonSerializerService;
            _appDataService = appDataService;
            _analyticsService = analyticsService;
            Init();
        }

        public ICommand CheckRequirementsCommand => new RelayCommand(CheckRequirementsAction, CheckRequirementsCan);

        public ObservableCollection<RequirementViewModel> Requirements { get; } = new ObservableCollection<RequirementViewModel>();

        public string MessageChecking
        {
            get => _messageChecking;
            set
            {
                _messageChecking = value;
                OnPropertyChanged();
            }
        }

        public string MessageChecked
        {
            get => _messageChecked;
            set
            {
                _messageChecked = value;
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
            Configuration configuration = _jsonSerializerService.Deserialize<Configuration>(strConfiguration);
            CheckRequirementsAction();
            foreach (var requirement in configuration.Requirements)
            {
                Requirements.Add(new RequirementViewModel(requirement));                
            }

            _analyticsService.TrackScreen(AnalyticsConfiguration.MainViewScreenName);
        }

        private async void CheckRequirementsAction()
        {
            _analyticsService.TrackEvent(AnalyticsConfiguration.CheckCategory, AnalyticsConfiguration.CheckAllRequirementsAction, null, 0);
            IsChecking = true;
            MessageChecking = _resourceLoader.GetString("TitleChecking");

            await LaunchBackgroundProcess();

            if (App.Connection == null) return;

            Message = "running";

            foreach (var requirement in Requirements)
            {
                await CheckRequirementRecursive(requirement);
            }

            MessageChecking = _resourceLoader.GetString("TitleCheckCompleted");
            MessageChecked = Requirements.Any(requirement => !requirement.IsOptional && requirement.Status != ResponseStatus.Success)
                ? _resourceLoader.GetString("SubTitleCheckFile")
                : _resourceLoader.GetString("SubTitleCheckSucces");

            // TODO: need to terminate the BackGround process!
            ValueSet valueSet = new ValueSet { { BackgroundProcessCommand.Terminate, true } };
            await App.Connection.SendMessageAsync(valueSet);
            IsChecking = false;
        }

        private async Task CheckRequirementRecursive(RequirementViewModel requirement)
        {
            ValueSet valueSet = new ValueSet { { BackgroundProcessCommand.RunChecks, _jsonSerializerService.Serialize(requirement) } };
            requirement.Status = ResponseStatus.Processing;

            var response = await App.Connection.SendMessageAsync(valueSet);
            var passed = (response?.Message.Keys.Contains(requirement.Name) ?? false)
                ? (bool)response?.Message[requirement.Name]
                : false;

            requirement.Status = passed ? ResponseStatus.Success : ResponseStatus.Failed;

            _analyticsService.TrackEvent(
                AnalyticsConfiguration.CheckCategory,
                AnalyticsConfiguration.CheckRequirementAction,
                requirement.Name,
                passed ? 1 : 0);

            requirement.Modules?.ToList().ForEach(async x => await CheckRequirementRecursive(x));
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
