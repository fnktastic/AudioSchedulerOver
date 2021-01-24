using AudioSchedulerOver.Enum;
using AudioSchedulerOver.Exceptions;
using AudioSchedulerOver.Helper;
using AudioSchedulerOver.Logging;
using AudioSchedulerOver.Model;
using AudioSchedulerOver.Repository;
using AudioSchedulerOver.Scheduler;
using AudioSchedulerOver.Service;
using AudioSession;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
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
        private readonly IAudioRepository _audioRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly ISettingRepository _settingRepository;
        private readonly IMachineRepository _machineRepository;
        private readonly ISerialQueue _serialQueue; 

        private ApplicationVolumeProvider _applicationVolumeProvider;
        private readonly Timer _uiTimer;

        private const string APP = "Y.Music";

        private const int AUTOCHECK_INTERVAL = 3;

        private readonly double autoReloadInterval = 0.5;

        private const string STARTUP_CONFIGS = "startupConfigs.txt";

        private const int DEFAULT_REPEAT_INTERVAL = 1;

        private const int DEFAULT_COUNTDOWN_UPDATE_MSEC = 1000;

        private bool isAutoRunFired = false;

        private bool loggedIn = false;

        public static int Fading_Speed = 0;

        private readonly object _locker = new object();

        #region properties
        private ObservableCollection<AudioViewModel> _audios;
        public ObservableCollection<AudioViewModel> Audios
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

        public ListCollectionView GetAudiosCollectionView(IEnumerable<AudioViewModel> audios)
        {
            return (ListCollectionView)CollectionViewSource.GetDefaultView(audios);
        }
        #endregion

        public MainViewModel(IAudioRepository audioRepository, IScheduleRepository scheduleRepository, IMachineRepository machineRepository, ISettingRepository settingRepository, ISerialQueue serialQueue)
        {
            if (File.Exists(STARTUP_CONFIGS))
            {
                var confs = File.ReadAllLines(STARTUP_CONFIGS);

                autoReloadInterval = double.Parse(confs[0], System.Globalization.NumberStyles.Any);
            }

            _audioRepository = audioRepository;
            _scheduleRepository = scheduleRepository;
            _settingRepository = settingRepository;
            _machineRepository = machineRepository;
            _serialQueue = serialQueue;

            var mediaPlayer = new MediaPlayer();

            _playerService = new PlayerService(mediaPlayer);

            _audioPlaybackScheduler = new AudioPlaybackScheduler();

            _uiTimer = new Timer()
            {
                Interval = DEFAULT_COUNTDOWN_UPDATE_MSEC
            };

            _uiTimer.Elapsed += uiTimer_Tick;

            Task.WhenAll(Init(true));
        }

        private async Task Init(bool onStartup = false)
        {
            isAutoRunFired = false;

            await _serialQueue.Enqueue(async () => await _settingRepository.Init());

            var audios = (await _serialQueue.Enqueue(async () => await _audioRepository.GetAllAsync())).Select(x => x.ConvertToAudioViewModel());
            var schedules = (await _serialQueue.Enqueue(async () => await _scheduleRepository.GetAllAsync(MachineIdGenerator.Get))).Select(x => x.ConvertToScheduleViewModel());


            Audios = new ObservableCollection<AudioViewModel>(audios);
            Schedules = new ObservableCollection<ScheduleViewModel>(schedules);

            TargetVolunme = int.Parse(await GetSetting("tagetVolume"));
            AppName = await GetSetting("appName");
            FadingSpeed = int.Parse(await GetSetting("fadingSpeed"));

            FilteredAudiosCollection = GetAudiosCollectionView(_audios);
            FilteredAudiosCollection.Filter += FilteredAudioCollection_Filter;

            try
            {
                if (loggedIn == false)
                {
                    Machine = await _serialQueue.Enqueue(async () => await _machineRepository.SignIn(MachineIdGenerator.Get));
                    loggedIn = true;
                }
            }
            catch (StationInactiveException)
            {
                Application.Current.Shutdown(-1);
            }

            InitTimers(onStartup);
        }

        private void uiTimer_Tick(object sender, EventArgs e)
        {
            var timers = SchedulerService.Instance.GetTimers();

            foreach (var schedule in _schedules)
            {
                if (timers.ContainsKey(schedule.ScheduleId))
                {
                    var timeToGo = timers[schedule.ScheduleId];

                    var newTimeSpan = timeToGo.TimeSpan -= TimeSpan.FromMilliseconds(DEFAULT_COUNTDOWN_UPDATE_MSEC);

                    if (newTimeSpan.TotalMilliseconds < 1)
                    {
                        timeToGo.TimeSpan = TimeSpan.FromHours(timeToGo.IntervalInHour);
                    }

                    if (newTimeSpan.TotalMilliseconds >= 0)
                        schedule.NextFire = newTimeSpan;
                }
            }
        }

        private void InitTimers(bool onStartup = false)
        {
            if (onStartup)
            {
                var task1 = Task.Run(async () =>
                {
                    while (true)
                    {
                        EstablishConnection();

                        await Task.Delay(TimeSpan.FromSeconds(AUTOCHECK_INTERVAL));
                    }
                });

                var task2 = Task.Run(async () =>
                {
                    while (true)
                    {
                        await SaveData();

                        await UpdateConfigs();

                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
                        {
                            await Init().ContinueWith(i =>
                            {
                                DisableSchedules();
                                EnableSchedules();
                            });
                        }));

                        await Task.Delay(TimeSpan.FromMinutes(autoReloadInterval));
                    }
                });

                _uiTimer.Start();

                Task.WhenAll(task1, task2);
            }
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
                        var audio = Audio.CreateInstnceFromPath(file).ConvertToAudioViewModel();

                        await _serialQueue.Enqueue(async () => await _audioRepository.AddAsync(audio.ConvertToAudio()));

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

        private RelayCommand<AudioViewModel> _peviewAudioCommand;
        public RelayCommand<AudioViewModel> PeviewAudioCommand => _peviewAudioCommand ?? (_peviewAudioCommand = new RelayCommand<AudioViewModel>(PeviewAudio));
        private void PeviewAudio(AudioViewModel audio)
        {
            try
            {
                _playerService.OpenAndPlay(audio, audio);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<AudioViewModel> _removeAudioCommand;
        public RelayCommand<AudioViewModel> RemoveAudioCommand => _removeAudioCommand ?? (_removeAudioCommand = new RelayCommand<AudioViewModel>(RemoveAudio));
        private async void RemoveAudio(AudioViewModel audio)
        {
            try
            {
                _audios.Remove(audio);

                await _serialQueue.Enqueue(async () => await _audioRepository.RemoveAsync(audio.ConvertToAudio()));
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private RelayCommand<AudioViewModel> _addAudioToScheduleCommand;
        public RelayCommand<AudioViewModel> AddAudioToScheduleCommand => _addAudioToScheduleCommand ?? (_addAudioToScheduleCommand = new RelayCommand<AudioViewModel>(AddAudioToSchedule));
        private void AddAudioToSchedule(AudioViewModel audio)
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
                AudioViewModel audio = scheduleViewModel.Audio;
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
                            var t = Task.Factory.StartNew(async () =>
                            {
                                await _applicationVolumeProvider.SetApplicationVolume(_targetVolume, _fadingSpeed);
                                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                                 {
                                     _playerService.OpenAndPlay(audio, scheduleViewModel);
                                 }));
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
                _audioPlaybackScheduler.KillSchedule(scheduleViewModel.ScheduleId);

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
                _playerService.OpenAndPlay(scheduleViewModel.Audio, scheduleViewModel);
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
                Schedules.Remove(scheduleViewModel);

                await _serialQueue.Enqueue(async () => await _scheduleRepository.RemoveAsync(scheduleViewModel.ConvertToSchedule()));
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
        private async void SaveSchedule(ScheduleViewModel scheduleViewModel)
        {
            try
            {
                await _serialQueue.Enqueue(async () => await _scheduleRepository.UpdateAsync(scheduleViewModel.ConvertToSchedule()));
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
                if (_applicationVolumeProvider != null)
                    _applicationVolumeProvider.ResetVolume();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));

                Logger.Log.Error(e);

                if (_applicationVolumeProvider != null)
                    _applicationVolumeProvider.ResetVolume();
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
                await _serialQueue.Enqueue(async () => await _settingRepository.Update(key, value));
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private async Task<string> GetSetting(string key)
        {
            try
            {
                return (await _serialQueue.Enqueue(async () => await _settingRepository.Get(key))).Value;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
                return string.Empty;
            }
        }

        bool resetVolume = true;
        private bool EstablishConnection()
        {
            try
            {
                if (IsProcessExist(_appName) == false)
                {
                    ErrorMessage = "Cant find the target app by given name. Check exact name of the app and try again.";
                    SuccessMessage = string.Empty;
                    IsConnectSuccess = false;

                    //DisableSchedules();

                    isAutoRunFired = false;

                    return false;
                }

                _applicationVolumeProvider = new ApplicationVolumeProvider(_appName);

                PlayerService.ApplicationVolumeProvider = _applicationVolumeProvider;

                if (_applicationVolumeProvider.IsConnected)
                {
                    if (resetVolume)
                    {
                        _applicationVolumeProvider.ResetVolume();
                        resetVolume = false;
                    }

                    float? appVolume = _applicationVolumeProvider.GetApplicationVolume();
                    SuccessMessage = string.Format("App {0} is detected", _appName);
                    ErrorMessage = string.Empty;
                    IsConnectSuccess = true;

                    if (isAutoRunFired == false)
                    {
                        //EnableSchedules();

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
                if (schedule.IsActive)
                {
                    StartScheduledPlayback(schedule);
                }
            }
        }

        private void DisableSchedules()
        {
            foreach (var scheduleViewModel in _schedules.Where(x => x.IsActive == true))
            {
                _audioPlaybackScheduler.KillSchedule(scheduleViewModel.ScheduleId);
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
                    await _serialQueue.Enqueue(async () => await _scheduleRepository.UpdateAsync(schedule.ConvertToSchedule()));
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }
    }
}