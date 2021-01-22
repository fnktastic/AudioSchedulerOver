using AudioSchedulerOver.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Model
{
    public class Audio
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }

        public static Audio CreateInstnceFromPath(string filePath)
        {
            return new Audio()
            {
                FilePath = filePath,
                Name = Path.GetFileName(filePath),
                Id = Guid.NewGuid()
            };
        }

        public AudioViewModel ConvertToAudioViewModel()
        {
            return new AudioViewModel()
            {
                Id = this.Id,
                Name = this.Name,
                FilePath = this.FilePath
            };
        }
    }
}
