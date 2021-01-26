
using AudioSchedulerOver.DataAccess;
using AudioSchedulerOver.Helper;
using AudioSchedulerOver.Repository;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;

namespace AudioSchedulerOver.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();

            SimpleIoc.Default.Register<IDataContextFactory, DataContextFactory>();

            SimpleIoc.Default.Register<ISerialQueue, SerialQueue>();

            SimpleIoc.Default.Register<IAudioRepository, AudioRepository>();
            SimpleIoc.Default.Register<ISettingRepository, SettingRepository>();
            SimpleIoc.Default.Register<IScheduleRepository, ScheduleRepository>();
            SimpleIoc.Default.Register<IMachineRepository, MachineRepository>();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}