using AudioSchedulerOver.DataAccess;
using AudioSchedulerOver.Exceptions;
using AudioSchedulerOver.Logging;
using AudioSchedulerOver.Model;
using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Repository
{
    public interface IMachineRepository
    {
        Task<Machine> SignIn(string machineId, string name);
    }

    public class MachineRepository : IMachineRepository
    {
        private readonly Context _context;

        public MachineRepository(Context context)
        {
            _context = context;
        }

        public async Task<Machine> SignIn(string machineId, string name)
        {
            try
            {
                var machine = await _context.Machines.FindAsync(machineId);

                if (machine != null)
                {
                    if (machine.IsActive == false) throw new StationInactiveException();

                    machine.LatestLoginAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return machine;
                }

                var newMachine = new Machine()
                {
                    Id = machineId,
                    Name = name,
                    IsActive = true,
                    IsOnline = true,
                    LatestLoginAt = DateTime.UtcNow
                };

                _context.Machines.Add(newMachine);

                await _context.SaveChangesAsync();

                return newMachine;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
                return null;
            }
        }
    }
}
