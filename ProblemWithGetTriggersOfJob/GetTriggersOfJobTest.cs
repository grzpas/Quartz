
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace ProblemWithGetTriggersOfJob
{

    [TestFixture]
    public class TriggersTest
    {      
        private List<Tuple<IJobDetail, ITrigger>> GetScheduledJobsWithTriggersInternal(IScheduler scheduler)
        {
            var jobGroups = scheduler.GetJobGroupNames();

            var result = new List<Tuple<IJobDetail, ITrigger>>();

            foreach (string group in jobGroups)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = scheduler.GetJobKeys(groupMatcher);
                foreach (var jobKey in jobKeys)
                {
                    var trigger = scheduler.GetTriggersOfJob(jobKey).FirstOrDefault();
                    var jobDetail = scheduler.GetJobDetail(jobKey);
                    if (jobDetail != null && trigger != null)
                    {
                        result.Add(Tuple.Create(jobDetail, trigger));
                    }
                }
            }
            return result;
        }

        [Test]
        public void GetTriggersOfJobTest()
        {
            var scheduler = CreateScheduler();
            scheduler.Clear();
            for (int i = 0; i < 5; ++i)
            {
                var trigger =  TriggerBuilder.Create().StartNow().WithSimpleSchedule(x => x.WithMisfireHandlingInstructionIgnoreMisfires()).Build();
                var jobDetails = JobBuilder.Create<MySimpleJob>().StoreDurably(false).WithIdentity(Guid.NewGuid().ToString(), "SimpleJobs").Build();
                scheduler.ScheduleJob(jobDetails, trigger);
            }
            scheduler.Start();

            while (GetScheduledJobsWithTriggersInternal(scheduler).Count != 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));    
            }
        }


        private static NameValueCollection ConfigureQuartzProperties()
        {
            var properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = "Scheduler";
            properties["quartz.scheduler.instanceId"] = "Scheduler1";
            properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
            properties["quartz.threadPool.threadCount"] = "1";
            properties["quartz.threadPool.threadPriority"] = "Normal";
            properties["quartz.jobStore.misfireThreshold"] = "1000";
            properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
            properties["quartz.jobStore.useProperties"] = "false";
            properties["quartz.jobStore.dataSource"] = "default";
            properties["quartz.jobStore.clustered"] = "true";
            properties["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.SQLiteDelegate, Quartz";
            properties["quartz.plugin.jobHistory.type"] = "Quartz.Plugin.History.LoggingJobHistoryPlugin";
            properties["quartz.dataSource.default.connectionString"] = @"Data Source=..\..\Db\quartz.db;Version=3;";
            properties["quartz.dataSource.default.provider"] = "SQLite-10";
            properties["quartz.dataSource.default.maxConnections"] = "3";
            return properties;
        }

        public static IScheduler CreateScheduler()
        {
            var schedulerFactory = new StdSchedulerFactory(ConfigureQuartzProperties());
            var scheduler = schedulerFactory.GetScheduler();
            return scheduler;
        }

    }
}
