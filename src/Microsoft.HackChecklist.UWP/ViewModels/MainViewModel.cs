using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation.Collections;
using Microsoft.HackChecklist.UWP.Contracts;
using Microsoft.HackChecklist.UWP.ViewModels.Base;

namespace Microsoft.HackChecklist.UWP.ViewModels
{
    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private string _message;
        public ICommand SendRequest => new RelayCommand(DoSendRequest);


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

        public async void DoSendRequest()
        {
            await LaunchBackgroundProcess();

            if (App.Connection == null) return;

            Message = "running";
            var valueSet = new ValueSet {{"runChecks", true}};

            var response = await App.Connection.SendMessageAsync(valueSet);
            Message = (bool)response.Message["DeveloperMode"] ? "Developer Mode is Enabled" : "Developer Mode is Disabled";
            Message += "\n";
            Message += (bool)response.Message["VS2017"] ? "Visual Studio 2017 is installed" : "Visual Studio 2017 is not installed";

            // need to terminate the BackGround process!
            valueSet = new ValueSet {{"terminate", true}};
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
