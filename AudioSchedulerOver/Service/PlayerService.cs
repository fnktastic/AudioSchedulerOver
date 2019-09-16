using AudioSchedulerOver.Model;
using AudioSession;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AudioSchedulerOver.Service
{
    public class PlayerService
    {
        private readonly MediaPlayer _mediaPlayer;

        public static ApplicationVolumeProvider ApplicationVolumeProvider;

        public PlayerService(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;

            _mediaPlayer.Volume = 50;

            _mediaPlayer.MediaEnded += _mediaPlayer_MediaEnded;
        }

        private void Close()
        {
            _mediaPlayer.Close();
        }

        public void OpenAndPlay(string path)
        {
            Close();

            var uri = new Uri(path);

            _mediaPlayer.Open(uri);

            Play();
        }

        public void OpenAndPlay(Audio audio)
        {
            Close();

            var uri = new Uri(audio.FilePath);
            
            _mediaPlayer.Open(uri);

            Play();
        }

        public void Play()
        {
            _mediaPlayer.Play();
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
        }

        private void _mediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            if (ApplicationVolumeProvider != null)
                ApplicationVolumeProvider.SetApplicationVolume(100);
        }
    }
}
