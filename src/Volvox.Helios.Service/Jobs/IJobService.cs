using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Volvox.Helios.Service.Jobs
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
