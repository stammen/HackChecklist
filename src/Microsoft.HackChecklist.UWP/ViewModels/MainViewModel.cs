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
            CheckRegistry();
        }

        public void CheckRegistry()
        {
            Requirements = Configuration.Requirements.ToList();
        }

        private async void DoSendRequest()
        {
            await LaunchBackgroundProcess();

            if (App.Connection == null) return;

            Message = "running";
            var valueSet = new ValueSet { { "runChecks", true } };

            var response = await App.Connection.SendMessageAsync(valueSet);

            // TODO: WIP pending of integration of layout.
            Message = "\n";
            Message += (bool)response.Message["DeveloperMode"]
                ? "Developer Mode is Enabled"
                : "Developer Mode is Disabled";
            Message += "\n";
            Message += "Windows Version Build" + (string)response.Message["WindowsVersion"];
            Message += "\n";
            Message += (bool)response.Message["VS2017"]
                ? "Visual Studio 2017 is installed"
                : "Visual Studio 2017 is not installed";
            Message += "\n";
            Message += (bool)response.Message["SDK UWP"]
                ? "Microsoft Universal SDK is installed"
                : "Microsoft Universal SDK is not installed";
            Message += "\n";
            Message += (bool)response.Message[".NET Desktop Develpoment"]
                ? ".NET desktop development is installed"
                : ".NET desktop development is not installed";
            Message += "\n";
            Message += (bool)response.Message["Xamarin with Android SDK"]
                ? "Xamarin with Android SDK, Java, and Google Android Emulator is installed"
                : "Xamarin with Android SDK, Java, and Google Android Emulator is not installed";
            Message += "\n";
            Message += (bool)response.Message["Azure Cli"]
                ? "Azure Cli is installed"
                : "Azure Cli is not installed";

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
