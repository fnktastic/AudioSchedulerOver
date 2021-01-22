using AudioSchedulerOver.Interface;
using AudioSchedulerOver.Model;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.ViewModel
{
    public class AudioViewModel : ViewModelBase, IPlayable
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                RaisePropertyChanged(nameof(IsPlaying));
            }
        }

        public Audio ConvertToAudio()
        {
            return new Audio()
            {
                Id = this.Id,
                Name = this.Name,
                FilePath = this.FilePath
            };
        }
    }
}
