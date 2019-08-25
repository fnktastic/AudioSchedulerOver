using AudioSchedulerOver.DataAccess;
using AudioSchedulerOver.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Repository
{
    public interface ISettingRepository
    {
        Setting Get(string key);

        Task Update(string key, string value);
        void Init();
    }

    public class SettingRepository : ISettingRepository
    {
        private readonly Context _context;

        public SettingRepository(Context context)
        {
            _context = context;
        }

        public Setting Get(string key)
        {
            return _context.Settings.ToList().FirstOrDefault(x => x.Key == key);
        }

        public async Task Update(string key, string value)
        {
            if (value == null)
                value = string.Empty;

            var dbEntry = _context.Settings.FirstOrDefault(x => x.Key == key);
            if (dbEntry != null)
            {
                dbEntry.Value = value;

                await _context.SaveChangesAsync();
            }
        }

        public void Init()
        {
            var appName = Get("appName");
            if (appName == null)
            {
                var i = new Setting() { Id = 1, Key = "appName", Value = "<app name>" };
                _context.Settings.Add(i);
                _context.SaveChanges();
            }

            var tagetVolume = Get("tagetVolume");
            if (tagetVolume == null)
            {
                var o = new Setting() { Id = 2, Key = "tagetVolume", Value = "27" };
                _context.Settings.Add(o);
                _context.SaveChanges();
            }
        }
    }
}
