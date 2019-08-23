using AudioSchedulerOver.Model;
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

        public PlayerService(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;

            _mediaPlayer.Volume = 50;
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
    }
}
