using AudioSchedulerOver.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Model
{
    public class Machine
    {
        public string Id { get; set; } = MachineIdGenerator.Get;
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LatestLoginAt { get; set; }
        public List<Schedule> Schedules { get; set; }
    }
}
