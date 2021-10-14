// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Teams.Samples.HelloWorld.Web.Model;
using System;
using System.Timers;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Timer reminderTimer = new Timer(60000);
            reminderTimer.Enabled = true;
            reminderTimer.AutoReset = true;
            reminderTimer.Elapsed += ReminderTimer_Elapsed;

            CreateWebHostBuilder(args).Build().Run();
        }

        private static void ReminderTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
            foreach (var reminder in DependencyDataStore.RemindersListDataStore)
            {
                DateTime lastTrigger = reminder.LastTriggeredDateTime;
                DateTime newtriggerTime = lastTrigger.AddSeconds(reminder.Interval);
                if (newtriggerTime < DateTime.Now)
                {
                    Console.WriteLine("Sending reminder");
                    reminder.LastTriggeredDateTime = DateTime.Now;
                    // send a reminder on teams channel
                }
                Console.WriteLine(reminder.Message);
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
