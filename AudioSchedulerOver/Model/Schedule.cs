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

        public DayEnum DayEnum { get; set; }

        public int Interval { get; set; }

        public long StartDate { get; set; }

        public Audio Audio { get; set; }

        public ScheduleViewModel ConvertToScheduleViewModel()
        {
            return new ScheduleViewModel()
            {
                Audio = this.Audio,
                Interval = this.Interval,
                IntervalEnum = this.IntervalEnum,
                ScheduleId = this.Id,
                StartDate = TimeSpan.FromTicks(this.StartDate),
                DayEnum = this.DayEnum
            };
        }
    }
}
