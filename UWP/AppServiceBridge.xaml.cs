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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using Windows.UI.Popups;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.AppService;
using System.Threading.Tasks;

namespace SDKTemplate
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppServiceBridge : Page
    {
        bool win32Launched = false;

        public AppServiceBridge()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Launches the Win32 background process via the new "fullTrustProcess" extenstion
        /// </summary>
        private async Task LaunchBackgroundProcess()
        {
            try
            {
                // Make sure the BackgroundProcess is in your AppX folder, if not rebuild the solution
                await Windows.ApplicationModel.FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                await Task.Delay(1000); // quick fix, need to make it better
                win32Launched = true;
            }
            catch (Exception)
            {
                win32Launched = false;
            }
        }


        private async void RunChecksClicked(object sender, RoutedEventArgs e)
        {
            await LaunchBackgroundProcess();
            RunChecksResponseText.Text = "running";

            ValueSet valueSet = new ValueSet();
            valueSet.Add("runChecks", "hello");

            if (App.Connection != null)
            {
                AppServiceResponse response = await App.Connection.SendMessageAsync(valueSet);
                RunChecksResponseText.Text = (bool)response.Message["response"] ? "Developer Mode is Enabled" : "Developer Mode is Disabled";
            }
        }
    }
}
