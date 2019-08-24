using AudioSchedulerOver.Enum;
using AudioSchedulerOver.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AudioSchedulerOver.Scheduler
{
    public class AudioPlaybackScheduler
    {
        public void Interval(double interval, Action task, IntervalEnum intervalEnum, Guid scheduleId, DateTime? startAt = null)
        {
            switch (intervalEnum)
            {
                case IntervalEnum.Day:
                    interval *= 24;
                    break;
                case IntervalEnum.Hour:
                    break;
                case IntervalEnum.Minute:
                    interval /= 60;
                    break;
                case IntervalEnum.Second:
                    interval /= 3600;
                    break;
            }

            SchedulerService.Instance.ScheduleTask(interval, task, scheduleId, startAt);
        }

        public void KillSchedule(Guid scheduleId)
        {
            SchedulerService.Instance.KillSchedule(scheduleId);
        }
    }
}
