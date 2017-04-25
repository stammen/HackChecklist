using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Microsoft.HackChecklist.BackgroundProcess
{
    public class SystemChecker
    {
        private readonly AppServiceConnection _connection;        

        public SystemChecker()
        {
            _connection = new AppServiceConnection();
            _connection.AppServiceName = "CommunicationService";
            _connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            _connection.RequestReceived += Connection_RequestReceived;
        }

        public void Run()
        {
            var appServiceThread = new Thread(ThreadProc);
            appServiceThread.Start();
        }

        private async void ThreadProc()
        {
            var status = await _connection.OpenAsync();
            switch (status)
            {
                case AppServiceConnectionStatus.Success:
                    Debug.WriteLine("Connection established - waiting for requests");
                    break;
                case AppServiceConnectionStatus.AppNotInstalled:
                    Debug.WriteLine("The app AppServicesProvider is not installed.");
                    return;
                case AppServiceConnectionStatus.AppUnavailable:
                    Debug.WriteLine("The app AppServicesProvider is not available.");
                    return;
                case AppServiceConnectionStatus.AppServiceUnavailable:
                    Debug.WriteLine($"The app AppServicesProvider is installed but it does not provide the app service {_connection.AppServiceName}.");
                    return;
                case AppServiceConnectionStatus.Unknown:
                    Debug.WriteLine(string.Format("An unkown error occurred while we were trying to open an AppServiceConnection."));
                    return;
            }
        }

        private void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var key = args.Request.Message.First().Key;
            var value = args.Request.Message.First().Value.ToString();
            Debug.WriteLine($"Received message '{key}' with value '{value}'");

            if (key == "request")
            {
                var valueSet = new ValueSet();
                valueSet.Add("response", value.ToUpper());
                Debug.WriteLine($"Sending response: '{value.ToUpper()}'");
                args.Request.SendResponseAsync(valueSet).Completed += delegate { };
            }
            else if (key == "runChecks")
            {
                // TODO: WIP Hardcoded string will be received as parameters.
                var visualStudioChecker = new VisualStudioChecker();                
                var installedPrograms = RegistryChecker.GetLocalRegistryValues(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", "DisplayName");

                var valueSet = new ValueSet();
                valueSet.Add("DeveloperMode", RegistryChecker.GetLocalRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock", "AllowAllTrustedApps") == "1");
                valueSet.Add("WindowsVersion", RegistryChecker.GetLocalRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber"));
                valueSet.Add("VS2017", visualStudioChecker.IsVisualStudio2017Installed());
                valueSet.Add("SDK UWP", visualStudioChecker.IsWorkloadInstalled("Microsoft.VisualStudio.Workload.Universal"));
                valueSet.Add(".NET Desktop Develpoment", visualStudioChecker.IsWorkloadInstalled("Microsoft.VisualStudio.Workload.ManagedDesktop"));
                valueSet.Add("Xamarin with Android SDK", visualStudioChecker.IsWorkloadInstalled("Microsoft.VisualStudio.Workload.NetCrossPlat"));
                valueSet.Add("Azure Cli", installedPrograms?.Any(installedProgram => installedProgram.Contains("Azure Cli", StringComparison.InvariantCultureIgnoreCase)) ?? false);

                args.Request.SendResponseAsync(valueSet).Completed += delegate { };
            }
            else if (key == "terminate")
            {
                var valueSet = new ValueSet();
                valueSet.Add("response", true);
                Debug.WriteLine($"Sending terminate response: '{"true"}'");
                args.Request.SendResponseAsync(valueSet).Completed += delegate {
                };
            }
        }
    }
}
