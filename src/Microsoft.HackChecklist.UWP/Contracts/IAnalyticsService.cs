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
