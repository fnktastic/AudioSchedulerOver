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

        private void ScheduleTaskOnce(double intervalInHour, Action task, Guid scheduleId, DayEnum dayEnum, TimeSpan? startAt = null)
        {
            Timer timer;
            TimeSpan timeToGo;

            var targetDate = GetNextWeekday((DayOfWeek)dayEnum);

            targetDate = targetDate.Add(startAt.Value);

            if (DateTime.Now.DayOfWeek == (DayOfWeek)dayEnum)
            {
                targetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0); // start of te day

                targetDate = targetDate.Add(startAt.Value); // add the rest

                if (DateTime.Now > targetDate)
                    targetDate = targetDate.AddDays(7);
            }

            timeToGo = targetDate - DateTime.Now;

            timer = new Timer(x =>
            {
                task.Invoke();
            }, null, timeToGo.Ticks / 10_000, Timeout.Infinite);

            _timers.Add(scheduleId, timer);
        }

        private void ScheduleRepeatedTask(double intervalInHour, Action task, Guid scheduleId, DayEnum dayEnum, TimeSpan? startAt = null)
        {
            var targetDate = GetNextWeekday((DayOfWeek)dayEnum);

            targetDate = targetDate.Add(startAt.Value);

            if (DateTime.Now.DayOfWeek == (DayOfWeek)dayEnum)
            {
                targetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0); // start of te day

                targetDate = targetDate.Add(startAt.Value); // add the rest

                while (targetDate < DateTime.Now) // calculate next playing
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
        }

        public void ScheduleTask(double intervalInHour, Action task, Guid scheduleId, DayEnum dayEnum, TimeSpan? startAt = null)
        {
            if (intervalInHour == 0)
                ScheduleTaskOnce(intervalInHour, task, scheduleId, dayEnum, startAt);

            if (intervalInHour > 0)
                ScheduleRepeatedTask(intervalInHour, task, scheduleId, dayEnum, startAt);
        }

        public void KillSchedule(Guid scheduleId)
        {
            if (_timers.ContainsKey(scheduleId))
            {
                var timer = _timers[scheduleId];

                timer.Change(Timeout.Infinite, Timeout.Infinite);

                _timers.Remove(scheduleId);
            }
        }
    }
}
