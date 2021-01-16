using DeviceId;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Helper
{
    public static class MachineId
    {
        private static string _id;
        static MachineId()
        {
            _id = new DeviceIdBuilder()
                .AddMachineName()
                .AddMacAddress()
                .AddProcessorId()
                .AddMotherboardSerialNumber()
                .ToString();
        }
        public static string Get => _id;
    }
}
