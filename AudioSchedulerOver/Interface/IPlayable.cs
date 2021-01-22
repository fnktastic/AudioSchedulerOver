using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Interface
{
    public interface IPlayable
    {
        bool IsPlaying { get; set; }
    }
}
