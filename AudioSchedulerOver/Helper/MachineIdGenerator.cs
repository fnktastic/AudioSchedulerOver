using DeviceId;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Helper
{
    public static class MachineIdGenerator
    {
        private static readonly string _id;
        static MachineIdGenerator()
        {
            _id = new DeviceIdBuilder()
                .AddMachineName()
                .AddMacAddress()
                .AddProcessorId()
                .AddMotherboardSerialNumber()
                .ToString();
        }
        public static string Get => _id;

        public static string Name => Environment.MachineName;
    }
}
