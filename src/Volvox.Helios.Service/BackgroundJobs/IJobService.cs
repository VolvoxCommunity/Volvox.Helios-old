using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Volvox.Helios.Service.BackgroundJobs
{
    public interface IJobService
    {
        string RunJob(Expression<Action> jobToRun);
        string RunJob(Expression<Func<Task>> jobToRun);
        string ScheduleJob(Expression<Action> jobToRun, TimeSpan delay);
        string ScheduleJob(Expression<Func<Task>> jobToRun, DateTimeOffset enqueueDate);
        void ScheduleRecurringJob(Expression<Action> jobToRun, string chronExpression, string jobIdentifyer);
        void ScheduleRecurringJob(Expression<Func<Task>> jobToRun, string chronExpression, string jobIdentifyer);
        void CancelRecurringJob(string jobIdentifyer);
    }
}
