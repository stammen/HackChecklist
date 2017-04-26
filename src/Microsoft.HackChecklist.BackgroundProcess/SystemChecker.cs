using Microsoft.HackChecklist.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Microsoft.HackChecklist.SystemChecker
{
    public class SystemChecker
    {
        private const string UninstallRegistrySubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string UninstallRegistryKeyValue = "DisplayName";

        private readonly AppServiceConnection _connection;        

        public SystemChecker()
        {
            _connection = new AppServiceConnection();
            _connection.AppServiceName = "CommunicationService";
            _connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            _connection.RequestReceived += Connection_RequestReceived;
        }

        public void Run(string command, string parameter)
        {
            var appServiceThread = new Thread(new ThreadStart(ThreadProc));
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
            var value = args.Request.Message.First().Value;
            Debug.WriteLine($"Received message '{key}' with value '{value}'");

            if (key == "request")
            {
                var valueSet = new ValueSet();
                valueSet.Add("response", value.ToString().ToUpper());
                Debug.WriteLine($"Sending response: '{value.ToString().ToUpper()}'");
                args.Request.SendResponseAsync(valueSet).Completed += delegate { };
            }
            else if (key == "runChecks")
            {
                var valueSet = new ValueSet();
                var software = (Software)value;
                if (software != null) valueSet.Add(software.Name, CheckRequirement(software));
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

        private bool CheckRequirement(Software software)
        {
            var checkResult = false;
            switch (software.CheckType)
            {
                case CheckType.RegistryValueCheck:
                    checkResult = string.Compare(
                        RegistryChecker.GetLocalRegistryValue(software.InstallationRegistryKey, software.InstallationRegistryValue),
                        software.InstallationRegistryExpectedValue) == 0;
                    break;
                case CheckType.IncludedInRegistryInstallationCheck:
                    var installedSoftware = RegistryChecker.GetLocalRegistryValues(UninstallRegistrySubKey, UninstallRegistryKeyValue);
                    checkResult = installedSoftware?.Any(program =>
                        program.Contains(software.InstallationRegistryKey, StringComparison.InvariantCultureIgnoreCase)) ?? false;
                    break;
                case CheckType.VisualStudioInstalledCheck:
                    checkResult = new VisualStudioChecker().IsVisualStudio2017Installed();
                    break;
                case CheckType.VisualStudioWorkloadInstalledCheck:
                    var visualStudioChecker = new VisualStudioChecker();
                    checkResult = new VisualStudioChecker().IsWorkloadInstalled(software.InstallationRegistryKey);
                    break;
                case CheckType.MinimumRegistryValueCheck:
                    checkResult = string.Compare(
                        RegistryChecker.GetLocalRegistryValue(software.InstallationRegistryKey, software.InstallationRegistryValue),
                        software.InstallationRegistryExpectedValue) >= 0;
                    break;
            }

            return checkResult;
        }
    }
}
