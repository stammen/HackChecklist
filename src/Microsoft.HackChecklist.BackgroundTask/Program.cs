﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Microsoft.HackChecklist.BackgroundTask
{
    class Program
    {
        static AppServiceConnection connection = null;
        static AutoResetEvent appServiceExit;

        static void Main(string[] args)
        {
            //we use an AutoResetEvent to keep to process alive until the communication channel established by the App Service is open
            appServiceExit = new AutoResetEvent(false);
            Thread appServiceThread = new Thread(new ThreadStart(ThreadProc));
            appServiceThread.Start();
            appServiceExit.WaitOne();
        }

        static async void ThreadProc()
        {
            //we create a connection with the App Service defined by the UWP app
            connection = new AppServiceConnection();
            connection.AppServiceName = "CommunicationService";
            connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            //we open the connection
            AppServiceConnectionStatus status = await connection.OpenAsync();

            if (status != AppServiceConnectionStatus.Success)
            {
                //if the connection fails, we terminate the Win32 process
                appServiceExit.Set();
            }
            else
            {
                //if the connection is successful, we communicate to the UWP app that the channel has been established
                ValueSet initialStatus = new ValueSet();
                initialStatus.Add("Status", "Ready");
                await connection.SendMessageAsync(initialStatus);
            }
        }

        private static async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            //we receive the data about the flight from the UWP app and we generate the boarding pass on the desktop
            string flightCode = args.Request.Message["Code"].ToString();
            string flightDate = args.Request.Message["Date"].ToString();
            string departureCity = args.Request.Message["Departure"].ToString();
            string arrivalCity = args.Request.Message["Arrival"].ToString();

            GenerateBoardingPass(flightCode, flightDate, departureCity, arrivalCity);

            //we send a message back to the UWP app to communicate that the operation has been completed with success
            ValueSet set = new ValueSet();
            set.Add("Status", "Success");

            await args.Request.SendResponseAsync(set);
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            //when the connection with the App Service is closed, we terminate the Win32 process
            appServiceExit.Set();
        }

        private static void GenerateBoardingPass(string flightCode, string flightDate, string departureCity, string arrivalCity)
        {
            //we generate the boarding pass on the desktop of the user
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string fileName = $"{userPath}\\BoardingPass.txt";
            var builder = new StringBuilder();
            builder.AppendLine("Boarding pass generated by FlightTracker");
            builder.AppendLine("-----------------------------------------");
            builder.AppendLine($"Flight code: {flightCode}");
            builder.AppendLine($"Flight date: {flightDate}");
            builder.AppendLine($"Departure city: {departureCity}");
            builder.AppendLine($"Arrival city: {arrivalCity}");
            builder.AppendLine("-----------------------------------------");
            builder.AppendLine("Thank you for using FlightTracker");
            File.WriteAllText(fileName, builder.ToString());
        }




    }
}