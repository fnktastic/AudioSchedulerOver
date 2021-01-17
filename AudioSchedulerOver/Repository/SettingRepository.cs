using AudioSchedulerOver.DataAccess;
using AudioSchedulerOver.Helper;
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
    public interface ISettingRepository
    {
        Task<Setting> Get(string key);

        Task Update(string key, string value);
        Task Init();
    }

    public class SettingRepository : ISettingRepository
    {
        private readonly Context _context;

        public SettingRepository(Context context)
        {
            _context = context;
        }

        public async Task<Setting> Get(string key)
        {
            try
            {
                return await _context.Settings.Where(x => x.MachineId == MachineId.Get).FirstOrDefaultAsync(x => x.Key == key);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
                return null;
            }
        }

        public async Task Update(string key, string value)
        {
            try
            {
                if (value == null)
                    value = string.Empty;

                var dbEntry = await _context.Settings.Where(x => x.MachineId == MachineId.Get).FirstOrDefaultAsync(x => x.Key == key);
                if (dbEntry != null)
                {
                    dbEntry.Value = value;

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        public async Task Init()
        {
            try
            {
                var appName = await Get("appName");
                if (appName == null)
                {
                    var i = new Setting() { Id = Guid.NewGuid(), Key = "appName", Value = "<app name>", MachineId = MachineId.Get };
                    _context.Settings.Add(i);
                    await _context.SaveChangesAsync();
                }

                var tagetVolume = await Get("tagetVolume");
                if (tagetVolume == null)
                {
                    var o = new Setting() { Id = Guid.NewGuid(), Key = "tagetVolume", Value = "27", MachineId = MachineId.Get };
                    _context.Settings.Add(o);
                    await _context.SaveChangesAsync();
                }

                var fadingSpeed = await Get("fadingSpeed");
                if (fadingSpeed == null)
                {
                    var o = new Setting() { Id = Guid.NewGuid(), Key = "fadingSpeed", Value = "0", MachineId = MachineId.Get };
                    _context.Settings.Add(o);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }
    }
}
