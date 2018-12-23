using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Volvox.Helios.Service.BackgroundJobs
{
    /// <inheritdoc />
    public class JobService : IJobService
    {
        /// <inheritdoc />
        string IJobService.RunJob(Expression<Action> jobToRun)
        {
            return BackgroundJob.Enqueue(jobToRun);
        }

        /// <inheritdoc />
        string IJobService.RunJob(Expression<Func<Task>> jobToRun)
        {
            return BackgroundJob.Enqueue(jobToRun);
        }

        /// <inheritdoc />
        string IJobService.ScheduleJob(Expression<Action> jobToRun, TimeSpan delay)
        {
            return BackgroundJob.Schedule(jobToRun, delay);
        }

        /// <inheritdoc />
        string IJobService.ScheduleJob(Expression<Func<Task>> jobToRun, DateTimeOffset enqueueDate)
        {
            return BackgroundJob.Schedule(jobToRun, enqueueDate);
        }

        /// <inheritdoc />
        void IJobService.CancelJob(string jobIdentifier)
        {
            BackgroundJob.Delete(jobIdentifier);
        }

        /// <inheritdoc />
        void IJobService.ScheduleRecurringJob(Expression<Action> jobToRun, string chronExpression, string jobIdentifyer)
        {
            RecurringJob.AddOrUpdate(jobIdentifyer, jobToRun, chronExpression);
        }

        /// <inheritdoc />
        void IJobService.ScheduleRecurringJob(Expression<Func<Task>> jobToRun, string chronExpression, string jobIdentifyer)
        {
            RecurringJob.AddOrUpdate(jobIdentifyer, jobToRun, chronExpression);
        }

        /// <inheritdoc />
        void IJobService.CancelRecurringJob(string jobIdentifyer)
        {
            RecurringJob.RemoveIfExists(jobIdentifyer);
        }
    }
}
