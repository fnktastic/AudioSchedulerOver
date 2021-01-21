using AudioSchedulerOver.Logging;
using AudioSchedulerOver.Model;
using AudioSchedulerOver.ViewModel;
using AudioSession;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AudioSchedulerOver.Service
{
    public class PlayerService
    {
        private readonly MediaPlayer _mediaPlayer;

        public static ApplicationVolumeProvider ApplicationVolumeProvider;

        private static ScheduleViewModel ScheduleViewModel { get; set; }

        public ScheduleViewModel GetPlayingSchedule => ScheduleViewModel;

        public PlayerService(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;

            _mediaPlayer.Volume = 50;

            _mediaPlayer.MediaEnded += _mediaPlayer_MediaEnded;

            _mediaPlayer.Changed += _mediaPlayer_Changed;
        }

        private void Close()
        {
            _mediaPlayer.Close();
        }

        public void EnablePlaying(ScheduleViewModel scheduleViewModel = null)
        {
            if (ScheduleViewModel != null)
            {
                ScheduleViewModel.IsPlaying = false;
            }

            if (scheduleViewModel != null)
            {
                ScheduleViewModel = scheduleViewModel;

                scheduleViewModel.IsPlaying = true;
            }
        }

        public void OpenAndPlay(string path, ScheduleViewModel scheduleViewModel = null)
        {
            try
            {
                EnablePlaying(scheduleViewModel);

                Close();

                var uri = new Uri(path);

                _mediaPlayer.Open(uri);

                Play();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        public void OpenAndPlay(Audio audio, ScheduleViewModel scheduleViewModel = null)
        {
            try
            {
                EnablePlaying(scheduleViewModel);

                Close();

                var uri = new Uri(audio.FilePath);

                _mediaPlayer.Open(uri);

                Play();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        private void Play()
        {
            try
            {
                _mediaPlayer.Play();
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", e.Message, e.StackTrace, e.Data));
            }
        }

        public void Pause()
        {
            _mediaPlayer.Pause();

            EnablePlaying(null);
        }

        private void _mediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            try
            {
                if (ApplicationVolumeProvider != null)
                    Task.WhenAll(ApplicationVolumeProvider.SetApplicationVolume(100, MainViewModel.Fading_Speed)).ConfigureAwait(false);

                EnablePlaying(null);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(string.Format("Application exception {0} {1} {2}", ex.Message, ex.StackTrace, ex.Data));
            }
        }

        private void _mediaPlayer_Changed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
