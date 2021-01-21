using AudioSchedulerOver.Enum;
using AudioSchedulerOver.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace AudioSchedulerOver.Service
{
    public class TimeToGo
    {
        public Timer Timer { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public double IntervalInHour { get; set; }

        public TimeToGo(Timer timer, TimeSpan timeSpan, double intervalInhour)
        {
            Timer = timer;
            TimeSpan = timeSpan;
            IntervalInHour = intervalInhour;
        }
    }

    public class SchedulerService
    {
        private static SchedulerService _instance;
        private readonly Dictionary<Guid, TimeToGo> _timers = new Dictionary<Guid, TimeToGo>();

        private SchedulerService() { }

        public static SchedulerService Instance => _instance ?? (_instance = new SchedulerService());

        public Dictionary<Guid, TimeToGo> GetTimers() => _timers;

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
                TimeSpan runIn;
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

                runIn = targetDate - DateTime.Now;

                period = targetDate - targetDate.AddDays(-7);

                timer = new Timer(x =>
                {
                    task.Invoke();
                }, null, runIn.Ticks / 10_000, period.Ticks / 10_000);

                var timeToGo = new TimeToGo(timer, runIn, intervalInHour);

                AddTimerSafe(scheduleId, timeToGo);
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

                var runIn = targetDate - DateTime.Now;

                var timer = new Timer(x =>
                {
                    task.Invoke();
                }, null, runIn, TimeSpan.FromHours(intervalInHour));

                var timeToGo = new TimeToGo(timer, runIn, intervalInHour);

                AddTimerSafe(scheduleId, timeToGo);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private void AddTimerSafe(Guid scheduleId, TimeToGo timeToGo)
        {
            if (_timers.ContainsKey(scheduleId))
            {
                KillSchedule(scheduleId);
            }

            _timers.Add(scheduleId, timeToGo);
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
                    var timeToGo = _timers[scheduleId];

                    _timers.Remove(scheduleId);

                    timeToGo.Timer.Change(Timeout.Infinite, Timeout.Infinite);

                    timeToGo.Timer.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }
    }
}
