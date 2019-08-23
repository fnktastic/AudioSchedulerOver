using AudioSchedulerOver.Enum;
using AudioSchedulerOver.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.ViewModel
{
    public class ScheduleViewModel
    {
        public Audio Audio { get; set; }

        public IntervalEnum IntervalEnum { get; set; }

        public int Interval { get; set; }
    }
}
