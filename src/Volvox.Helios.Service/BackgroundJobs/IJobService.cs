using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Volvox.Helios.Service.BackgroundJobs
{
    /// <summary>
    ///     Background job service to schedule or unschedule one-off background jobs or schedule or unschedule recurring jobs.
    /// </summary>
    public interface IJobService
    {
        /// <summary>
        ///     Immediately queue a one-off background job to run.
        /// </summary>
        /// <param name="jobToRun">The method to be invoked when the background job starts.</param>
        /// <returns>The background job id created for this job.</returns>
        string RunJob(Expression<Action> jobToRun);

        /// <summary>
        ///     Immediately queue a one-off background job to run.
        /// </summary>
        /// <param name="jobToRun">The method to be invoked when the background job starts.</param>
        /// <returns>The background job id created for this job.</returns>
        string RunJob(Expression<Func<Task>> jobToRun);

        /// <summary>
        ///     Queue a one-off background job to run after the delay.
        /// </summary>
        /// <param name="jobToRun">The method to be invoked when the background job starts.</param>
        /// <param name="delay">The time to delay before running this job.</param>
        /// <returns>The background job id created for this job.</returns>
        string ScheduleJob(Expression<Action> jobToRun, TimeSpan delay);

        /// <summary>
        ///     Queue a one-off background job to run at a specific date and time.
        /// </summary>
        /// <param name="jobToRun">The method to be invoked when the background job starts.</param>
        /// <param name="enqueueDate">The date and time when this job should run.</param>
        /// <returns>The background job id created for this job.</returns>
        string ScheduleJob(Expression<Func<Task>> jobToRun, DateTimeOffset enqueueDate);

        /// <summary>
        ///     Cancel a scheduled job.
        /// </summary>
        /// <param name="jobIdentifier">Id of job to cancel.</param>
        void CancelJob(string jobIdentifier);

        /// <summary>
        ///     Schedule a recurring job to run.
        /// </summary>
        /// <param name="jobToRun">The method to be invoked when the background job starts.</param>
        /// <param name="chronExpression">The cron expression to specify the repeating interval for the recurrence. Use the <see cref="Hangfire.Cron"/> helper class to easily generate cron expressions or visit https://en.wikipedia.org/wiki/Cron#CRON_expression for more information.</param>
        /// <param name="jobIdentifyer">The job identifier to reference this job by. Use the same job identifier to cancel this job or update it at a later point.</param>
        void ScheduleRecurringJob(Expression<Action> jobToRun, string chronExpression, string jobIdentifyer);

        /// <summary>
        ///     Schedule a recurring job to run.
        /// </summary>
        /// <param name="jobToRun">The method to be invoked when the background job starts.</param>
        /// <param name="chronExpression">The cron expression to specify the repeating interval for the recurrence. Use the <see cref="Hangfire.Cron"/> helper class to easily generate cron expressions or visit https://en.wikipedia.org/wiki/Cron#CRON_expression for more information.</param>
        /// <param name="jobIdentifyer">The job identifier to reference this job by. Use the same job identifier to cancel this job or update it at a later point.</param>
        void ScheduleRecurringJob(Expression<Func<Task>> jobToRun, string chronExpression, string jobIdentifyer);

        /// <summary>
        ///     Cancel a recurring job if it exists.
        /// </summary>
        /// <param name="jobIdentifyer">The job identifier used to create the recurring job.</param>
        void CancelRecurringJob(string jobIdentifyer);
    }
}
