using Microsoft.HackChecklist.BackgroundProcess.Extensions;
using Microsoft.HackChecklist.Models;
using Microsoft.HackChecklist.Models.Consts;
using Microsoft.HackChecklist.Models.Enums;
using Microsoft.HackChecklist.Services;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;


namespace Microsoft.HackChecklist.BackgroundProcess
{
    class Program
    {
        static AppServiceConnection connection = null;
        static AutoResetEvent appServiceExit;

        private const string UninstallRegistrySubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string UninstallRegistryKeyValue = "DisplayName";

        static void Main(string[] args)
        {
            //we use an AutoResetEvent to keep to process alive until the communication channel established by the App Service is open
            appServiceExit = new AutoResetEvent(false);
            Thread appServiceThread = new Thread(new ThreadStart(ThreadProc));
            appServiceThread.Start();
            appServiceExit.WaitOne();
        }

        static async void ThreadProc()
        {
            //we create a connection with the App Service defined by the UWP app
            connection = new AppServiceConnection();
            connection.AppServiceName = "CommunicationService";
            connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            //we open the connection
            AppServiceConnectionStatus status = await connection.OpenAsync();

            if (status != AppServiceConnectionStatus.Success)
            {
                //if the connection fails, we terminate the Win32 process
                appServiceExit.Set();
            }
            else
            {
                //if the connection is succesfull, we communicate to the UWP app that the channel has been established
                ValueSet initialStatus = new ValueSet();
                initialStatus.Add("Status", "Ready");
                await connection.SendMessageAsync(initialStatus);
            }
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            //when the connection with the App Service is closed, we terminate the Win32 process
            appServiceExit.Set();
        }

        private static async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var key = args.Request.Message.First().Key;
            var value = new JsonSerializerService().Deserialize<Requirement>(args.Request.Message.First().Value.ToString());
            var valueSet = new ValueSet();
            var software = (Software)value;
            if (software != null)
            {
                valueSet.Add(software.Name, CheckRequirement(software));
            }
            Debug.WriteLine($"Responsing valueSet: {valueSet}");
            await args.Request.SendResponseAsync(valueSet);
        }

        private static bool CheckRequirement(Software software)
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
