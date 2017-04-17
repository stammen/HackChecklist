using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.AppService;
using Microsoft.Win32;
using Microsoft.VisualStudio.Setup.Configuration;
using System.Runtime.InteropServices;

namespace BackgroundFormProcess
{
    class AppService : ApplicationContext
    {
        static AppServiceConnection connection = null;
        private const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154);

        [DllImport("Microsoft.VisualStudio.Setup.Configuration.Native.dll", ExactSpelling = true, PreserveSig = true)]
        private static extern int GetSetupConfiguration(
            [MarshalAs(UnmanagedType.Interface), Out] out ISetupConfiguration configuration,
            IntPtr reserved);

        public AppService()
        {
            Thread appServiceThread = new Thread(new ThreadStart(ThreadProc));
            appServiceThread.Start();
        }

        /// <summary>
        /// Creates the app service connection
        /// </summary>
        static async void ThreadProc()
        {
            connection = new AppServiceConnection();
            connection.AppServiceName = "CommunicationService";
            connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;

            AppServiceConnectionStatus status = await connection.OpenAsync();
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
                    Debug.WriteLine(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}.", connection.AppServiceName));
                    return;
                case AppServiceConnectionStatus.Unknown:
                    Debug.WriteLine(string.Format("An unkown error occurred while we were trying to open an AppServiceConnection."));
                    return;
            }
        }

        /// <summary>
        /// Receives message from UWP app and sends a response back
        /// </summary>
        private static void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string key = args.Request.Message.First().Key;
            string value = args.Request.Message.First().Value.ToString();
            Debug.WriteLine(string.Format("Received message '{0}' with value '{1}'", key, value));

            if (key == "request")
            {
                ValueSet valueSet = new ValueSet();
                valueSet.Add("response", value.ToUpper());
                Debug.WriteLine(string.Format("Sending response: '{0}'", value.ToUpper()));
                args.Request.SendResponseAsync(valueSet).Completed += delegate { };
            }
            else if (key == "runChecks")
            {
                ValueSet valueSet = new ValueSet();
                valueSet.Add("DeveloperMode", CheckDeveloperMode());
                valueSet.Add("VS2017", CheckVS2017());

                args.Request.SendResponseAsync(valueSet).Completed += delegate { };
            }
            else if (key == "terminate")
            {
                ValueSet valueSet = new ValueSet();
                valueSet.Add("response", true);
                Debug.WriteLine(string.Format("Sending terminate response: '{0}'", "true"));
                args.Request.SendResponseAsync(valueSet).Completed += delegate {
                    Application.Exit();
                };
            }
        }

        private static bool CheckDeveloperMode()
        {
            try
            {
                RegistryKey localKey;
                if (Environment.Is64BitOperatingSystem)
                    localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                else
                    localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

                var value = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock").GetValue("AllowAllTrustedApps").ToString();
                if(value == "1")
                {
                    return true;
                }

                value = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock").GetValue("AllowDevelopmentWithoutDevLicense").ToString();
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


        private static bool CheckVS2017()
        {
            try
            {
                var query = GetVSQuery();
                var query2 = (ISetupConfiguration2)query;
                var e = query2.EnumAllInstances();
                int fetched;
                int count = 0;
                var instances = new ISetupInstance[1];
                do
                {
                    e.Next(1, instances, out fetched);
                    if (fetched > 0)
                    {
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

        private static ISetupConfiguration GetVSQuery()
        {
            try
            {
                // Try to CoCreate the class object.
                return new SetupConfiguration();
            }
            catch (COMException ex) when (ex.HResult == REGDB_E_CLASSNOTREG)
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

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppService());
        }
    }
}
