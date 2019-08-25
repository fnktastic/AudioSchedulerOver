using AudioSchedulerOver.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioSchedulerOver.DataAccess;
using System.Data.Entity;

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
            _context.Schedules.Add(schedule);

            await _context.SaveChangesAsync();
        }

        public IEnumerable<Schedule> GetAll()
        {
            return _context.Schedules.Include(x => x.Audio).ToList();
        }

        public async Task RemoveAsync(Schedule schedule)
        {
            var dbEntry = _context.Schedules.Find(schedule.Id);
            if (dbEntry != null)
            {
                _context.Entry(dbEntry).State = EntityState.Deleted;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Schedule schedule)
        {
            var dbEntry = _context.Schedules.Find(schedule.Id);

            if(dbEntry != null)
            {
                dbEntry.Audio = schedule.Audio;
                dbEntry.Interval = schedule.Interval;
                dbEntry.IntervalEnum = schedule.IntervalEnum;
                dbEntry.StartDate = schedule.StartDate;
                dbEntry.DayEnum = schedule.DayEnum;

                await _context.SaveChangesAsync();
            }
        }
    }
}
