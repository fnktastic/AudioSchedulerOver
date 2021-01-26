using AudioSchedulerOver.DataAccess;
using AudioSchedulerOver.Logging;
using AudioSchedulerOver.Model;
using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Repository
{
    public interface IAudioRepository
    {
        Task<IEnumerable<Audio>> GetAllAsync();

        Task AddAsync(Audio audio);

        Task RemoveAsync(Audio audio);
    }
    class AudioRepository : IAudioRepository
    {
        private readonly IDataContextFactory _dataContextFactory;

        private Context context => _dataContextFactory.Instance;

        public AudioRepository(IDataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public async Task AddAsync(Audio audio)
        {
            try
            {
                context.Audios.Add(audio);

                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        public async Task<IEnumerable<Audio>> GetAllAsync()
        {
            try
            {
                return await context.Audios.ToListAsync();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
                return null;
            }
        }

        public async Task RemoveAsync(Audio audio)
        {
            try
            {
                var dbEntry = await context.Audios.FindAsync(audio.Id);
                if (dbEntry != null)
                {
                    context.Entry(dbEntry).State = EntityState.Deleted;
                }

                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }
    }
}
