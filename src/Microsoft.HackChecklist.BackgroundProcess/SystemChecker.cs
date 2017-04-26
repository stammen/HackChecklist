using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Microsoft.HackChecklist.BackgroundProcess.Extensions;
using Microsoft.HackChecklist.Models;
using Microsoft.HackChecklist.Models.Consts;
using Microsoft.HackChecklist.Models.Enums;
using Microsoft.HackChecklist.Services;

namespace Microsoft.HackChecklist.BackgroundProcess
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
            _connection.RequestReceived += ConnectionRequestReceived;
        }

        public void Run()
        {
            var appServiceThread = new Thread(ConnectionThread);
            appServiceThread.Start();
        }

        private async void ConnectionThread()
        {
            Console.WriteLine("I am listening");
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
                    Debug.WriteLine("An unkown error occurred while we were trying to open an AppServiceConnection.");
                    return;
            }
        }

        private void ConnectionRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var key = args.Request.Message.First().Key;
            var value = new JsonSerializerService().Deserialize<Requirement>(args.Request.Message.First().Value.ToString());
            Console.WriteLine($"Received message '{key}' with value '{value}'");

            if (key == BackgroundProcessCommand.Request)
            {
                var valueSet = new ValueSet();
                valueSet.Add("response", value.ToString().ToUpper());
                Debug.WriteLine($"Sending response: '{value.ToString().ToUpper()}'");
                args.Request.SendResponseAsync(valueSet).Completed += delegate { };
            }
            else if (key == BackgroundProcessCommand.RunChecks)
            {
                var valueSet = new ValueSet();
                var software = (Software)value;
                if (software != null) valueSet.Add(software.Name, CheckRequirement(software));
                args.Request.SendResponseAsync(valueSet).Completed += delegate { };
            }
            else if (key == BackgroundProcessCommand.RunChecks)
            {
                var valueSet = new ValueSet();
                valueSet.Add("response", true);
                Debug.WriteLine("Sending terminate response: \'true\'");
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
                    checkResult = string.CompareOrdinal(
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
                    checkResult = new VisualStudioChecker().IsWorkloadInstalled(software.InstallationRegistryKey);
                    break;
                case CheckType.MinimumRegistryValueCheck:
                    checkResult = string.CompareOrdinal(
                        RegistryChecker.GetLocalRegistryValue(software.InstallationRegistryKey, software.InstallationRegistryValue),
                        software.InstallationRegistryExpectedValue) >= 0;
                    break;
            }

            return checkResult;
        }
    }
}
