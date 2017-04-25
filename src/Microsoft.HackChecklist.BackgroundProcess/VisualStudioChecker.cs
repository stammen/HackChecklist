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

        private ISetupConfiguration _setupConfiguration { get; set; }

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
                return workloadPackage.Count() > 0;
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
                    if (fetched > 0)
                    {
                        var instance = instances[0];
                        var instance2 = (ISetupInstance2)instances[0];

                        isInstalled = packageChecker?.Invoke(instance2.GetPackages()) ?? false;
                    }
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
            if (_setupConfiguration != null) return _setupConfiguration;

            try
            {
                // Try to CoCreate the class object.
                _setupConfiguration = new SetupConfiguration();
                return _setupConfiguration;
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
