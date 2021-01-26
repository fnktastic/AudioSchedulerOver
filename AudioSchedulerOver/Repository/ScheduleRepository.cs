using AudioSchedulerOver.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioSchedulerOver.DataAccess;
using System.Data.Entity;
using AudioSchedulerOver.Logging;
using CommonServiceLocator;
using AudioSchedulerOver.Helper;

namespace AudioSchedulerOver.Repository
{
    public interface IScheduleRepository
    {
        Task<IEnumerable<Schedule>> GetAllAsync(string machineId = null);

        Task RemoveAsync(Schedule schedule);

        Task UpdateAsync(Schedule schedule);
    }
    class ScheduleRepository : IScheduleRepository
    {
        private readonly IDataContextFactory _dataContextFactory;

        public ScheduleRepository(IDataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        private async Task AddAsync(Schedule schedule)
        {
            try
            {
                using (var context = _dataContextFactory.Instance)
                {
                    context.Schedules.Add(schedule);

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        public async Task<IEnumerable<Schedule>> GetAllAsync(string machineId = null)
        {
            try
            {
                using (var context = _dataContextFactory.Instance)
                {
                    if (machineId != null)
                    {
                        return await context.Schedules.Where(x => x.MachineId == machineId).Include(x => x.Audio).ToListAsync();
                    }

                    return await context.Schedules.Include(x => x.Audio).ToListAsync();
                }
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
                using (var context = _dataContextFactory.Instance)
                {
                    var dbEntry = await context.Schedules.FindAsync(schedule.Id);
                    if (dbEntry != null)
                    {
                        context.Entry(dbEntry).State = EntityState.Deleted;
                    }

                    await context.SaveChangesAsync();
                }
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
                using (var context = _dataContextFactory.Instance)
                {

                    var dbEntry = await context.Schedules.FindAsync(schedule.Id);

                    if (dbEntry != null)
                    {
                        dbEntry.AudioId = schedule.AudioId;
                        dbEntry.Interval = schedule.Interval;
                        dbEntry.IntervalEnum = schedule.IntervalEnum;
                        dbEntry.StartDate = schedule.StartDate;
                        dbEntry.DayEnum = schedule.DayEnum;
                        dbEntry.IsActive = schedule.IsActive;
                        dbEntry.Repeatedly = schedule.Repeatedly;

                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        schedule.Audio = null;
                        schedule.MachineId = MachineIdGenerator.Get;
                        await AddAsync(schedule);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }
    }
}
