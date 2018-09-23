using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Hangfire;

namespace Volvox.Helios.Service.Jobs
{
    public class HangfireJobService : IJobService
    {
        string IJobService.RunJob(Expression<Action> jobToRun)
        {
            return BackgroundJob.Enqueue(jobToRun);
        }

        string IJobService.RunJob(Expression<Func<Task>> jobToRun)
        {
            return BackgroundJob.Enqueue(jobToRun);
        }

        string IJobService.ScheduleJob(Expression<Action> jobToRun, TimeSpan delay)
        {
            return BackgroundJob.Schedule(jobToRun, delay);
        }

        string IJobService.ScheduleJob(Expression<Func<Task>> jobToRun, DateTimeOffset enqueueDate)
        {
            return BackgroundJob.Schedule(jobToRun, enqueueDate);
        }

        void IJobService.ScheduleRecurringJob(Expression<Action> jobToRun, string chronExpression, string jobIdentifyer)
        {
            RecurringJob.AddOrUpdate(jobIdentifyer, jobToRun, chronExpression);
        }

        void IJobService.ScheduleRecurringJob(Expression<Func<Task>> jobToRun, string chronExpression, string jobIdentifyer)
        {
            RecurringJob.AddOrUpdate(jobIdentifyer, jobToRun, chronExpression);
        }

        void IJobService.CancelRecurringJob(string jobIdentifyer)
        {
            RecurringJob.RemoveIfExists(jobIdentifyer);
        }
    }
}
