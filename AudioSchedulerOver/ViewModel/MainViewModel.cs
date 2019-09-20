﻿using AudioSchedulerOver.DataAccess;
using AudioSchedulerOver.Enum;
using AudioSchedulerOver.Logging;
using AudioSchedulerOver.Model;
using AudioSchedulerOver.Repository;
using AudioSchedulerOver.Scheduler;
using AudioSchedulerOver.Service;
using AudioSession;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioSchedulerOver.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Context _context;
        private readonly PlayerService _playerService;
        private readonly AudioPlaybackScheduler _audioPlaybackScheduler;
        private readonly IAudioRepository _audioRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly ISettingRepository _settingRepository;

        private ApplicationVolumeProvider _applicationVolumeProvider;

        private const string APP = "Y.Music";

        private const int AUTOCHECK_INTERVAL = 3;

        private bool isConnectSuccess;

        private bool isAutoRunFired = false;

        private readonly Timer connectionTimer;

        #region properties
        private ObservableCollection<Audio> _audios;
        public ObservableCollection<Audio> Audios
        {
            get { return _audios; }
            set
            {
                _audios = value;
                RaisePropertyChanged(nameof(Audios));
            }
        }

        private ObservableCollection<ScheduleViewModel> _scheduleViewModels;
        public ObservableCollection<ScheduleViewModel> ScheduleViewModels
        {
            get { return _scheduleViewModels; }
            set
            {
                _scheduleViewModels = value;
                RaisePropertyChanged(nameof(ScheduleViewModels));
            }
        }

        private int _targetVolume;
        public int TargetVolunme
        {
            get { return _targetVolume; }
            set
            {
                _targetVolume = value;
                RaisePropertyChanged(nameof(TargetVolunme));
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                RaisePropertyChanged(nameof(ErrorMessage));
            }
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get { return _successMessage; }
            set
            {
                _successMessage = value;
                RaisePropertyChanged(nameof(SuccessMessage));
            }
        }

        private string _appName;
        public string AppName
        {
            get { return _appName; }
            set
            {
                _appName = value;
                RaisePropertyChanged(nameof(AppName));
            }
        }

        private IntervalEnum _selectedInterval;
        public IntervalEnum SelectedInterval
        {
            get { return _selectedInterval; }
            set
            {
                _selectedInterval = value;
                RaisePropertyChanged(nameof(SelectedInterval));
            }
        }
        #endregion

        public MainViewModel()
        {
            _context = new Context();
            _audioRepository = new AudioRepository(_context);
            _scheduleRepository = new ScheduleRepository(_context);
            _settingRepository = new SettingRepository(_context);
            _settingRepository.Init();

            var mediaPlayer = new MediaPlayer();
            _audioPlaybackScheduler = new AudioPlaybackScheduler();
            _playerService = new PlayerService(mediaPlayer);
            Audios = new ObservableCollection<Audio>(_audioRepository.GetAll());
            ScheduleViewModels = new ObservableCollection<ScheduleViewModel>
                (
                _scheduleRepository
                .GetAll()
                .Select(x => x.ConvertToScheduleViewModel())
                );

            TargetVolunme = int.Parse(GetSetting("tagetVolume"));
            AppName = GetSetting("appName");

            connectionTimer = new Timer(x =>
            {
                EstablishConnection();

            }, null, TimeSpan.FromSeconds(AUTOCHECK_INTERVAL), TimeSpan.FromSeconds(AUTOCHECK_INTERVAL));
        }

        private RelayCommand<DragEventArgs> _dropAudioCommand;
        public RelayCommand<DragEventArgs> DropAudioCommand => _dropAudioCommand ?? (_dropAudioCommand = new RelayCommand<DragEventArgs>(DropAudio));
        private async void DropAudio(DragEventArgs e)
        {
            try
            {
                var drop = e.Data.GetData("FileDrop");
                if (drop != null && drop is string[])
                {
                    var files = drop as string[];

                    foreach (var file in files)
                    {
                        var audio = Audio.CreateInstnceFromPath(file);

                        await _audioRepository.AddAsync(audio);

                        Audios.Add(audio);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", ex.Message, ex.StackTrace, ex.Data));
            }
        }

        private RelayCommand _connectToAppCommnd;
        public RelayCommand ConnectToAppCommnd => _connectToAppCommnd ?? (_connectToAppCommnd = new RelayCommand(ConnectToApp));
        private void ConnectToApp()
        {
            try
            {
                EstablishConnection();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<Audio> _peviewAudioCommand;
        public RelayCommand<Audio> PeviewAudioCommand => _peviewAudioCommand ?? (_peviewAudioCommand = new RelayCommand<Audio>(PeviewAudio));
        private void PeviewAudio(Audio audio)
        {
            try
            {
                _playerService.OpenAndPlay(audio);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<Audio> _removeAudioCommand;
        public RelayCommand<Audio> RemoveAudioCommand => _removeAudioCommand ?? (_removeAudioCommand = new RelayCommand<Audio>(RemoveAudio));
        private async void RemoveAudio(Audio audio)
        {
            try
            {
                _audios.Remove(audio);

                await _audioRepository.RemoveAsync(audio);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<Audio> _addAudioToScheduleCommand;
        public RelayCommand<Audio> AddAudioToScheduleCommand => _addAudioToScheduleCommand ?? (_addAudioToScheduleCommand = new RelayCommand<Audio>(AddAudioToSchedule));
        private void AddAudioToSchedule(Audio audio)
        {
            try
            {
                var scheduleViewModel = new ScheduleViewModel()
                {
                    Audio = audio,
                    ScheduleId = Guid.NewGuid(),
                    Interval = 0,
                    IntervalEnum = IntervalEnum.Second,
                    StartDate = TimeSpan.Zero
                };

                ScheduleViewModels.Add(scheduleViewModel);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<ScheduleViewModel> _startScheduledPlaybackCommand;
        public RelayCommand<ScheduleViewModel> StartScheduledPlaybackCommand => _startScheduledPlaybackCommand ?? (_startScheduledPlaybackCommand = new RelayCommand<ScheduleViewModel>(StartScheduledPlayback));
        private void StartScheduledPlayback(ScheduleViewModel scheduleViewModel)
        {
            try
            {
                if (isConnectSuccess == false)
                {
                    ErrorMessage = "Error. Connect to the media player first.";
                    return;
                }

                Audio audio = scheduleViewModel.Audio;
                TimeSpan start = scheduleViewModel.StartDate;
                int interval = scheduleViewModel.Interval;
                IntervalEnum intervalEnum = scheduleViewModel.IntervalEnum;
                Guid scheduleId = scheduleViewModel.ScheduleId;
                DayOfWeek dayEnum = scheduleViewModel.DayEnum;

                _audioPlaybackScheduler.Interval(interval, () =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Action(() =>
                    {
                        try
                        {
                            _applicationVolumeProvider.SetApplicationVolume(_targetVolume);
                            _playerService.OpenAndPlay(audio);
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
                        }
                    }));
                }, intervalEnum, scheduleId, dayEnum, start);

                scheduleViewModel.IsActive = true;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<ScheduleViewModel> _stopScheduledPlaybackCommand;
        public RelayCommand<ScheduleViewModel> StopScheduledPlaybackCommand => _stopScheduledPlaybackCommand ?? (_stopScheduledPlaybackCommand = new RelayCommand<ScheduleViewModel>(StopScheduledPlayback));
        private void StopScheduledPlayback(ScheduleViewModel scheduleViewModel)
        {
            try
            {
                Guid scheduleId = scheduleViewModel.ScheduleId;

                _audioPlaybackScheduler.KillSchedule(scheduleId);

                scheduleViewModel.IsActive = false;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<ScheduleViewModel> _playScheduleCommand;
        public RelayCommand<ScheduleViewModel> PlayScheduleCommand => _playScheduleCommand ?? (_playScheduleCommand = new RelayCommand<ScheduleViewModel>(PlaySchedule));
        private void PlaySchedule(ScheduleViewModel scheduleViewModel)
        {
            try
            {
                PeviewAudio(scheduleViewModel.Audio);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<ScheduleViewModel> _removeScheduleCommand;
        public RelayCommand<ScheduleViewModel> RemoveScheduleCommand => _removeScheduleCommand ?? (_removeScheduleCommand = new RelayCommand<ScheduleViewModel>(RemoveSchedule));
        private async void RemoveSchedule(ScheduleViewModel scheduleViewModel)
        {
            try
            {
                ScheduleViewModels.Remove(scheduleViewModel);

                await _scheduleRepository.RemoveAsync(scheduleViewModel.ConvertToSchedule());
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<ScheduleViewModel> _saveScheduleCommand;
        public RelayCommand<ScheduleViewModel> SaveScheduleCommand => _saveScheduleCommand ?? (_saveScheduleCommand = new RelayCommand<ScheduleViewModel>(SaveSchedule));
        private async void SaveSchedule(ScheduleViewModel scheduleViewModel)
        {
            try
            {
                await _scheduleRepository.UpdateAsync(scheduleViewModel.ConvertToSchedule());
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<object> _onAppCloseCommand;
        public RelayCommand<object> OnAppCloseCommand => _onAppCloseCommand ?? (_onAppCloseCommand = new RelayCommand<object>(OnAppClose));
        private void OnAppClose(object o)
        {
            try
            {
                UpdateConfigs();

                SaveData();

                if (_applicationVolumeProvider != null)
                    _applicationVolumeProvider.SetApplicationVolume(100);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));

                Logging.Logger.Log.Error(e);

                UpdateConfigs();

                SaveData();

                if (_applicationVolumeProvider != null)
                    _applicationVolumeProvider.SetApplicationVolume(100);
            }
            finally
            {
                Logger.Log.Info("Application Shutdown");
            }
        }

        private RelayCommand<object> _saveCommandCommnd;
        public RelayCommand<object> SaveCommandCommnd => _saveCommandCommnd ?? (_saveCommandCommnd = new RelayCommand<object>(SaveCommand));
        private void SaveCommand(object e)
        {

        }

        private void UpdateConfigs()
        {
            try
            {
                UpdateSetting("appName", _appName);
                UpdateSetting("tagetVolume", _targetVolume.ToString());
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private async void UpdateSetting(string key, string value)
        {
            try
            {
                await _settingRepository.Update(key, value);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private string GetSetting(string key)
        {
            try
            {
                return _settingRepository.Get(key).Value;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
                return string.Empty;
            }
        }

        private bool EstablishConnection()
        {
            try
            {
                if (IsProcessExist(_appName) == false)
                {
                    ErrorMessage = "Cant find the target app by given name. Check exact name of the app and try again.";
                    SuccessMessage = string.Empty;
                    isConnectSuccess = false;

                    foreach (var scheduleViewModel in ScheduleViewModels.Where(x => x.IsActive == true))
                    {
                        StopScheduledPlayback(scheduleViewModel);
                    }

                    isAutoRunFired = false;

                    return false;
                }

                _applicationVolumeProvider = new ApplicationVolumeProvider(_appName);

                PlayerService.ApplicationVolumeProvider = _applicationVolumeProvider;

                if (_applicationVolumeProvider.IsConnected)
                {
                    float? appVolume = _applicationVolumeProvider.GetApplicationVolume();
                    SuccessMessage = string.Format("App {0} is detected", _appName);
                    ErrorMessage = string.Empty;
                    isConnectSuccess = true;

                    if (isAutoRunFired == false)
                    {
                        foreach (var scheduleViewModel in ScheduleViewModels.Where(x => x.IsActive == false))
                        {
                            StartScheduledPlayback(scheduleViewModel);
                        }

                        isAutoRunFired = true;
                    }

                    //connectionTimer.Change(Timeout.Infinite, Timeout.Infinite);

                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
                return false;
            }
        }
        private bool IsProcessExist(string processName)
        {
            try
            {
                var process = Process.GetProcesses().FirstOrDefault(x => x.ProcessName == processName);

                if (process == null)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
                return false;
            }
        }

        private async void SaveData()
        {
            try
            {
                foreach (var schedule in _scheduleViewModels)
                {
                    await _scheduleRepository.UpdateAsync(schedule.ConvertToSchedule()); //.GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }
    }
}