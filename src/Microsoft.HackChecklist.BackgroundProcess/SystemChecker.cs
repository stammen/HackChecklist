using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Microsoft.VisualStudio.Setup.Configuration;
using Microsoft.Win32;

namespace Microsoft.HackChecklist.BackgroundProcess
{
    public class SystemChecker
    {
        private readonly AppServiceConnection _connection;
        private const int RegdbEClassnotreg = unchecked((int)0x80040154);

        [DllImport("Microsoft.VisualStudio.Setup.Configuration.Native.dll", ExactSpelling = true, PreserveSig = true)]
        private static extern int GetSetupConfiguration([MarshalAs(UnmanagedType.Interface), Out] out ISetupConfiguration configuration, 
            IntPtr reserved);

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

            CheckVs2017();
        }

        private async void ThreadProc()
        {
            var status = await _connection.OpenAsync();
            switch (status)
            {
                case AppServiceConnectionStatus.Success:

                    break;
                case AppServiceConnectionStatus.AppNotInstalled:
                case AppServiceConnectionStatus.AppUnavailable:
                case AppServiceConnectionStatus.AppServiceUnavailable:
                case AppServiceConnectionStatus.Unknown:
                case AppServiceConnectionStatus.RemoteSystemUnavailable:
                case AppServiceConnectionStatus.RemoteSystemNotSupportedByApp:
                case AppServiceConnectionStatus.NotAuthorized:

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
                var valueSet = new ValueSet();
                valueSet.Add("DeveloperMode", CheckDeveloperMode());
                valueSet.Add("VS2017", CheckVs2017());

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

        private bool CheckDeveloperMode()
        {
            try
            {
                RegistryKey localKey;
                if (Environment.Is64BitOperatingSystem)
                    localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                else
                    localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

                var value = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock").GetValue("AllowAllTrustedApps").ToString();
                if (value == "1")
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }


        private bool CheckVs2017()
        {
            try
            {
                var query = GetVsQuery();
                var query2 = (ISetupConfiguration2)query;
                var e = query2.EnumAllInstances();
                int fetched;
                var count = 0;
                var instances = new ISetupInstance[1];
                do
                {
                    e.Next(1, instances, out fetched);
                    if (fetched > 0)
                    {
                        var instance = instances[0];
                        var instance2 = (ISetupInstance2)instances[0];
                        var state = instance2.GetState();

                        var installationVersion = instance.GetInstallationVersion();
                        PrintWorkloads(instance2.GetPackages());

                        count++;
                    }
                }
                while (fetched > 0);

                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        private void PrintWorkloads(ISetupPackageReference[] packages)
        {
            foreach (var package in packages)
            {
                var id = package.GetId();
            }

            var uwpPackage = packages.Where(p => p.GetId().ToLower().Contains("microsoft.visualstudio.component.windows10sdk.14393"));
            if (uwpPackage.Any())
            {
                var temp = uwpPackage.First().GetType();
            }

            var workloads = from package in packages
                where string.Equals(package.GetType(), "Workload", StringComparison.OrdinalIgnoreCase)
                orderby package.GetId()
                select package;

            foreach (var workload in workloads)
            {
                var id = workload.GetId();
            }
        }

        private ISetupConfiguration GetVsQuery()
        {
            try
            {
                // Try to CoCreate the class object.
                return new SetupConfiguration();
            }
            catch (COMException ex) when (ex.HResult == RegdbEClassnotreg)
            {
                // Try to get the class object using app-local call.
                ISetupConfiguration query;
                var result = GetSetupConfiguration(out query, IntPtr.Zero);

                if (result < 0)
                {
                    throw new COMException("Failed to get query", result);
                }

                return query;
            }
        }
    }
}
