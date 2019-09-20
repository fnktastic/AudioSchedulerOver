using AudioSchedulerOver.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioSchedulerOver.DataAccess;
using System.Data.Entity;
using AudioSchedulerOver.Logging;

namespace AudioSchedulerOver.Repository
{
    public interface IScheduleRepository
    {
        IEnumerable<Schedule> GetAll();

        Task AddAsync(Schedule schedule);

        Task RemoveAsync(Schedule schedule);

        Task UpdateAsync(Schedule schedule);
    }
    class ScheduleRepository : IScheduleRepository
    {
        private readonly Context _context;

        public ScheduleRepository(Context context)
        {
            _context = context;
        }

        public async Task AddAsync(Schedule schedule)
        {
            try
            {
                _context.Schedules.Add(schedule);

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        public IEnumerable<Schedule> GetAll()
        {
            try
            {
                return _context.Schedules.Include(x => x.Audio).ToList();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
                return null;
            }
        }

        public async Task RemoveAsync(Schedule schedule)
        {
            try
            {
                var dbEntry = _context.Schedules.Find(schedule.Id);
                if (dbEntry != null)
                {
                    _context.Entry(dbEntry).State = EntityState.Deleted;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        public async Task UpdateAsync(Schedule schedule)
        {
            try
            {
                var dbEntry = _context.Schedules.Find(schedule.Id);

                if (dbEntry != null)
                {
                    dbEntry.Audio = schedule.Audio;
                    dbEntry.Interval = schedule.Interval;
                    dbEntry.IntervalEnum = schedule.IntervalEnum;
                    dbEntry.StartDate = schedule.StartDate;
                    dbEntry.DayEnum = schedule.DayEnum;

                    await _context.SaveChangesAsync();
                }
                else
                {
                    await AddAsync(schedule);
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }
    }
}
