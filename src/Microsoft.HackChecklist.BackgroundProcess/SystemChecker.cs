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

using System.Threading.Tasks;
using Microsoft.HackChecklist.BackgroundProcess.Extensions;
using Microsoft.HackChecklist.Models;
using Microsoft.HackChecklist.Models.Consts;
using Microsoft.HackChecklist.Models.Enums;
using Microsoft.HackChecklist.Services;
using System;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Microsoft.HackChecklist.BackgroundProcess
{
    public class SystemChecker
    {
        private const string UninstallRegistrySubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string UninstallRegistryKeyValue = "DisplayName";

        static AppServiceConnection _connection = null;
        static AutoResetEvent _appServiceExit = null;

        public SystemChecker()
        {

        }

        public void Run()
        {
            _appServiceExit = new AutoResetEvent(false);
            var appServiceThread = new Thread(ConnectionThread);
            appServiceThread.Start();
            _appServiceExit.WaitOne();
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            //when the connection with the App Service is closed, we terminate the Win32 process
            Console.WriteLine($"Connection_ServiceClosed");
            _appServiceExit.Set();
        }

        static async void ConnectionThread()
        {
            //we create a connection with the App Service defined by the UWP app
            _connection = new AppServiceConnection();
            _connection.AppServiceName = "CommunicationService";
            _connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            _connection.RequestReceived += Connection_RequestReceived;
            _connection.ServiceClosed += Connection_ServiceClosed;

            //we open the connection
            AppServiceConnectionStatus status = await _connection.OpenAsync();

            if (status != AppServiceConnectionStatus.Success)
            {
                //if the connection fails, we terminate the Win32 process
                _appServiceExit.Set();
            }
            else
            {
                Console.WriteLine("Connection established - waiting for requests");
                //if the connection is successful, we communicate to the UWP app that the channel has been established
                ValueSet initialStatus = new ValueSet();
                initialStatus.Add("Status", "Ready");
                await _connection.SendMessageAsync(initialStatus);
            }
        }

        private static async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            Console.WriteLine("RequestReceived");

            var key = args.Request.Message.First().Key;
            Console.WriteLine($"I have something {key} - {args.Request.Message.First().Value}");
            Requirement value;
            try
            {
                value = new JsonSerializerService().Deserialize<Requirement>(args.Request.Message.First().Value.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Console.WriteLine($"Received message '{key}' with value '{value}'");

            if (key == BackgroundProcessCommand.Request)
            {
                var valueSet = new ValueSet();
                valueSet.Add("response", value.ToString().ToUpper());
                Console.WriteLine($"Sending response: '{value.ToString().ToUpper()}'");
                args.Request.SendResponseAsync(valueSet).Completed += delegate { };
            }
            else if (key == BackgroundProcessCommand.RunChecks)
            {
                var valueSet = new ValueSet();
                var software = (Software)value;
                if (software != null)
                {
                    valueSet.Add(software.Name, CheckRequirement(software));
                }
                Console.WriteLine($"Responsing valueSet: {valueSet}");
                args.Request.SendResponseAsync(valueSet).Completed += delegate { };
                //Console.WriteLine($"Responsed with {software?.Status}");
            }
            else if (key == BackgroundProcessCommand.RunChecks)
            {
                var valueSet = new ValueSet();
                valueSet.Add("response", true);
                Console.WriteLine("Sending terminate response: \'true\'");
                args.Request.SendResponseAsync(valueSet).Completed += delegate {
                };
            }

            //we send a message back to the UWP app to communicate that the operation has been completed with success
            ValueSet set = new ValueSet();
            set.Add("Status", "Success");

            await args.Request.SendResponseAsync(set);
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
