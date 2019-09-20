using AudioSchedulerOver.DataAccess;
using AudioSchedulerOver.Logging;
using AudioSchedulerOver.Model;
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
        IEnumerable<Audio> GetAll();

        Task AddAsync(Audio audio);

        Task RemoveAsync(Audio audio);
    }
    class AudioRepository : IAudioRepository
    {
        private readonly Context _context;

        public AudioRepository(Context context)
        {
            _context = context;
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

        public IEnumerable<Audio> GetAll()
        {
            try
            {
                return _context.Audios.ToList();
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
                var dbEntry = _context.Audios.Find(audio.Id);
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
