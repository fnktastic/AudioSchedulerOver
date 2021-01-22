using AudioSchedulerOver.Interface;
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

        private static IPlayable _playable { get; set; }

        public IPlayable Playable => _playable;

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

        public void EnablePlaying(IPlayable playable = null)
        {
            if (_playable != null)
            {
                _playable.IsPlaying = false;
            }

            if (playable != null)
            {
                _playable = playable;

                _playable.IsPlaying = true;
            }
        }

        public void OpenAndPlay(string path, IPlayable playable = null)
        {
            try
            {
                EnablePlaying(playable);

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

        public void OpenAndPlay(AudioViewModel audio, IPlayable playable = null)
        {
            try
            {
                EnablePlaying(playable);

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
