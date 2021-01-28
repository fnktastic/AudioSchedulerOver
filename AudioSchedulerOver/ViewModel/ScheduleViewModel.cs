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
    public class ScheduleViewModel : ViewModelBase, IPlayable, IDirty
    {
        public AudioViewModel Audio { get; set; }
        public string MachineId { get; set; }
        public Guid ScheduleId { get; set; }

        private IntervalEnum _intervalEnum;
        public IntervalEnum IntervalEnum
        {
            get { return _intervalEnum; }
            set
            {
                if (_intervalEnum == value) return;
                _intervalEnum = value;
                RaisePropertyChanged(nameof(IntervalEnum));
                _dirty = true;
            }
        }

        private DayOfWeek _dayEnum;
        public DayOfWeek DayEnum
        {
            get { return _dayEnum; }
            set
            {
                if (_dayEnum == value) return;
                _dayEnum = value;
                RaisePropertyChanged(nameof(DayEnum));
                _dirty = true;
            }
        }

        private int _interval;
        public int Interval
        {
            get { return _interval; }
            set
            {
                if (_interval == value) return;
                _interval = value;
                RaisePropertyChanged(nameof(Interval));
                _dirty = true;
            }
        }

        private TimeSpan _startDate;
        public TimeSpan StartDate
        {
            get { return _startDate; }
            set
            {
                if (_startDate == value) return;
                _startDate = value;
                RaisePropertyChanged(nameof(StartDate));
                _dirty = true;
            }
        }

        private int _hours;
        public int Hours
        {
            get { return _hours; }
            set
            {
                if (_hours == value) return;
                _hours = value;
                RaisePropertyChanged(nameof(Hours));
                UpdateTimeSpan();
                _dirty = true;
            }
        }

        private int _minutes;
        public int Minutes
        {
            get { return _minutes; }
            set
            {
                if (_minutes == value) return;
                _minutes = value;
                RaisePropertyChanged(nameof(Minutes));
                UpdateTimeSpan();
                _dirty = true;
            }
        }

        private int _seconds;
        public int Seconds
        {
            get { return _seconds; }
            set
            {
                if (_seconds == value) return;
                _seconds = value;
                RaisePropertyChanged(nameof(Seconds));
                UpdateTimeSpan();
                _dirty = true;
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                RaisePropertyChanged(nameof(IsActive));
                _dirty = true;
            }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_isPlaying == value) return;
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
                if (_repeatedly == value) return;
                _repeatedly = value;
                RaisePropertyChanged(nameof(Repeatedly));
                _dirty = true;
            }
        }

        private TimeSpan _nextFire;
        public TimeSpan NextFire
        {
            get { return _nextFire; }
            set
            {
                if (_nextFire == value) return;
                _nextFire = value;
                RaisePropertyChanged(nameof(NextFire));
            }
        }

        private bool _dirty;
        /*private bool d;
        public bool _dirty
        {
            get { return d; }
            set
            {
                if (d == value) return;
                d = value;
            }
        }*/
        public bool IsDirty => _dirty;

        public string Path => Audio != null ? Audio.FilePath : string.Empty;

        public void CleanObject() => _dirty = false;

        public Schedule ConvertToSchedule()
        {
            return new Schedule()
            {
                Audio = this.Audio.ConvertToAudio(),
                MachineId = this.MachineId,
                AudioId = this.Audio.Id,
                Id = this.ScheduleId,
                Interval = this.Interval,
                IntervalEnum = this.IntervalEnum,
                StartDate = this.StartDate.Ticks,
                DayEnum = this.DayEnum,
                IsActive = this.IsActive,
                Repeatedly = this.Repeatedly
            };
        }

        private void UpdateTimeSpan()
        {
            StartDate = new TimeSpan(_hours, _minutes, _seconds);
        }
    }
}
