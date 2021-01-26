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
        public string Name { get; set; }
        public string Metadata { get; set; }
        public bool IsActive { get; set; }
        //meaning: if true -> load shared settings
        //         if false -> load private settings 
        public bool IsOnline { get; set; }
        public DateTime LatestLoginAt { get; set; }
        public List<Schedule> Schedules { get; set; }
    }
}
