using AudioSchedulerOver.Enum;
using AudioSchedulerOver.Model;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.ViewModel
{
    public class ScheduleViewModel : ViewModelBase
    {
        public Audio Audio { get; set; }

        public Guid ScheduleId { get; set; }

        private IntervalEnum _intervalEnum;
        public IntervalEnum IntervalEnum
        {
            get { return _intervalEnum; }
            set
            {
                _intervalEnum = value;
                RaisePropertyChanged(nameof(IntervalEnum));
            }
        }

        private DayEnum _dayEnum;
        public DayEnum DayEnum
        {
            get { return _dayEnum; }
            set
            {
                _dayEnum = value;
                RaisePropertyChanged(nameof(DayEnum));
            }
        }

        private int _interval;
        public int Interval
        {
            get { return _interval; }
            set
            {
                _interval = value;
                RaisePropertyChanged(nameof(Interval));
            }
        }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                RaisePropertyChanged(nameof(StartDate));
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                RaisePropertyChanged(nameof(IsActive)); 
            }
        }

        public Schedule ConvertToSchedule()
        {
            return new Schedule()
            {
                Audio = this.Audio,
                Id = this.ScheduleId,
                Interval = this.Interval,
                IntervalEnum = this.IntervalEnum,
                StartDate = this.StartDate,
                DayEnum = this.DayEnum
            };
        }
    }
}
