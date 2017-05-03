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

        private readonly AppServiceConnection _connection;        

        public SystemChecker()
        {
            _connection = new AppServiceConnection();
            _connection.AppServiceName = "CommunicationService";
            _connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            _connection.RequestReceived += RequestReceived;
        }

        public void Run()
        {
            var appServiceThread = new Thread(ConnectionThread);
            appServiceThread.Start();
        }

        private async void ConnectionThread()
        {
            try
            {
                //Console.WriteLine("I am the thread");
                var status = await _connection.OpenAsync();
                //Console.WriteLine($"I am listening with status: {status}");
                switch (status)
                {
                    case AppServiceConnectionStatus.Success:
                        //Console.WriteLine("Connection established - waiting for requests");
                        ValueSet initialStatus = new ValueSet();
                        initialStatus.Add("Status", "Ready");
                        await _connection.SendMessageAsync(initialStatus);
                        break;
                    case AppServiceConnectionStatus.AppNotInstalled:
                        Console.WriteLine("The app AppServicesProvider is not installed.");
                        return;
                    case AppServiceConnectionStatus.AppUnavailable:
                        Console.WriteLine("The app AppServicesProvider is not available.");
                        return;
                    case AppServiceConnectionStatus.AppServiceUnavailable:
                        Console.WriteLine($"The app AppServicesProvider is installed but it does not provide the app service {_connection.AppServiceName}.");
                        return;
                    case AppServiceConnectionStatus.Unknown:
                        Console.WriteLine("An unkown error occurred while we were trying to open an AppServiceConnection.");
                        return;
                    case AppServiceConnectionStatus.RemoteSystemUnavailable:
                        break;
                    case AppServiceConnectionStatus.RemoteSystemNotSupportedByApp:
                        break;
                    case AppServiceConnectionStatus.NotAuthorized:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
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
