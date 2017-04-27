using GoogleAnalytics;

namespace Microsoft.HackChecklist.UWP.Contracts
{
    public interface IAnalyticsService
    {
        Tracker Tracker { get; }

        void TrackEvent(string category, string action, string label, long value);

        void TrackScreen(string screenName);
    }
}
