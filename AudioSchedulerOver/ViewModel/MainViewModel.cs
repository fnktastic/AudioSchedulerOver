﻿using AudioSchedulerOver.DataAccess;
using AudioSchedulerOver.Enum;
using AudioSchedulerOver.Exceptions;
using AudioSchedulerOver.Helper;
using AudioSchedulerOver.Logging;
using AudioSchedulerOver.Model;
using AudioSchedulerOver.Repository;
using AudioSchedulerOver.Scheduler;
using AudioSchedulerOver.Service;
using AudioSession;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioSchedulerOver.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly PlayerService _playerService;
        private readonly AudioPlaybackScheduler _audioPlaybackScheduler;
        private IAudioRepository _audioRepository;
        private IScheduleRepository _scheduleRepository;
        private ISettingRepository _settingRepository;
        private IMachineRepository _machineRepository;

        private ApplicationVolumeProvider _applicationVolumeProvider;

        private const string APP = "Y.Music";

        private const int AUTOCHECK_INTERVAL = 3;

        private readonly double autoReloadInterval = 15;

        private const string STARTUP_CONFIGS = "startupConfigs.txt";

        private const int DEFAULT_REPEAT_INTERVAL = 1;

        private bool isAutoRunFired = false;

        private bool loggedIn = false;

        public static int Fading_Speed = 0;

        private readonly DispatcherTimer connectionTimer = new DispatcherTimer();

        private readonly DispatcherTimer configTimer = new DispatcherTimer();

        private readonly string databasePath;

        private readonly object _locker = new object();

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

        private ObservableCollection<ScheduleViewModel> _schedules;
        public ObservableCollection<ScheduleViewModel> Schedules
        {
            get { return _schedules; }
            set
            {
                _schedules = value;
                RaisePropertyChanged(nameof(Schedules));
            }
        }

        private Machine _machine;
        public Machine Machine
        {
            get { return _machine; }
            set
            {
                _machine = value;
                RaisePropertyChanged(nameof(Machine));
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

        private int _fadingSpeed;
        public int FadingSpeed
        {
            get { return _fadingSpeed; }
            set
            {
                _fadingSpeed = value;
                Fading_Speed = _fadingSpeed;
                RaisePropertyChanged(nameof(FadingSpeed));
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

        private bool _isConnectSuccess;
        public bool IsConnectSuccess
        {
            get { return _isConnectSuccess; }
            set
            {
                _isConnectSuccess = value;
                RaisePropertyChanged(nameof(IsConnectSuccess));
            }
        }

        private string _audiosSearchTerm;
        public string AudiosSearchTerm
        {
            get { return _audiosSearchTerm; }
            set
            {
                _audiosSearchTerm = value;
                RaisePropertyChanged(nameof(AudiosSearchTerm));

                if (FilteredAudiosCollection != null)
                    FilteredAudiosCollection.Refresh();
            }
        }

        private ListCollectionView _filteredAudiosCollection;
        public ListCollectionView FilteredAudiosCollection
        {
            get => _filteredAudiosCollection;
            set
            {
                if (Equals(_filteredAudiosCollection, value)) return;
                _filteredAudiosCollection = value;
                RaisePropertyChanged(nameof(FilteredAudiosCollection));
            }
        }

        public ListCollectionView GetAudiosCollectionView(IEnumerable<Audio> audios)
        {
            return (ListCollectionView)CollectionViewSource.GetDefaultView(audios);
        }

        #endregion

        public MainViewModel(IAudioRepository audioRepository, IScheduleRepository scheduleRepository, IMachineRepository machineRepository, ISettingRepository settingRepository)
        {
            if (File.Exists(STARTUP_CONFIGS))
            {
                var confs = File.ReadAllLines(STARTUP_CONFIGS);
                databasePath = confs[0];

                if (Path.IsPathRooted(databasePath) == false)
                {
                    string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                    databasePath = Path.Combine(userFolder, databasePath);
                }

                autoReloadInterval = double.Parse(confs[1], System.Globalization.NumberStyles.Any);
            }

            _audioRepository = audioRepository;
            _scheduleRepository = scheduleRepository;
            _settingRepository = settingRepository;
            _machineRepository = machineRepository;

            var mediaPlayer = new MediaPlayer();

            _playerService = new PlayerService(mediaPlayer);

            _audioPlaybackScheduler = new AudioPlaybackScheduler();

            Task.WhenAll(Init());
        }

        private async Task Init()
        {
            isAutoRunFired = false;

            await _settingRepository.Init();

            var audios = await _audioRepository.GetAllAsync();
            
            Audios = new ObservableCollection<Audio>(audios);
            Schedules = new ObservableCollection<ScheduleViewModel>
                (
                    (await _scheduleRepository
                        .GetAllAsync())
                        .Select(x => x.ConvertToScheduleViewModel())
                );

            TargetVolunme = int.Parse(await GetSetting("tagetVolume"));
            AppName = await GetSetting("appName");
            FadingSpeed = int.Parse(await GetSetting("fadingSpeed"));

            FilteredAudiosCollection = GetAudiosCollectionView(_audios);
            FilteredAudiosCollection.Filter += FilteredAudioCollection_Filter;

            try
            {
                if (loggedIn == false)
                {
                    Machine = await _machineRepository.SignIn(MachineId.Get);
                    loggedIn = true;
                }
            }
            catch(StationInactiveException)
            {
                Application.Current.Shutdown(-1);
            }

            InitTimers();
        }

        private void InitTimers()
        {
            connectionTimer.Interval = TimeSpan.FromSeconds(AUTOCHECK_INTERVAL);
            connectionTimer.Tick += new EventHandler((object s, EventArgs a) =>
            {
                EstablishConnection();
            });
            connectionTimer.Start();

            configTimer.Interval = TimeSpan.FromMinutes(autoReloadInterval);
            configTimer.Tick += new EventHandler(async (object s, EventArgs a) =>
            {
                {
                    await UpdateConfigs().ContinueWith(async i => 
                    {
                        await SaveData().ContinueWith(async t => 
                        {
                            DisableSchedules();

                            await Init().ContinueWith(j =>
                            {
                                EnableSchedules();
                            });
                        });
                    });
                }
            });
            configTimer.Start();
        }

        #region filters
        private bool FilteredAudioCollection_Filter(object obj)
        {
            try
            {
                var audio = obj as Audio;

                if (string.IsNullOrWhiteSpace(_audiosSearchTerm))
                {
                    return true;
                }

                if (audio.Name.ToUpper().Contains(_audiosSearchTerm.ToUpper()) ||
                    audio.FilePath.ToUpper().Contains(_audiosSearchTerm.ToUpper()))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(string.Format("FilteredGroupCollection_Filter exception {0} {1} {2}", ex.Message, ex.StackTrace, ex.Data));
                return false;
            }

        }
        #endregion

        private RelayCommand<DragEventArgs> _dropAudioCommand;
        public RelayCommand<DragEventArgs> DropAudioCommand => _dropAudioCommand ?? (_dropAudioCommand = new RelayCommand<DragEventArgs>(DropAudio));
        private void DropAudio(DragEventArgs e)
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

                        _audioRepository.AddAsync(audio).ConfigureAwait(false);

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
        private void RemoveAudio(Audio audio)
        {
            try
            {
                _audios.Remove(audio);

                _audioRepository.RemoveAsync(audio).ConfigureAwait(false);
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
                    Interval = DEFAULT_REPEAT_INTERVAL,
                    IntervalEnum = IntervalEnum.Hour,
                    StartDate = TimeSpan.Zero,
                    Repeatedly = true,
                };

                Schedules.Add(scheduleViewModel);
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
                if (IsConnectSuccess == false)
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

                _audioPlaybackScheduler.Interval(interval, async () =>
                {
                    await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Action(() =>
                    {
                        try
                        {
                            var t = Task.Factory.StartNew(() =>
                            {
                                _applicationVolumeProvider.SetApplicationVolume(_targetVolume, _fadingSpeed);
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => _playerService.OpenAndPlay(audio)));
                            });

                            Task.WhenAny(t).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
                        }
                    }));
                }, intervalEnum, scheduleId, dayEnum, scheduleViewModel.Repeatedly, start);

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
        private void RemoveSchedule(ScheduleViewModel scheduleViewModel)
        {
            try
            {
                Schedules.Remove(scheduleViewModel);

                _scheduleRepository.RemoveAsync(scheduleViewModel.ConvertToSchedule()).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<Audio> _stopPlayingScheduleCommand;
        public RelayCommand<Audio> StopPlayingScheduleCommand => _stopPlayingScheduleCommand ?? (_stopPlayingScheduleCommand = new RelayCommand<Audio>(StopPlayingSchedule));
        private void StopPlayingSchedule(Audio audio)
        {
            try
            {
                _playerService.Pause();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<ScheduleViewModel> _saveScheduleCommand;
        public RelayCommand<ScheduleViewModel> SaveScheduleCommand => _saveScheduleCommand ?? (_saveScheduleCommand = new RelayCommand<ScheduleViewModel>(SaveSchedule));
        private void SaveSchedule(ScheduleViewModel scheduleViewModel)
        {
            try
            {
                _scheduleRepository.UpdateAsync(scheduleViewModel.ConvertToSchedule()).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<object> _onAppCloseCommand;
        public RelayCommand<object> OnAppCloseCommand => _onAppCloseCommand ?? (_onAppCloseCommand = new RelayCommand<object>(OnAppClose));
        private async void OnAppClose(object o)
        {
            try
            {
                UpdateConfigs().Wait();

                await SaveData();

                if (_applicationVolumeProvider != null)
                    _applicationVolumeProvider.SetApplicationVolume(100);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));

                Logger.Log.Error(e);

                UpdateConfigs().Wait();

                await SaveData();

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

        private async Task UpdateConfigs()
        {
            try
            {
                await UpdateSetting("appName", _appName);
                await UpdateSetting("tagetVolume", _targetVolume.ToString());
                await UpdateSetting("fadingSpeed", FadingSpeed.ToString());
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private async Task UpdateSetting(string key, string value)
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

        private async Task <string> GetSetting(string key)
        {
            try
            {
                return (await _settingRepository.Get(key)).Value;
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
                    IsConnectSuccess = false;

                    DisableSchedules();

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
                    IsConnectSuccess = true;

                    if (isAutoRunFired == false)
                    {
                        EnableSchedules();

                        isAutoRunFired = true;
                    }
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

        private void EnableSchedules()
        {
            foreach (var schedule in _schedules)
            {
                if (_schedules.Any(y => y.ScheduleId == schedule.ScheduleId && y.IsActive))
                {
                    StartScheduledPlayback(schedule);
                }
            }
        }

        private void DisableSchedules()
        {
            foreach (var scheduleViewModel in _schedules.Where(x => x.IsActive == true))
            {
                StopScheduledPlayback(scheduleViewModel);
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

        private async Task SaveData()
        {
            try
            {
                foreach (var schedule in _schedules)
                {
                    await _scheduleRepository.UpdateAsync(schedule.ConvertToSchedule());
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }
    }
}