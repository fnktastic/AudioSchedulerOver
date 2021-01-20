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
        private Context _context
        {
            get
            {
                return ServiceLocator.Current.GetInstance<Context>();
            }
        }

        public AudioRepository(Context context)
        {
            //_context = context;
        }

        public async Task AddAsync(Audio audio)
        {
            try
            {
                _context.Audios.Add(audio);

                await _context.SaveChangesAsync();
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
                return await _context.Audios.ToListAsync();
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
                var dbEntry = await _context.Audios.FindAsync(audio.Id);
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
    }
}
