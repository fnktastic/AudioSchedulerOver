using AudioSchedulerOver.Enum;
using AudioSchedulerOver.Model;
using AudioSchedulerOver.Repository;
using AudioSchedulerOver.Scheduler;
using AudioSchedulerOver.Service;
using AudioSession;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioSchedulerOver.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly PlayerService _playerService;
        private readonly AudioPlaybackScheduler _audioPlaybackScheduler;        
        private readonly IAudioRepository _audioRepository;

        private ApplicationVolumeProvider _applicationVolumeProvider;

        private const string APP = "Y.Music";

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

        public MainViewModel()
        {
            var mediaPlayer = new MediaPlayer();
            _playerService = new PlayerService(mediaPlayer);
            _audioPlaybackScheduler = new AudioPlaybackScheduler();
            _audioRepository = new AudioRepository();
            Audios = new ObservableCollection<Audio>(_audioRepository.GetAll());
            ScheduleViewModels = new ObservableCollection<ScheduleViewModel>();

            //var processes = Process.GetProcesses();
        }

        private void SchedulePlayback()
        {
            _audioPlaybackScheduler.IntervalInSeconds(20, 03, 30, () =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Action(() =>
                {
                    _applicationVolumeProvider.SetApplicationVolume(5);
                    var audio = _audioRepository.GetLast();
                    _playerService.OpenAndPlay(audio);
                }));
            });
        }

        private RelayCommand<DragEventArgs> _dropAudioCommand;
        public RelayCommand<DragEventArgs> DropAudioCommand => _dropAudioCommand ?? (_dropAudioCommand = new RelayCommand<DragEventArgs>(DropAudio));
        private void DropAudio(DragEventArgs e)
        {
            var drop = e.Data.GetData("FileDrop");
            if (drop != null && drop is string[])
            {
                var files = drop as string[];

                foreach (var file in files)
                {
                    var audio = Audio.CreateInstnceFromPath(file);
                    Audios.Add(audio);
                    _audioRepository.Add(audio);                    
                }
            }
        }

        private RelayCommand _connectToAppCommnd;
        public RelayCommand ConnectToAppCommnd => _connectToAppCommnd ?? (_connectToAppCommnd = new RelayCommand(ConnectToApp));
        private void ConnectToApp()
        {
            _applicationVolumeProvider = new ApplicationVolumeProvider(_appName);

            if(_applicationVolumeProvider.IsConnected)
            {
                float? appVolume = _applicationVolumeProvider.GetApplicationVolume();
                SuccessMessage = string.Format("App {0} is detected. Curent volume level is {1}", _appName, appVolume.HasValue ? appVolume.Value.ToString() : "unknown");
                ErrorMessage = string.Empty;
            }
            else
            {
                ErrorMessage = "Cant find the target app by given name. Check exact name of the app and try again.";
                SuccessMessage = string.Empty;
            }
        }

        private RelayCommand<Audio> _peviewAudioCommand;
        public RelayCommand<Audio> PeviewAudioCommand => _peviewAudioCommand ?? (_peviewAudioCommand = new RelayCommand<Audio>(PeviewAudio));
        private void PeviewAudio(Audio audio)
        {
            _playerService.OpenAndPlay(audio);
        }

        private RelayCommand<Audio> _addAudioToScheduleCommand;
        public RelayCommand<Audio> AddAudioToScheduleCommand => _addAudioToScheduleCommand ?? (_addAudioToScheduleCommand = new RelayCommand<Audio>(AddAudioToSchedule));
        private void AddAudioToSchedule(Audio audio)
        {
            ScheduleViewModels.Add(new ScheduleViewModel()
            {
                Audio = audio,
                Interval = 0,
                IntervalEnum = IntervalEnum.Second
            });
        }

        private RelayCommand<ScheduleViewModel> _startScheduledPlaybackCommand;
        public RelayCommand<ScheduleViewModel> StartScheduledPlaybackCommand => _startScheduledPlaybackCommand ?? (_startScheduledPlaybackCommand = new RelayCommand<ScheduleViewModel>(StartScheduledPlayback));
        private void StartScheduledPlayback(ScheduleViewModel scheduleViewModel)
        {

        }

        private RelayCommand<ScheduleViewModel> _playScheduleCommand;
        public RelayCommand<ScheduleViewModel> PlayScheduleCommand => _playScheduleCommand ?? (_playScheduleCommand = new RelayCommand<ScheduleViewModel>(PlaySchedule));
        private void PlaySchedule(ScheduleViewModel scheduleViewModel)
        {
            PeviewAudio(scheduleViewModel.Audio);
        }

        private RelayCommand<ScheduleViewModel> _removeScheduleCommand;
        public RelayCommand<ScheduleViewModel> RemoveScheduleCommand => _removeScheduleCommand ?? (_removeScheduleCommand = new RelayCommand<ScheduleViewModel>(RemoveSchedule));
        private void RemoveSchedule(ScheduleViewModel scheduleViewModel)
        {
            ScheduleViewModels.Remove(scheduleViewModel);
        }
    }
}