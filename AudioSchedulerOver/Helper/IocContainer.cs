using AudioSchedulerOver.DataAccess;
using AudioSchedulerOver.Repository;
using LightInject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Helper
{
    public static class IocContainer
    {
        public static void Init()
        {
            var container = new ServiceContainer();
            container.Register<Context>();
            container.Register<IAudioRepository, AudioRepository>();
            container.Register<IMachineRepository, MachineRepository>();
            container.Register<IScheduleRepository, ScheduleRepository>();
            container.Register<ISettingRepository, SettingRepository>();
        }
    }
}
