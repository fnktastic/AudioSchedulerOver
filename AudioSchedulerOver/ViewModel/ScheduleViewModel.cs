using AudioSchedulerOver.Enum;
using AudioSchedulerOver.Helper;
using AudioSchedulerOver.Interface;
using AudioSchedulerOver.Model;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.ViewModel
{
    public class ScheduleViewModel : ViewModelBase, IPlayable
    {
        public AudioViewModel Audio { get; set; }

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

        private DayOfWeek _dayEnum;
        public DayOfWeek DayEnum
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

        private TimeSpan _startDate;
        public TimeSpan StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                RaisePropertyChanged(nameof(StartDate));
            }
        }

        private int _hours;
        private int _minutes;
        private int _seconds;

        public int Hours
        {
            get { return _hours; }
            set
            {
                _hours = value;
                RaisePropertyChanged(nameof(Hours));
                UpdateTimeSpan();
            }
        }

        public int Minutes
        {
            get { return _minutes; }
            set
            {
                _minutes = value;
                RaisePropertyChanged(nameof(Minutes));
                UpdateTimeSpan();
            }
        }

        public int Seconds
        {
            get { return _seconds; }
            set
            {
                _seconds = value;
                RaisePropertyChanged(nameof(Seconds));
                UpdateTimeSpan();
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

        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                RaisePropertyChanged(nameof(IsPlaying));
            }
        }

        private bool _repeatedly;
        public bool Repeatedly
        {
            get { return _repeatedly; }
            set
            {
                _repeatedly = value;
                RaisePropertyChanged(nameof(Repeatedly));
            }
        }

        private TimeSpan _nextFire;
        public TimeSpan NextFire
        {
            get { return _nextFire; }
            set
            {
                _nextFire = value;
                RaisePropertyChanged(nameof(NextFire));
            }
        }

        public Schedule ConvertToSchedule()
        {
            return new Schedule()
            {
                Audio = this.Audio.ConvertToAudio(),
                AudioId = this.Audio.Id,
                Id = this.ScheduleId,
                Interval = this.Interval,
                IntervalEnum = this.IntervalEnum,
                StartDate = this.StartDate.Ticks,
                DayEnum = this.DayEnum,
                IsActive = this.IsActive,
                Repeatedly = this.Repeatedly,
                MachineId = MachineIdGenerator.Get
            };
        }

        private void UpdateTimeSpan()
        {
            StartDate = new TimeSpan(_hours, _minutes, _seconds);
        }
    }
}
