namespace Microsoft.HackChecklist.UWP.Services
{
    public static class AnalyticsConfiguration
    {
        // TODO: Use this trackingId for release: UA-97593129-1
        public const string TrackingId = "UA-97550520-1";

        public const bool IsDebug = false;
        public const bool ReportUncaughtExceptions = true;
        public const bool AutoAppLifetimeMonitoring = true;
        
        public const string MainViewScreenName = "MainView";

        public const string CheckCategory = "Check";
        public const string CheckAllRequirementsAction = "CheckAllRequirementsAction";
        public const string CheckRequirementAction = "CheckRequirementAction";
    }
}
