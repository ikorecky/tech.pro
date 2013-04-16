﻿using System;
using System.Configuration;
using Quartz;
using Quartz.Impl;

namespace Worker {

    class WorkerOptions {

        /// <summary>
        /// Whether or not to run the job immediately at runtime
        /// </summary>
        public bool RunImmediately { get; set; }

        /// <summary>
        /// The email address to send a notification to
        /// </summary>
        public string Email { get; set; }

    }

    class Program {
        private static readonly ISchedulerFactory SchedulerFactory;
        private static readonly IScheduler Scheduler;
        private static IJobDetail _emailJobDetail;
        private static WorkerOptions _options;

        static Program() {

            // Create a regular old Quartz scheduler
            SchedulerFactory = new StdSchedulerFactory();
            Scheduler = SchedulerFactory.GetScheduler();

        }

        static void Main(string[] args) {

            // Read our options from config (provided locally 
            // or via cloud host)
            ReadOptionsFromConfig();      
            
            // Now let's start our scheduler; you could perform
            // any processing or bootstrapping code here before
            // you start it but it must be started to schedule
            // any jobs
            Scheduler.Start();

            // Let's generate our email job detail now
            CreateJob();

            // And finally, schedule the job
            ScheduleJob();
        }

        private static void CreateJob() {

            // The job builder uses a fluent interface to
            // make it easier to build and generate an
            // IJobDetail object
            _emailJobDetail = JobBuilder.Create<EmailJob>()
                .WithIdentity("SendToMyself")   // Here we can assign a friendly name to our job        
                .Build();                       // And now we build the job detail

        }

        private static void ScheduleJob() {

            // Let's create a trigger that fires immediately
            ITrigger trigger = TriggerBuilder.Create()

                // A description helps other people understand what you want
                .WithDescription("Every day at 3AM CST")

                // A daily time schedule gives you a
                // DailyTimeIntervalScheduleBuilder which provides
                // a fluent interface to build a schedule
                .WithDailyTimeIntervalSchedule(x => x

                    // Here we specify the interval
                    .WithIntervalInHours(24)

                    // And how often to repeat it
                    .OnEveryDay()

                    // Specify the time of day the trigger fires, in UTC (9am),
                    // since CST is UTC-0600
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(9, 0))

                    // Specify the timezone
                    //
                    // I like to use UTC dates in my applications to make sure
                    // I stay consistent, especially when you never know what
                    // server you're on!
                    .InTimeZone(TimeZoneInfo.Utc))

                // Finally, we take the schedule and build a trigger
                .Build();

            // Ask the scheduler to schedule our EmailJob
            Scheduler.ScheduleJob(_emailJobDetail, trigger);
        }

        private static void ReadOptionsFromConfig() {

            // Make sure we have options to change
            if (_options == null)
                _options = new WorkerOptions();

            // Try to read the RunImmediately value from app.config
            string configRunImmediately = ConfigurationManager.AppSettings["RunImmediately"];
            bool runImmediately;

            if (Boolean.TryParse(configRunImmediately, out runImmediately)) {
                _options.RunImmediately = runImmediately;
            }

            // Try to read the Email value from app.config
            string configEmail = ConfigurationManager.AppSettings["Email"];

            if (!String.IsNullOrEmpty(configEmail)) {
                _options.Email = configEmail;
            }
        }
    }

    /// <summary>
    /// Our email job, yet to be implemented
    /// </summary>
    public class EmailJob : IJob {
        public void Execute(IJobExecutionContext context) {

            // Let's start simple, write to the console
            Console.WriteLine("Hello World! " + DateTime.Now.ToString("h:mm:ss tt"));

        }
    }
}