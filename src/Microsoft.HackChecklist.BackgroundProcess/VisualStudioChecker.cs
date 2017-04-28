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

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Microsoft.HackChecklist.BackgroundProcess
{
    public class VisualStudioChecker
    {
        private const int RegdbEClassnotreg = unchecked((int)0x80040154);
        private const string WorkloadPackageType = "Workload";

        [DllImport("Microsoft.VisualStudio.Setup.Configuration.Native.dll", ExactSpelling = true, PreserveSig = true)]
        private static extern int GetSetupConfiguration(
            [MarshalAs(UnmanagedType.Interface), Out] out ISetupConfiguration configuration,
            IntPtr reserved);

        private ISetupConfiguration SetupConfiguration { get; set; }

        public bool IsVisualStudio2017Installed()
        {
            // Any instance of Visual Studio 2017 means it is installed.
            return IsPackageInstalled((packages) => true);
        }

        public bool IsWorkloadInstalled(string workloadId)
        {
            if (string.IsNullOrEmpty(workloadId)) return false;
            
            return IsPackageInstalled((packages) =>
            {
                if (packages == null || !packages.Any()) return false;

                var workloadPackage = packages.Where(p =>
                    string.Equals(p.GetType(), WorkloadPackageType, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(p.GetId(), workloadId, StringComparison.InvariantCultureIgnoreCase));
                return workloadPackage.Any();
            });
        }

        private bool IsPackageInstalled(Func<ISetupPackageReference[], bool> packageChecker)
        {
            try
            {
                var isInstalled = false;
                var query = GetVisualStudioConfiguration();
                var query2 = (ISetupConfiguration2)query;
                var e = query2.EnumAllInstances();

                int fetched;
                var instances = new ISetupInstance[1];
                do
                {
                    e.Next(1, instances, out fetched);
                    if (fetched <= 0) continue;
                    var instance = (ISetupInstance2)instances[0];

                    isInstalled = packageChecker?.Invoke(instance.GetPackages()) ?? false;
                }
                while (fetched > 0 && !isInstalled);

                return isInstalled;
            }
            catch
            {
                return false;
            }
        }        

        private ISetupConfiguration GetVisualStudioConfiguration()
        {
            if (SetupConfiguration != null)
            {
                return SetupConfiguration;
            }

            try
            {
                // Try to CoCreate the class object.
                SetupConfiguration = new SetupConfiguration();
                return SetupConfiguration;
            }
            catch (COMException ex) when (ex.HResult == RegdbEClassnotreg)
            {
                // Try to get the class object using app-local call.
                var result = GetSetupConfiguration(out ISetupConfiguration query, IntPtr.Zero);

                if (result < 0)
                {
                    throw new COMException("Failed to get query", result);
                }

                return query;
            }
        }
    }
}
