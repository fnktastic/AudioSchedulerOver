using AudioSchedulerOver.DataAccess;
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
            _context.Audios.Add(audio);

            await _context.SaveChangesAsync();
        }

        public IEnumerable<Audio> GetAll()
        {
            return _context.Audios.ToList();
        }

        public async Task RemoveAsync(Audio audio)
        {
            var dbEntry = _context.Audios.Find(audio.Id);
            if(dbEntry != null)
            {
                _context.Entry(dbEntry).State = EntityState.Deleted;
            }

            await _context.SaveChangesAsync();
        }
    }
}
