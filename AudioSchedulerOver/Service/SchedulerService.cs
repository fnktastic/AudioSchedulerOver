using AudioSchedulerOver.Enum;
using AudioSchedulerOver.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AudioSchedulerOver.Service
{
    public class SchedulerService
    {
        private static SchedulerService _instance;
        private readonly Dictionary<Guid, Timer> _timers = new Dictionary<Guid, Timer>();

        private SchedulerService() { }

        public static SchedulerService Instance => _instance ?? (_instance = new SchedulerService());

        static DateTime GetNextWeekday(System.DayOfWeek day, int extraDay = 1)
        {
            DateTime result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0).AddDays(extraDay);
            while (result.DayOfWeek != day)
                result = result.AddDays(1);
            return result;
        }

        private void ScheduleTaskOnce(double intervalInHour, Action task, Guid scheduleId, DayOfWeek dayEnum, TimeSpan? startAt = null)
        {
            try
            {
                Timer timer;
                TimeSpan timeToGo;
                TimeSpan period;

                var targetDate = GetNextWeekday((System.DayOfWeek)dayEnum);

                targetDate = targetDate.Add(startAt.Value);

                if (DateTime.Now.DayOfWeek == dayEnum)
                {
                    targetDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0); // start of te day

                    targetDate = targetDate.Add(startAt.Value); // add the rest

                    if (DateTime.Now > targetDate)
                        targetDate = targetDate.AddDays(7);
                }

                timeToGo = targetDate - DateTime.Now;

                period = targetDate - targetDate.AddDays(-7);

                timer = new Timer(x =>
                {
                    task.Invoke();
                }, null, timeToGo.Ticks / 10_000, period.Ticks / 10_000);

                AddTimerSafe(scheduleId, timer);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private void ScheduleRepeatedTask(double intervalInHour, Action task, Guid scheduleId, DayOfWeek dayEnum, TimeSpan? startAt = null)
        {
            try
            {
                var targetDate = GetNextWeekday(dayEnum);

                targetDate = targetDate.Add(startAt.Value);

                //if (DateTime.Now.DayOfWeek == dayEnum)
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

                AddTimerSafe(scheduleId, timer);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private void AddTimerSafe(Guid scheduleId, Timer timer)
        {
            if (_timers.ContainsKey(scheduleId))
            {
                KillSchedule(scheduleId);
            }

            _timers.Add(scheduleId, timer);
        }

        public void ScheduleTask(double intervalInHour, Action task, Guid scheduleId, DayOfWeek dayEnum, bool repeatedly, TimeSpan? startAt = null)
        {
            if (repeatedly == false)
            {
                ScheduleTaskOnce(intervalInHour, task, scheduleId, dayEnum, startAt);
            }

            if (repeatedly)
            {
                ScheduleRepeatedTask(intervalInHour, task, scheduleId, dayEnum, startAt);
            }
        }

        public void KillSchedule(Guid scheduleId)
        {
            try
            {
                if (_timers.ContainsKey(scheduleId))
                {
                    var timer = _timers[scheduleId];

                    _timers.Remove(scheduleId);

                    timer.Change(Timeout.Infinite, Timeout.Infinite);

                    timer.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }
    }
}
