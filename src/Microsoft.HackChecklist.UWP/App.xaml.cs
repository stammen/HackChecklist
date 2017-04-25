using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;

namespace Microsoft.HackChecklist.UWP
{
    sealed partial class App : Application
    {
        public static AppServiceConnection Connection;
        BackgroundTaskDeferral _appServiceDeferral;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            if (!(args.TaskInstance.TriggerDetails is AppServiceTriggerDetails)) return;

            _appServiceDeferral = args.TaskInstance.GetDeferral();
            args.TaskInstance.Canceled += OnTaskCanceled; // Associate a cancellation handler with the background task.

            var details = args.TaskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (details != null)
            {
                Connection = details.AppServiceConnection;
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _appServiceDeferral?.Complete();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame {Language = Windows.Globalization.ApplicationLanguages.Languages[0]};
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(View.MainView), e.Arguments);
            }
            Window.Current.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}