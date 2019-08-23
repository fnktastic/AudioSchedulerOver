using AudioSchedulerOver.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Repository
{
    public interface IAudioRepository
    {
        Audio GetLast();

        IEnumerable<Audio> GetAll();

        void Add(Audio audio);
    }
    class AudioRepository : IAudioRepository
    {
        private static List<Audio> _audios = new List<Audio>();

        public AudioRepository()
        {
            _audios.Add(new Audio()
            {
                Name = "Track 1",
                FilePath = @"C:\Users\fnkta\Music\justin_hurwitz_ryan_gosling_emma_stone_-_city_of_stars_may_finally_come_true_score_saundtrek_iz_filma_la_la_lend_la_la_land_(zf.fm).mp3"
            });
        }

        public void Add(Audio audio)
        {
            _audios.Add(audio);
        }

        public IEnumerable<Audio> GetAll()
        {
            return _audios;
        }

        public Audio GetLast()
        {
            return _audios.LastOrDefault();
        }
    }
}
