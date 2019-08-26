using AudioSchedulerOver.Enum;
using AudioSchedulerOver.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Service
{
    public class SchedulerService
    {
        private static SchedulerService _instance;
        private readonly Dictionary<Guid, Timer> _timers = new Dictionary<Guid, Timer>();

        private SchedulerService() { }

        public static SchedulerService Instance => _instance ?? (_instance = new SchedulerService());

        static DateTime GetNextWeekday(DayOfWeek day, int extraDay = 1)
        {
            DateTime result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + extraDay, 0, 0, 0);
            while (result.DayOfWeek != day)
                result = result.AddDays(1);
            return result;
        }

        public void ScheduleTask(double intervalInHour, Action task, Guid scheduleId, DayEnum dayEnum, TimeSpan? startAt = null)
        {
            var targetDate = GetNextWeekday((DayOfWeek)dayEnum);

            targetDate = targetDate.Add(startAt.Value);
            
            if(DateTime.Now.DayOfWeek == (DayOfWeek) dayEnum)
            {
                targetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0); // start of te day

                targetDate = targetDate.Add(startAt.Value); // add the rest

                while(targetDate < DateTime.Now) // calculate next playing
                {
                    targetDate = targetDate.AddHours(intervalInHour);
                }
            }

            var timeToGo = targetDate - DateTime.Now;

            var timer = new Timer(x =>
            {
                task.Invoke();
            }, null, timeToGo, TimeSpan.FromHours(intervalInHour));

            _timers.Add(scheduleId, timer);

            /*DateTime now = DateTime.Now;

            if(startAt.HasValue == false)
                startAt = TimeSpan.Zero;

            DateTime firstRun = new DateTime(startAt.Value.Year, startAt.Value.Month, startAt.Value.Day, startAt.Value.Hour, startAt.Value.Minute, startAt.Value.Second, startAt.Value.Millisecond);

            if (startAt > firstRun)
            {
                firstRun = firstRun.AddDays(1);
            }

            TimeSpan timeToGo = firstRun - now;
            if (timeToGo <= TimeSpan.Zero)
            {
                timeToGo = TimeSpan.Zero;
            }

            var timer = new Timer(x =>
            {
                task.Invoke();
            }, null, timeToGo, TimeSpan.FromHours(intervalInHour));

            _timers.Add(scheduleId, timer);*/
        }

        public void KillSchedule(Guid scheduleId)
        {
            if(_timers.ContainsKey(scheduleId))
            {
                var timer = _timers[scheduleId];

                timer.Change(Timeout.Infinite, Timeout.Infinite);

                _timers.Remove(scheduleId);
            }
        }
    }
}
