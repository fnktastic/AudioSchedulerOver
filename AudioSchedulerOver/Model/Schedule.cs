using AudioSchedulerOver.Enum;
using AudioSchedulerOver.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Model
{
    public class Schedule
    {
        public Guid Id { get; set; }

        public IntervalEnum IntervalEnum { get; set; }

        public DayOfWeek DayEnum { get; set; }

        public int Interval { get; set; }

        public long StartDate { get; set; }

        public bool IsActive { get; set; }

        public bool Repeatedly { get; set; }

        public Guid AudioId { get; set; }
        public Audio Audio { get; set; }

        public string MachineId { get; set; }
        public Machine Machine { get; set; }

        public ScheduleViewModel ConvertToScheduleViewModel()
        {
            var timeSpan = TimeSpan.FromTicks(this.StartDate);

            return new ScheduleViewModel()
            {
                Audio = this.Audio,
                Interval = this.Interval,
                IntervalEnum = this.IntervalEnum,
                ScheduleId = this.Id,
                StartDate = timeSpan,
                Hours = timeSpan.Hours,
                Minutes = timeSpan.Minutes,
                Seconds = timeSpan.Seconds,
                DayEnum = this.DayEnum,
                IsActive = this.IsActive,
                Repeatedly = this.Repeatedly,
            };
        }
    }
}
